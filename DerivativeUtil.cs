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

        public static MathExpr Derive(MathExpr expr, MathVariable v, int num_derivatives)
        {
            var sub = expr;

            for (int i = 0; i < num_derivatives; i++)
            {
                var derivative = sub.Derive(v);
                sub = derivative.Reduce();
            }

            return sub;
        }

    }
}
