using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyBot
{
    internal class TradeAcceptException : Exception
    {
        public TradeAcceptException()
        { }

        public TradeAcceptException(string message) : base(message) 
        { }
    }
}
