using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class TaylorExpansionUtil
    {
        public static MathExpr Expand(MathExpr f, VariableMathExpr v, MathExpr base_input, int num_derivatives)
        {
            var exprs = new List<MathExpr>() { MathEvalUtil.Eval(f, (v, base_input)) };

            MathExpr sub = f;
            double factor = 1;

            for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(sub); term++)
            {
                factor *= term;
                sub = DerivativeUtil.Derive(sub, v);

                exprs.Add(MultMathExpr.Create(
                    MathEvalUtil.Eval(sub, (v, base_input)),
                    v.Pow(term),
                    new ExactConstMathExpr(factor).Pow(-1)));
            }

            return AddMathExpr.Create(exprs).Reduce();
        }
    }
}
