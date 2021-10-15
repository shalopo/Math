using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class TaylorExpansionUtil
    {
        public static MathExpr Expand(ExpandableMathFunctionDef f, int num_derivatives, MathVariable v, MathExpr base_input)
        {
            var var_with_input = new[] { (v, base_input) };
            MathExpr derivative = f.Definition;
            double factor = 1;

            var terms = new List<MathExpr>();

            var @const = MathEvalUtil.Transform(derivative, var_with_input).Reduce(ReduceOptions.DEFAULT);
            if (!MathEvalUtil.IsZero(@const))
            {
                terms.Add(@const);
            }

            for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(derivative); term++)
            {
                factor *= term;
                derivative = DerivativeUtil.Derive(derivative, v);

                var expr = MathEvalUtil.Transform(derivative, var_with_input) * (v - base_input).Pow(term) / factor;

                var reduced_expr = expr.Reduce(ReduceOptions.DEFAULT);

                if (!MathEvalUtil.IsZero(reduced_expr))
                {
                    terms.Add(reduced_expr);
                }
            }

            // Avoiding reduction of the whole add expression in order to prevent undesired reordering

            var taylor = AddMathExpr.Create(terms);
            return taylor;
        }
    }
}
