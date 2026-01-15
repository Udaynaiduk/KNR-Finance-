namespace MoneyTrackr.Borrowers.Helpers
{
    public static class LoanCalculator
    {
        /// <summary>
        /// Calculates total interest between two dates based on interest per ₹100 per month.
        /// </summary>
        /// <param name="principal">Principal loan amount.</param>
        /// <param name="interestPer100PerMonth">Interest per ₹100 per month (e.g., 2 means ₹2 per ₹100 per month).</param>
        /// <param name="startDate">Loan start date.</param>
        /// <param name="endDate">Loan end date or payment date.</param>
        /// <returns>Total calculated interest.</returns>
        public static decimal CalculateInterest(decimal principal, decimal interestPer100PerMonth, DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("End date cannot be earlier than start date.");

            // Get months and days difference using helper
            (int totalMonths, int remainingDays) = DateHelper.CalculateFullMonths(startDate, endDate);

            // Interest for full months
            decimal interestForMonths = (principal / 100m) * interestPer100PerMonth * totalMonths;

            // Interest for remaining days (pro-rated, assuming 30-day month)
            decimal interestForDays = ((principal / 100m) * interestPer100PerMonth)/30 * remainingDays;

            // Total interest
            decimal totalInterest = interestForMonths + interestForDays;

            return Math.Round(totalInterest, 2);
        }
    }
}
