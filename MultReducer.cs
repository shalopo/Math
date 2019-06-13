using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class MultReducer
    {
        public static MathExpr Reduce(IEnumerable<MathExpr> exprs)
        {
            exprs = (from expr in exprs select expr.Reduce());

            exprs = (from expr in exprs select expr is MultMathExpr mult_expr ? mult_expr.Exprs : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            var negative_coefficient = exprs.Aggregate(1, (agg, expr) => expr is NegateMathExpr ? -agg : agg);
            exprs = (from expr in exprs select expr is NegateMathExpr negate ? negate.Expr : expr);

            var powers = new Dictionary<MathExpr, MathExpr>();
            foreach (var expr in exprs)
            {
                var pow = expr.AsPowerExpr();
                if (powers.ContainsKey(pow.Base))
                {
                    //TODO: input range validity: x/x is 1 for x!=0
                    powers[pow.Base] += pow.Exponent;
                }
                else
                {
                    powers.Add(pow.Base, pow.Exponent);
                }
            }

            exprs = (from item in powers
                     let @base = item.Key
                     let exponent_reduced = item.Value.Reduce()
                     select MathEvalUtil.IsPositive(exponent_reduced) ?
                        PowerMathExpr.Create(@base, exponent_reduced).Reduce() :
                        ReciprocalMathExpr.Create(PowerMathExpr.Create(@base, (-exponent_reduced).Reduce())));

            var coefficient = NumericalConstMathExpr.Mult(exprs.OfType<NumericalConstMathExpr>().Append(negative_coefficient));

            if (MathEvalUtil.IsZero(coefficient))
            {
                return ExactConstMathExpr.ZERO;
            }

            exprs = exprs.Where(expr => !(expr is NumericalConstMathExpr));

            if (!coefficient.Equals(ExactConstMathExpr.ONE))
            {
                exprs = exprs.Prepend(coefficient);
            }

            return MultMathExpr.Create(exprs);
        }

    }
}
