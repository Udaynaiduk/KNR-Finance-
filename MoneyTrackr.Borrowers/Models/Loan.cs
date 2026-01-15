using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackr.Borrowers.Models
{
    [Table("Loans")]
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Borrower")]
        public int BorrowerId { get; set; }

        [Column(TypeName = "decimal(10,2)")]  // smaller, good for MySQL
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]   // stores interest like 7.25%
        public decimal InterestRate { get; set; }

        public DateTime StartDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PartialPayment { get; set; }

        public DateTime? PartialPaymentPaidDate { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? FullPaymentPaidDate { get; set; }


        public bool MontlyIntersetPayer { get; set; }

        public bool NonCycledMemeber { get; set; }

        public string? Notes { get; set; }

        [NotMapped]
        public DateTime EndDate
        {
            get 
            {
                if (FullPaymentPaidDate.HasValue)
                {
                    return FullPaymentPaidDate.Value;
                }
                return StartDate.AddYears(3);
            }
        
        }

        [NotMapped]
        public decimal RemainingAmount => Amount - PartialPayment;

        [NotMapped]
        public bool HasPartialPayment => PartialPayment > 0;

        [NotMapped]
        public bool IsOverThreeYears
        {
            get
            {
                // If a partial payment exists, use its paid date
                DateTime referenceDate = PartialPaymentPaidDate ?? StartDate;

                // Check if 3 years have passed from the reference date
                return DateTime.UtcNow >= referenceDate.AddYears(3);
            }
        }
    }
}
