using MoneyTrackr.Borrowers.Models;
using System.Linq.Expressions;

namespace MoneyTrackr.Borrowers.Repository
{
    public interface IRepository<T> where T : class
    {
        // Get all records
        Task<IEnumerable<T>> GetAllAsync();

        // Get record by Id
        Task<T?> GetByIdAsync(int id);

        // Find by condition (with optional filter)
        Task<IEnumerable<T>> GetByNameAsync(string name);

        // Add new record
        Task AddAsync(T entity);

        // Update existing record
        Task UpdateAsync(int id,T entity);

        // Delete record
        Task DeleteAsync(int id);

        Task<IEnumerable<T>> GetBorrowers3YearAnniversaryAsync(int month,int  year);

        Task<LoanInterestInfo> CalculateInterestAsync(int loanId);

        Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync();

        Task<(decimal TotalLoanAmount, decimal TotalInterest)> GetLoansSummaryAsync();
        
        Task<bool> AddUser(User user);

        Task<User> ValidateUser(string username, string password);
    }
}


