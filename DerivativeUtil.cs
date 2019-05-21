using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class DerivativeUtil
    {
        public static MathExpr Derive(MathExpr expr, MathVariable v) => expr.Derive(v).Reduce();

    }
}
