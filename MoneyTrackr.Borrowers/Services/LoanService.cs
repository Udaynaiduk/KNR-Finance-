using MoneyTrackr.Borrowers.Helpers;
using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Repository;
using System.Threading.Tasks;

namespace MoneyTrackr.Borrowers.Services
{
    public class LoanService : ILoanService
    {
        private readonly IRepository<Borrower> _borrowerRepo;
        private readonly IRepository<Loan> _loanRepo;

        public LoanService(IRepository<Borrower> borrowerRepo, IRepository<Loan> loanRepo)
        {
            _borrowerRepo = borrowerRepo;
            _loanRepo = loanRepo;
        }

        public async Task<IEnumerable<Borrower>> GetAllLoansAsync()
        {
            try
            {
                return await _borrowerRepo.GetAllAsync();
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message,ex.ErrorCode);
            }
        }

        public async Task<Borrower?> GetBorrowerWithLoansAsync(int borrowerId)
        {
            try
            {
                return await _borrowerRepo.GetByIdAsync(borrowerId) as Borrower;
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<IEnumerable<Borrower>> GetLoansByBorrowerNameAsync(string fullName)
        {
            try
            {
                return await _borrowerRepo.GetByNameAsync(fullName);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task AddLoanAsync(Borrower borrowerInput)
        {
            try
            {
                var existingBorrowers = await _borrowerRepo.GetByNameAsync(borrowerInput.FullName);
                var existingBorrower = existingBorrowers
                    .FirstOrDefault(b => b.FullName.Equals(borrowerInput.FullName, StringComparison.OrdinalIgnoreCase));

                var newLoan = borrowerInput.Loans.FirstOrDefault();
                if (newLoan == null)
                {
                    if(existingBorrower==null)
                    {
                        await _borrowerRepo.AddAsync(borrowerInput);
                        return;
                    }
                    else
                    {
                        throw new LoanServiceException("User Already Exist");
                    }
                       
                }
                if (existingBorrower == null)
                {
                    var newBorrower = new Borrower
                    {
                        FullName = borrowerInput.FullName,
                        PhoneNumber = borrowerInput.PhoneNumber,
                        Address = borrowerInput.Address,
                        Loans = borrowerInput.Loans
                    };
                    await _borrowerRepo.AddAsync(newBorrower);
                }
                else
                {    
                    foreach(var loan in borrowerInput.Loans)
                    {
                            loan.BorrowerId = existingBorrower.Id;
                            await _loanRepo.AddAsync(loan);
                    }
                }
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
            
        }

        public async Task UpdateLoanAsync(int loanId, Loan loan)
        {
            try
            {
                await _loanRepo.UpdateAsync(loanId, loan);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task DeleteLoanAsync(int loanId)
        {
            try
            {
                await _loanRepo.DeleteAsync(loanId);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task UpdateBorrowerAsync(int borrowerId, Borrower borrower)
        {
            try
            {
                await _borrowerRepo.UpdateAsync(borrowerId, borrower);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<IEnumerable<Borrower>> GetAllBorrowersWhoReached3YearsInMonthAsync(int month, int year)
        {
            try
            {
                return await _borrowerRepo.GetBorrowers3YearAnniversaryAsync(month,year);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<LoanInterestInfo> CalculateInterestAsync(int loanId)
        {
            try
            {
                return await _loanRepo.CalculateInterestAsync(loanId);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync()
        {
            try
            {
                return await _borrowerRepo.CalculateAllLoansInterestAsync();
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task DeleteBorrowerAsync(int Id)
        {
            try
            {
                await _borrowerRepo.DeleteAsync(Id);
            }
             catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message,ex.ErrorCode);
            }
        }

        public async Task<Loan> GetLoanByIdAsync(int id)
        {
            
            try
            {
                return await _loanRepo.GetByIdAsync(id) as Loan;
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<(decimal TotalLoanAmount, decimal TotalInterest)> GetAllLoansSummaryAsync()
        {
            try
            {
                return await _loanRepo.GetLoansSummaryAsync();
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }
       public async Task<bool> SignUp(User user)
        {
            try
            {
                return await _loanRepo.AddUser(user);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

       public async Task<User> SignIn(string username, string password)
        {
            try
            {
                return await _loanRepo.ValidateUser(username, password);
            }
            catch (LoanServiceException Ex)
            {
               throw new LoanServiceException(Ex.Message, Ex.ErrorCode);
            }
        }
    }
}
