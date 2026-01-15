namespace MoneyTrackr.Borrowers.Models
{
    public class LoanInterestInfo
    {
        public string? BorrowerName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestPerMonthPer100 { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public decimal ParialPayment { get; set; }

        // Partial payment period
        public int PartialMonths { get; set; }
        public int PartialDays { get; set; }
        public decimal PartialInterest { get; set; }
        public string? NewOutstandingAfterPartial { get; set; }
        // Remaining period
        public int RemainingMonths { get; set; }
        public int RemainingDays { get; set; }
        public decimal RemainingInterest { get; set; }

        public int FullThreeYearCycles { get; set; }
        public DateTime LastCycleStart { get; set; }
        public DateTime LoanStartDate { get; set; }

        // Per-cycle details
        public List<LoanCycleInfo> Cycles { get; set; } = new List<LoanCycleInfo>();
    }


}
