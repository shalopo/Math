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

                var exprs = new List<MathExpr>();

                var @const = MathEvalUtil.Eval(f, var_with_input).Reduce();
                if (!MathEvalUtil.IsZero(@const))
                {
                    exprs.Add(@const);
                }

                for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(sub); term++)
                {
                    factor *= term;
                    sub = DerivativeUtil.Derive(sub, v);
                    var expr = MathEvalUtil.Eval(sub, var_with_input) * (v - base_input).Pow(term) / factor;
                    var reduced_expr = expr.Reduce();

                    if (!MathEvalUtil.IsZero(reduced_expr))
                    {
                        exprs.Add(reduced_expr);
                    }
                }

                // Avoiding reduction of the whole add expression in order to prevent undesired reordering

                var taylor = AddMathExpr.Create(exprs);
                return taylor;
            }
            catch (UndefinedMathBehavior)
            {
                return UndefinedMathExpr.Instance;
            }
        }
    }
}
