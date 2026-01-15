using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Models;

namespace MoneyTrackr.Borrowers.Repository
{
    public class MoneyTrackrDbContext : DbContext
    {
        public MoneyTrackrDbContext(DbContextOptions<MoneyTrackrDbContext> options)
            : base(options)
        {
        }
        // DbSets for Borrowers and Loans would go here
         public DbSet<Borrower> Borrowers { get; set; }
         public DbSet<Loan> Loans { get; set; }
    }
}
