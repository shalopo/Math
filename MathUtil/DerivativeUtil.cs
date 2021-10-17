using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class DerivativeUtil
    {
        public static MathExpr Derive(MathExpr expr, MathVariable v, int num_derivatives = 1)
        {
            if (num_derivatives == 0)
            {
                return expr;
            }

            if (num_derivatives < 0)
            {
                throw new ArgumentException(nameof(num_derivatives));
            }

            expr = expr.Derive(v);

            for (int i = 1; i < num_derivatives; i++)
            {
                expr = expr.Reduce(ReduceOptions.DEFAULT);
                expr = expr.Derive(v);
            }

            return expr;
        }

    }
}
