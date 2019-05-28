using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class TaylorExpansionUtil
    {
        public static MathExpr Expand(ExpandableMathFunctionDef f, int num_derivatives, MathExpr base_input)
        {
            try
            {
                var var_with_input = new[] { (MathFunctionDef.x1.Variable, base_input) };
                MathExpr sub = f.Definition;
                double factor = 1;

                var exprs = new List<MathExpr>();

                var @const = MathEvalUtil.Eval(sub, var_with_input).Reduce();
                if (!MathEvalUtil.IsZero(@const))
                {
                    exprs.Add(@const);
                }

                for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(sub); term++)
                {
                    factor *= term;
                    sub = DerivativeUtil.Derive(sub, MathFunctionDef.x1.Variable);
                    var expr = MathEvalUtil.Eval(sub, var_with_input) * (MathFunctionDef.x1 - base_input).Pow(term) / factor;
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
