using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class UnreducedMathExprException : Exception
    {
        public UnreducedMathExprException(IEnumerable<MathExpr> exprs) : 
            base($"Unsupported operation for unreduced expressions: {string.Join(", ", exprs.Select(expr => expr.ToString()))}")
        {
        }
    }
}
