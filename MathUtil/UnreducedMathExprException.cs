using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class UnreducedMathExprException : Exception
    {
        public UnreducedMathExprException(IEnumerable<MathExpr> terms) : 
            base($"Unsupported operation for unreduced expressions: {string.Join(", ", terms.Select(expr => expr.ToString()))}")
        {
        }
    }
}
