using MoneyTrackr.Borrowers.Models;

namespace MoneyTrackr.Borrowers.Services
{
    public interface ILoanService
    {
        Task<IEnumerable<Borrower>> GetAllLoansAsync();

        Task<Borrower?> GetBorrowerWithLoansAsync(int borrowerId);

        Task<IEnumerable<Borrower>> GetLoansByBorrowerNameAsync(string fullName);

        /// <summary>
        /// Adds a loan for an existing borrower or creates a new borrower if necessary.
        /// </summary>
        Task AddLoanAsync(Borrower borrowerInput);

        Task UpdateLoanAsync(int loanId, Loan loan);

        Task DeleteLoanAsync(int loanId);

        Task DeleteBorrowerAsync(int Id);

        Task UpdateBorrowerAsync(int borrowerId, Borrower borrower);

        /// <summary>
        /// Retrieves borrowers who have reached 3 years of loan duration in a specific month.
        /// </summary>
        Task<IEnumerable<Borrower>> GetAllBorrowersWhoReached3YearsInMonthAsync(int month,int year);

        Task<LoanInterestInfo> CalculateInterestAsync(int loanId);

        Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync();

        Task<Loan> GetLoanByIdAsync(int id);

        Task<(decimal TotalLoanAmount, decimal TotalInterest)> GetAllLoansSummaryAsync();
    }
}
