using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class UndefinedMathBehavior : Exception
    {
        public UndefinedMathBehavior(string message) : base(message) { }
        public UndefinedMathBehavior(string message, Exception innerException) : base(message, innerException) { }
    }
}
