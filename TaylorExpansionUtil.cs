using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class TaylorExpansionUtil
    {
        public static MathExpr Expand(MathExpr f, VariableMathExpr v, MathExpr base_input, int num_derivatives)
        {
            try
            {
                var var_with_input = new[] { (v.Variable, base_input) };
                MathExpr sub = f;
                double factor = 1;

                var exprs = new List<MathExpr>() { MathEvalUtil.Eval(f, var_with_input) };

                for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(sub); term++)
                {
                    factor *= term;
                    sub = DerivativeUtil.Derive(sub, v);
                    exprs.Add(MathEvalUtil.Eval(sub, var_with_input) * (v - base_input).Pow(term) * ReciprocalMathExpr.Create(factor));
                }

                var taylor = AddMathExpr.Create(exprs).Reduce();
                var taylor_reduced = taylor.Reduce();
                return taylor_reduced;
            }
            catch (UndefinedMathBehavior)
            {
                return UndefinedMathExpr.Instance;
            }
        }
    }
}
