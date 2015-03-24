using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyBot.OfferValidators
{
    public class SwapValidationRule
    {
        public HashSet<string> MyItems { get; set; }
        public HashSet<string> TheirItems { get; set; }
        public decimal Price { get; set; }
    }
}
