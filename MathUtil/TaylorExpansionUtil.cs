using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class TaylorExpansionUtil
    {
        public static MathExpr Expand(ExpandableMathFunctionDef f, int max_derivatives, MathVariable v, MathExpr base_input
            , int max_seconds)
        {
            var startTime = DateTime.UtcNow;
            var maxEndTime = startTime + TimeSpan.FromSeconds(max_seconds);

            var var_with_input = new[] { (v, base_input) };
            MathExpr term = f.Definition;
            double factor = 1;

            var terms = new List<MathExpr>();

            var @const = MathEvalUtil.ComplexEvalWith(term, var_with_input);
            if (!MathEvalUtil.IsZero(@const))
            {
                terms.Add(@const);
            }

            for (int i = 1; i <= max_derivatives && !MathEvalUtil.IsZero(term) && DateTime.UtcNow < maxEndTime; i++)
            {
                factor *= i;
                var derivative = DerivativeUtil.Derive(term, v).Reduce(ReduceOptions.DEFAULT);

                Console.WriteLine($"d^{i}{f.Name}/d{f.Arg}^{i} = {derivative}");
                Console.WriteLine();

                var expr = MathEvalUtil.Transform(derivative, var_with_input) * (v - base_input).Pow(i) / factor;

                var reduced_expr = expr.Reduce(ReduceOptions.DEFAULT);

                if (!MathEvalUtil.IsZero(reduced_expr))
                {
                    terms.Add(reduced_expr);
                }

                term = derivative;
            }

            // Avoiding reduction of the whole add expression in order to prevent undesired reordering

            var taylor = AddMathExpr.Create(terms);
            return taylor;
        }
    }
}
