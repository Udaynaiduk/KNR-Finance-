namespace MoneyTrackr.Borrowers.Services
{
        public class LoanServiceException : Exception
        {
            public int ErrorCode { get; set; }

            public LoanServiceException(string message, int errorCode = 500)
                : base(message)
            {
                ErrorCode = errorCode;
            }

            public LoanServiceException(string message, Exception innerException, int errorCode = 500)
                : base(message, innerException)
            {
                ErrorCode = errorCode;
            }
        }
 }
