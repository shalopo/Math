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

            var exprs = new List<MathExpr>();

            var @const = MathEvalUtil.EvalTransformVariables(derivative, var_with_input).Reduce();
            if (!MathEvalUtil.IsZero(@const))
            {
                exprs.Add(@const);
            }

            for (int term = 1; term <= num_derivatives && !MathEvalUtil.IsZero(derivative); term++)
            {
                factor *= term;
                derivative = DerivativeUtil.Derive(derivative, v);

                Console.WriteLine();
                Console.WriteLine($"d^{term}(f)/dx^{term}  = {derivative}");

                var expr = MathEvalUtil.EvalTransformVariables(derivative, var_with_input) * (v - base_input).Pow(term) / factor;

                //Console.WriteLine();
                //Console.WriteLine($"taylor term {term}: {expr}");

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
    }
}
