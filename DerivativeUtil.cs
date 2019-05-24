using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class DerivativeUtil
    {
        public static MathExpr Derive(MathExpr expr, MathVariable v, int num_derivatives = 1)
        {
            var sub = expr;

            for (int i = 0; i < num_derivatives; i++)
            {
                sub = sub.Derive(v).Reduce();
            }

            return sub;
        }

    }
}
