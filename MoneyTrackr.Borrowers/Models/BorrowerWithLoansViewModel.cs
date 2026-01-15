using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.Borrowers.ViewModels
{
    public class BorrowerWithLoansViewModel
    {
        // Borrower info
        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        // Multiple loans
        public List<LoanViewModel> Loans { get; set; } = new();
    }

    public class LoanViewModel
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public decimal PartialPayment { get; set; } = 0.0m;

        public DateTime? PartialPaymentPaidDate { get; set; }

        public bool MontlyIntersetPayer { get; set; } = false;

        public bool NonCycledMemeber { get; set; } = false;

        public string? Notes { get; set; }
    }
}
