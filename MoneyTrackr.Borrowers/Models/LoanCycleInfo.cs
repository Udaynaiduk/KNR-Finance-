using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrackr.Borrowers.Models
{
    public class LoanCycleInfo
    {
        public int CycleNumber { get; set; }          // 1, 2, 3…
        public DateTime CycleStart { get; set; }
        public DateTime CycleEnd { get; set; }
        public decimal PrincipalStart { get; set; }   // Principal at start of cycle
        public decimal Interest { get; set; }         // Interest for this cycle
        public decimal PrincipalEnd { get; set; }     // Principal after adding interest
    }
}
