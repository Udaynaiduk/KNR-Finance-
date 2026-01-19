using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.Borrowers.Helpers;
using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Services;
using MoneyTrackr.Borrowers.ViewModels;
using MoneyTrackrView.Models;
using System.Diagnostics;
using System.Security.Claims;


namespace MoneyTrackrView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILoanService _loanService;
        

        public HomeController(ILogger<HomeController> logger, ILoanService loanService)
        {
            _logger = logger;
            _loanService = loanService;
           
        }

        [Authorize(Roles ="Admin")]
        #region Basic Pages
        public async Task<IActionResult> Index(int month, int year)
        {
            try
            {
                var graph = await _loanService.GetAllLoansSummaryAsync();
                ViewBag.TotalAmount = graph.TotalLoanAmount;
                ViewBag.TotalInterest = graph.TotalInterest;
                if (year == 0)
                {
                    year = DateTime.Now.Year;
                }
                if (month == 0)
                {
                    month = DateTime.Now.Month;
                }
                var loans = await _loanService.GetAllBorrowersWhoReached3YearsInMonthAsync(month, year);

                ViewBag.SelectedMonth = month;
                ViewBag.Selectedyear = year;

                return View(loans);
            }
            catch (Exception ex)
            {

                return RedirectToAction("Error", new { message = ex.Message });
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            ViewBag.Error = message ?? "Error";
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region Borrower & Loan Views

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            try
            {
                var borrowers = await _loanService.GetAllLoansAsync();
                return View(borrowers);
            }
            catch (Exception ex)
            {

                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        // GET: /Borrower/ById/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetBorrowerById(int id)
        {
            try
            {
                var borrower = await _loanService.GetBorrowerWithLoansAsync(id);
                if (borrower == null)
                {
                    TempData["ErrorMessage"] = "Borrower not found.";
                    return RedirectToAction("BorrowersDashboard");
                }

                return View(borrower);
            }
            catch (Exception ex)
            {

                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        #endregion

        #region Loan Edit / Partial / Close

        // GET: /Borrower/EditLoan/5?mode=edit|partial|close
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditLoan(int? id, string mode = "edit")
        {
            try
            {
                if (id == null)
                    return BadRequest();

                var loan = await _loanService.GetLoanByIdAsync(id.Value);
                if (loan == null)
                {
                    TempData["ErrorMessage"] = "Loan not found.";
                    return RedirectToAction("GetAllLoans");
                }

                string[] validModes = new[] { "edit", "partial", "close" };
                if (!validModes.Contains(mode)) mode = "edit";

                ViewBag.Mode = mode;
                return View(loan);
            }
            catch (Exception ex)
            {

                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        // POST: /Borrower/EditLoan
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoan(int? id, string mode, Loan loan)
        {
            try
            {
                if (id == null || loan == null)
                    return BadRequest();

                string[] validModes = new[] { "edit", "partial", "close" };
                if (!validModes.Contains(mode)) mode = "edit";
                ViewBag.Mode = mode;

                // Apply business logic based on mode
                if (mode == "partial" && loan.PartialPayment > 0)
                {
                    loan.PartialPaymentPaidDate ??= DateTime.Now;
                }
                else if (mode == "close")
                {
                    loan.IsPaid = true;
                    loan.FullPaymentPaidDate ??= DateTime.Now;
                }
                await _loanService.UpdateLoanAsync(id.Value, loan);

                return RedirectToAction("GetBorrowerById", new { id = loan.BorrowerId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        #endregion

        #region Interest Calculations

        // GET: /Borrower/Interest/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetInterestFor(int id)
        {
            try
            {
                var loanInfo = await _loanService.CalculateInterestAsync(id);
                return View(loanInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        // GET: /Borrower/AllLoansInterest
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllLoansInterest()
        {
            try
            {
                var loansInterest = await _loanService.CalculateAllLoansInterestAsync();
                return View(loansInterest);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithLoan(BorrowerWithLoansViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Map ViewModel to Borrower entity
                var borrower = new Borrower
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Loans = model.Loans.Select(l => new Loan
                    {
                        Amount = l.Amount,
                        InterestRate = l.InterestRate,
                        StartDate = l.StartDate,
                        PartialPayment = l.PartialPayment,
                        PartialPaymentPaidDate = l.PartialPaymentPaidDate,
                        MontlyIntersetPayer = l.MontlyIntersetPayer,
                        NonCycledMemeber = l.NonCycledMemeber,
                        Notes = l.Notes,
                        IsPaid = false
                    }).ToList()
                };

                await _loanService.AddLoanAsync(borrower);

                return RedirectToAction(nameof(BorrowersDashboard));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateWithLoan()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddLoan(int borrowerId)
        {
            try
            {
                var borrower = await _loanService.GetBorrowerWithLoansAsync(borrowerId);
                if (borrower == null) return NotFound();

                ViewBag.BorrowerFullName = borrower.FullName;
                ViewBag.BorrowerPhoneNumber = borrower.PhoneNumber;
                ViewBag.BorrowerAddress = borrower.Address;
                ViewBag.BorrowerId = borrower.Id;

                // Start with 1 empty loan by default
                var model = new List<Loan> { new Loan() };

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddLoans(List<Loan> loans)
        {
            try
            {
                if (loans == null || loans.Count == 0) return BadRequest("No loans submitted.");

                var borrower = await _loanService.GetBorrowerWithLoansAsync(loans.First().BorrowerId);
                if (ModelState.IsValid)
                {
                    await _loanService.AddLoanAsync(new Borrower
                    {
                        Id = borrower.Id,
                        FullName = borrower.FullName,
                        PhoneNumber = borrower.PhoneNumber,
                        Address = borrower.Address,
                        Loans = loans
                    });
                    return RedirectToAction("GetBorrowerById", new { id = borrower.Id });
                }
                // If invalid, reload the borrower details    
                ViewBag.BorrowerFullName = borrower.FullName;
                ViewBag.BorrowerPhoneNumber = borrower.PhoneNumber;
                ViewBag.BorrowerAddress = borrower.Address;

                return View("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteLoan(int id, int BorrowerId)
        {
            try
            {

                await _loanService.DeleteLoanAsync(id);
                return RedirectToAction("GetBorrowerById", new { id = BorrowerId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> BorrowersDashboard()
        {
            try
            {
                var borrowers = await _loanService.GetAllLoansAsync();
                return View(borrowers);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteBorrower(int id)
        {
            try
            {
                await _loanService.DeleteBorrowerAsync(id);
                return RedirectToAction("BorrowersDashboard");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetLoanById(int id)
        {
            try
            {
                var loan = await _loanService.GetLoanByIdAsync(id);
                return View(loan);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

       
        [HttpGet]
        public IActionResult InterestCalculator()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InterestCalculator(decimal principal, decimal interestPer100PerMonth, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                ViewBag.Error = "End date cannot be earlier than start date.";
                return View();
            }

            try
            {
                var interest = LoanCalculator.CalculateInterest(principal, interestPer100PerMonth, startDate, endDate);
                ViewBag.Interest = interest;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> SearchBorrower([FromQuery(Name = "query")] string search)
        {
            try
            {
                var Borrower = await _loanService.GetLoansByBorrowerNameAsync(search);
                return View(Borrower);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }

        }

        // GET: /Account/SignUp
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: /Account/SignUp
        [HttpPost]
        public async Task<IActionResult> SignUp(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                var newuser = new User
                {
                    Username = user.Username,
                    Password = user.Password,
                    Email = user.Email,
                    Role = "User"
                };
                var result = await _loanService.SignUp(newuser);
                if (result)
                    return RedirectToAction("SignIn"); // Redirect to login after successful signup

                ModelState.AddModelError("", "Sign up failed. Try again.");
            }
            catch (LoanServiceException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(user);
        }

        // GET: /Account/SignIn
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        // POST: /Account/SignIn
    [HttpPost]
    public async Task<IActionResult> SignIn(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Username and password are required.");
            return View();
        }

        try
        {
            var userValid = await _loanService.SignIn(username,password);

            if (userValid !=null)
            {
                // Create claims
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userValid.Username),
                new Claim(ClaimTypes.Email, userValid.Email),
                new Claim(ClaimTypes.Role, userValid.Role ?? "User") // Default role
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Remember login across sessions
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
                    if (userValid.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("InterestCalculator", "Home");
                    }
            }

            ModelState.AddModelError("", "Invalid username or password.");
        }
        catch (LoanServiceException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        return View();
    }
        
       

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditBorrower(Borrower borrower)
        {
            if (!ModelState.IsValid)
            {
                return View(borrower);
            }
            try
            {

                await _loanService.UpdateBorrowerAsync(borrower.Id, borrower);
                return RedirectToAction("GetBorrowerById", new { id = borrower.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }

        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> EditBorrower(int id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var borrower = await _loanService.GetBorrowerWithLoansAsync(id);
            return View(borrower);
        }

        #endregion
    }
}

