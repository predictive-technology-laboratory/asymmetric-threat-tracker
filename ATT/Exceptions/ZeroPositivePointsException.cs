using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTL.ATT.Exceptions
{
    public class ZeroPositivePointsException : Exception
    {
        public ZeroPositivePointsException(string message = "")
            : base(message)
        {
        }
    }
}
