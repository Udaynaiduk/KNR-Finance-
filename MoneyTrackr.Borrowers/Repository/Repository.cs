using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Helpers;
using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Services; // For LoanServiceException

namespace MoneyTrackr.Borrowers.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MoneyTrackrDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(MoneyTrackrDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Get all entities
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    return await _dbSet
                        .Include("Loans")
                        .ToListAsync();
                }

                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve all entities.", ex, 500);
            }
        }

        // Get by ID
        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    var borrower = await _dbSet.OfType<Borrower>()
                        .Include(b => b.Loans)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (borrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    return borrower as T;
                }

                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                    throw new LoanServiceException($"Entity with id {id} not found.", 404);

                return entity;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve entity by ID.", ex, 500);
            }
        }

        // Get by name (for Borrowers only)
        public async Task<IEnumerable<T>> GetByNameAsync(string name)
        {
            try
            {
                // If name is null, empty, or whitespace, return empty list
                if (string.IsNullOrWhiteSpace(name))
                    return Enumerable.Empty<T>();

                // Query borrowers matching the name
                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .Where(b => EF.Functions.Like(b.FullName, $"%{name.Trim()}%"))
                    .ToListAsync();

                return borrowers.Cast<T>();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve borrowers by name.", ex, 500);
            }
        }


        // Add entity
        public async Task AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to add entity.", ex, 500);
            }
        }

        // Update entity by ID
        public async Task UpdateAsync(int id, T entity)
        {
            try
            {
                if (typeof(T) == typeof(Loan))
                {
                    var updatedLoan = entity as Loan ?? throw new LoanServiceException("Entity must be a Loan.", 400);

                    var existingLoan = await _context.Loans.FindAsync(id);
                    if (existingLoan == null)
                        throw new LoanServiceException($"Loan with id {id} not found.", 404);

                    if (updatedLoan.Amount != default(decimal))
                        existingLoan.Amount = updatedLoan.Amount;
                    if (updatedLoan.InterestRate != default(decimal))
                        existingLoan.InterestRate = updatedLoan.InterestRate;
                    if (updatedLoan.StartDate != default(DateTime))
                        existingLoan.StartDate = updatedLoan.StartDate;
                    if (updatedLoan.PartialPaymentPaidDate != default(DateTime))
                        existingLoan.PartialPaymentPaidDate = updatedLoan.PartialPaymentPaidDate;
                    if (updatedLoan.PartialPayment != default(decimal))
                        existingLoan.PartialPayment = updatedLoan.PartialPayment;

                    existingLoan.IsPaid = updatedLoan.IsPaid;
                }
                else if (typeof(T) == typeof(Borrower))
                {
                    var updatedBorrower = entity as Borrower ?? throw new LoanServiceException("Entity must be a Borrower.", 400);

                    var existingBorrower = await _context.Borrowers.FindAsync(id);
                    if (existingBorrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    if (!string.IsNullOrWhiteSpace(updatedBorrower.FullName))
                        existingBorrower.FullName = updatedBorrower.FullName;
                    if (!string.IsNullOrWhiteSpace(updatedBorrower.PhoneNumber))
                        existingBorrower.PhoneNumber = updatedBorrower.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(updatedBorrower.Address))
                        existingBorrower.Address = updatedBorrower.Address;

                    if (updatedBorrower.Loans != null && updatedBorrower.Loans.Count > 0)
                    {
                        foreach (var loanDto in updatedBorrower.Loans)
                        {
                            var loan = await _context.Loans.FindAsync(loanDto.Id);
                            if (loan != null)
                            {
                                if (loanDto.Amount != default(decimal))
                                    loan.Amount = loanDto.Amount;
                                if (loanDto.InterestRate != default(decimal))
                                    loan.InterestRate = loanDto.InterestRate;
                                if (loanDto.StartDate != default(DateTime))
                                    loan.StartDate = loanDto.StartDate;
                                if (loanDto.PartialPayment != default(decimal))
                                    loan.PartialPayment = loanDto.PartialPayment;
                                if (loanDto.PartialPaymentPaidDate != default(DateTime))
                                    loan.PartialPaymentPaidDate = loanDto.PartialPaymentPaidDate;

                                loan.IsPaid = loanDto.IsPaid;
                            }
                        }
                    }
                }
                else
                {
                    throw new LoanServiceException("UpdateAsync is only valid for Loan or Borrower DTOs.", 400);
                }

                await _context.SaveChangesAsync();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to update entity.", ex, 500);
            }
        }

        // Delete entity by ID
        public async Task DeleteAsync(int id)
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    var borrower = await _context.Borrowers
                        .Include(b => b.Loans)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (borrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    _context.Loans.RemoveRange(borrower.Loans);
                    _context.Borrowers.Remove(borrower);
                }
                else
                {
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                        throw new LoanServiceException($"Entity with id {id} not found.", 404);

                    _dbSet.Remove(entity);
                }

                await _context.SaveChangesAsync();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to delete entity.", ex, 500);
            }
        }

        // Get borrowers who reached 3-year anniversary in N months
        public async Task<IEnumerable<T>> GetBorrowers3YearAnniversaryAsync(int month, int year)
        {
            if (typeof(T) != typeof(Borrower))
            {
                throw new LoanServiceException("GetBorrowers3YearAnniversaryAsync is only valid for Borrower entities.", 400);
            }


            try
            {
                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .Select(b => new Borrower
                    {
                        Id = b.Id,
                        FullName = b.FullName,
                        PhoneNumber = b.PhoneNumber,
                        Address = b.Address,
                        Loans = b.Loans
                            .Where(loan =>
                                !loan.IsPaid &&
                                // loan completing 3 years between today and windowEnd
                                loan.StartDate.AddYears(3).Month == month
                               && loan.StartDate.AddYears(3).Year == year
                            )
                            .ToList()
                    })
                    .Where(b => b.Loans.Any())
                    .ToListAsync();

                return borrowers.Cast<T>();
            }
            catch (Exception ex)
            {
                throw new LoanServiceException(
                    "Failed to retrieve borrowers completing 3-year loans.",
                    ex,
                    500);
            }
        }

        public async Task<LoanInterestInfo> CalculateInterestAsync(int loanId)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                    throw new LoanServiceException($"Loan with ID {loanId} not found.", 404);

                var borrower = await _context.Borrowers.FindAsync(loan.BorrowerId);
                if (borrower == null)
                    throw new LoanServiceException("Borrower not found for this loan.", 404);

                DateTime now = DateTime.UtcNow;
                decimal principal = loan.Amount;
                decimal partialInterest = 0;
                decimal remainingInterest = 0;
                int partialMonths = 0, partialDays = 0;
                int remainingMonths = 0, remainingDays = 0;
                int fullCycles = 0;

                var loanInfo = new LoanInterestInfo();

                // ===================== Skip Interest for Monthly Payers =====================
                if (loan.MontlyIntersetPayer)
                {
                    loanInfo.BorrowerName = borrower.FullName;
                    loanInfo.PrincipalAmount = loan.Amount;
                    loanInfo.InterestPerMonthPer100 = loan.InterestRate;
                    loanInfo.LoanStartDate = loan.StartDate;
                    loanInfo.ParialPayment = loan.PartialPayment;
                    loanInfo.InterestRate = loan.InterestRate;
                    loanInfo.TotalInterest = 0;
                    loanInfo.TotalPayableAmount = principal - loan.PartialPayment;
                    return loanInfo;
                }

                // Interest calculation start
                DateTime interestStartDate = loan.StartDate;

                // ===================== Partial Payment =====================
                if (loan.PartialPayment > 0 && loan.PartialPaymentPaidDate.HasValue)
                {
                    partialInterest = LoanCalculator.CalculateInterest(
                        principal,
                        loan.InterestRate,
                        loan.StartDate,
                        loan.PartialPaymentPaidDate.Value
                    );

                    (partialMonths, partialDays) = DateHelper.CalculateFullMonths(
                        loan.StartDate,
                        loan.PartialPaymentPaidDate.Value
                    );

                    principal = principal + partialInterest - loan.PartialPayment;
                    interestStartDate = loan.PartialPaymentPaidDate.Value;
                }

                // ===================== Full 3-Year Compounding Cycles =====================
                if (!loan.NonCycledMemeber) // Only apply cycles if member is cycled
                {
                    DateTime cycleStart = interestStartDate;
                    int cycleNumber = 1;

                    while (cycleStart.AddYears(3) <= now)
                    {
                        DateTime cycleEnd = cycleStart.AddYears(3);

                        decimal cycleInterest = LoanCalculator.CalculateInterest(
                            principal,
                            loan.InterestRate,
                            cycleStart,
                            cycleEnd
                        );

                        var cycleInfo = new LoanCycleInfo
                        {
                            CycleNumber = cycleNumber,
                            CycleStart = cycleStart,
                            CycleEnd = cycleEnd,
                            PrincipalStart = principal,
                            Interest = cycleInterest,
                            PrincipalEnd = principal + cycleInterest
                        };

                        loanInfo.Cycles.Add(cycleInfo);

                        principal += cycleInterest; // compound
                        cycleStart = cycleEnd;
                        fullCycles++;
                        cycleNumber++;
                    }

                    // ===================== Remaining Period (<3 Years) =====================
                    if (cycleStart < now)
                    {
                        remainingInterest = LoanCalculator.CalculateInterest(
                            principal,
                            loan.InterestRate,
                            cycleStart,
                            now
                        );

                        (int remMonths, int remDays) = DateHelper.CalculateFullMonths(cycleStart, now);
                        remainingMonths += remMonths;
                        remainingDays += remDays;
                    }
                }
                else
                {
                    // For NonCycledMember, calculate simple interest from interestStartDate to now
                    remainingInterest = LoanCalculator.CalculateInterest(
                        principal,
                        loan.InterestRate,
                        interestStartDate,
                        now
                    );

                    (remainingMonths, remainingDays) = DateHelper.CalculateFullMonths(interestStartDate, now);
                }

                decimal totalPayable = principal + remainingInterest;

                // ===================== Fill DTO =====================
                loanInfo.BorrowerName = borrower.FullName;
                loanInfo.PrincipalAmount = loan.Amount;
                loanInfo.ParialPayment = loan.PartialPayment;
                loanInfo.InterestRate = loan.InterestRate;

                loanInfo.PartialMonths = partialMonths;
                loanInfo.PartialDays = partialDays;
                loanInfo.PartialInterest = Math.Round(partialInterest, 2);
                loanInfo.NewOutstandingAfterPartial = $"Principal = {principal:0.00} + Partial Interest = {partialInterest:0.00} - Partial Payment = {loan.PartialPayment:0.00} = {principal:0.00}";

                loanInfo.RemainingMonths = remainingMonths;
                loanInfo.RemainingDays = remainingDays;
                loanInfo.RemainingInterest = Math.Round(remainingInterest, 2);

                loanInfo.TotalInterest = Math.Round(partialInterest + remainingInterest, 2);
                loanInfo.TotalPayableAmount = Math.Round(totalPayable, 2);

                loanInfo.FullThreeYearCycles = fullCycles;
                loanInfo.LastCycleStart = interestStartDate;
                loanInfo.LoanStartDate = loan.StartDate;

                return loanInfo;
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to calculate interest for the loan.", ex, 500);
            }
        }



        // Calculate interest for all loans
        public async Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync()
        {
            try
            {
                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .ToListAsync();

                var result = new List<LoanInterestInfo>();
                DateTime now = DateTime.UtcNow;

                foreach (var borrower in borrowers)
                {
                    foreach (var loan in borrower.Loans)
                    {
                        if (loan.IsPaid)
                            continue;

                        decimal principal = loan.Amount;
                        decimal partialInterest = 0;
                        decimal remainingInterest = 0;
                        int partialMonths = 0, partialDays = 0;
                        int remainingMonths = 0, remainingDays = 0;
                        int fullCycles = 0;

                        DateTime startDate = loan.StartDate;

                        // Store cycle details
                        var cycles = new List<LoanCycleInfo>();

                        // 0️⃣ Handle Monthly Interest Payers: skip all interest
                        if (loan.MontlyIntersetPayer)
                        {
                            result.Add(new LoanInterestInfo
                            {
                                BorrowerName = borrower.FullName,
                                PrincipalAmount = loan.Amount,
                                ParialPayment = loan.PartialPayment,
                                InterestRate = loan.InterestRate,
                                InterestPerMonthPer100 = loan.InterestRate,
                                PartialInterest = 0,
                                RemainingInterest = 0,
                                TotalInterest = 0,
                                TotalPayableAmount = loan.Amount - loan.PartialPayment,
                                PartialMonths = 0,
                                PartialDays = 0,
                                RemainingMonths = 0,
                                RemainingDays = 0,
                                FullThreeYearCycles = 0,
                                LastCycleStart = startDate,
                                Cycles = cycles
                            });
                            continue;
                        }

                        // 1️⃣ Partial payment
                        if (loan.PartialPayment > 0 && loan.PartialPaymentPaidDate.HasValue)
                        {
                            partialInterest = LoanCalculator.CalculateInterest(
                                principal,
                                loan.InterestRate,
                                startDate,
                                loan.PartialPaymentPaidDate.Value
                            );

                            (partialMonths, partialDays) = DateHelper.CalculateFullMonths(startDate, loan.PartialPaymentPaidDate.Value);

                            principal = principal + partialInterest - loan.PartialPayment;
                            startDate = loan.PartialPaymentPaidDate.Value;
                        }

                        // 2️⃣ Full 3-year cycles (skip if NonCycledMember)
                        DateTime cycleStart = startDate;
                        if (!loan.NonCycledMemeber)
                        {
                            while (cycleStart.AddYears(3) <= now)
                            {
                                DateTime cycleEnd = cycleStart.AddYears(3);

                                decimal cycleInterest = LoanCalculator.CalculateInterest(
                                    principal,
                                    loan.InterestRate,
                                    cycleStart,
                                    cycleEnd
                                );

                                cycles.Add(new LoanCycleInfo
                                {
                                    CycleNumber = fullCycles + 1,
                                    CycleStart = cycleStart,
                                    CycleEnd = cycleEnd,
                                    PrincipalStart = principal,
                                    Interest = Math.Round(cycleInterest, 2),
                                    PrincipalEnd = principal + cycleInterest
                                });

                                principal += cycleInterest;
                                cycleStart = cycleEnd;
                                fullCycles++;
                            }
                        }

                        // 3️⃣ Remaining period (<3 Years) or for NonCycledMember
                        if (cycleStart < now)
                        {
                            remainingInterest = LoanCalculator.CalculateInterest(
                                principal,
                                loan.InterestRate,
                                cycleStart,
                                now
                            );

                            (remainingMonths, remainingDays) = DateHelper.CalculateFullMonths(cycleStart, now);
                        }

                        decimal totalPayable = principal + remainingInterest;

                        result.Add(new LoanInterestInfo
                        {
                            BorrowerName = borrower.FullName,
                            PrincipalAmount = loan.Amount,
                            ParialPayment = loan.PartialPayment,
                            InterestRate = loan.InterestRate,
                            InterestPerMonthPer100 = loan.InterestRate,
                            PartialMonths = partialMonths,
                            PartialDays = partialDays,
                            PartialInterest = Math.Round(partialInterest, 2),
                            RemainingMonths = remainingMonths,
                            RemainingDays = remainingDays,
                            RemainingInterest = Math.Round(remainingInterest, 2),
                            TotalInterest = Math.Round(partialInterest + remainingInterest, 2),
                            TotalPayableAmount = Math.Round(totalPayable, 2),
                            FullThreeYearCycles = fullCycles,
                            LastCycleStart = cycleStart,
                            Cycles = cycles
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Unexpected error while calculating interest for all loans.", ex, 500);
            }
        }

        public async Task<(decimal TotalLoanAmount, decimal TotalInterest)> GetLoansSummaryAsync()
        {
            var allLoans = await CalculateAllLoansInterestAsync();

            decimal totalLoanAmount = allLoans.Sum(l => l.PrincipalAmount);
            decimal totalInterest = allLoans.Sum(l => l.TotalInterest);

            return (totalLoanAmount, totalInterest);
        }

        public async Task<bool> AddUser(User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (existingUser == null)
            {
                var passwordHasher = new PasswordHasher<object>();
                var newUser = new User
                {
                    Username = user.Username.ToLower(),
                    Email = user.Email.ToLower(),
                    Password = passwordHasher.HashPassword(null, user.Password),
                    Role = user.Role
                };

                await _context.Users.AddAsync(newUser);  // <-- use newUser here
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new LoanServiceException("Username already exists.", 400);
            }
        }


        public async Task<User> ValidateUser(string username, string password)
        {
            try
            {
                // 1. Find the user by username (case-insensitive)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.Equals(username.ToLower()));

                if (user == null)
                {
                    throw new LoanServiceException("Invalid username", 401);
                }

                // 2. Create a PasswordHasher
                var passwordHasher = new PasswordHasher<object>();

                // 3. Verify the password
                var result = passwordHasher.VerifyHashedPassword(null, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    return user; // Valid login
                }

                throw new LoanServiceException("Invalid password", 401);
            }
            catch (LoanServiceException)
            {
                throw; // Rethrow known exceptions
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Error validating user", ex, 500);
            }
        }



    }

}
