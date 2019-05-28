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

            var negative_coefficient = exprs.Aggregate(1, (agg, expr) => agg * (expr is NegateMathExpr ? -1 : 1));
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
                     select MathEvalUtil.IsNegative(exponent_reduced) ?
                        ReciprocalMathExpr.Create(PowerMathExpr.Create(@base, (-exponent_reduced).Reduce())) :
                        PowerMathExpr.Create(@base, exponent_reduced).Reduce());

            var coefficient = exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value);
            coefficient *= negative_coefficient;

            if (coefficient == 0.0)
            {
                return ExactConstMathExpr.ZERO;
            }

            var reciprocal_term = MultMathExpr.Create(exprs.OfType<ReciprocalMathExpr>().Select(r => r.Expr)).Reduce().AsMultTerm();
            {
                double new_reciprocal_coefficient;
                (coefficient, new_reciprocal_coefficient) = FractionUtil.ReduceFraction(coefficient, reciprocal_term.Coefficient);
                reciprocal_term = new MultTerm(reciprocal_term.Expr, new_reciprocal_coefficient);
            }

            exprs = exprs.Where(expr => !(expr is ReciprocalMathExpr) && !(expr is ExactConstMathExpr));

            if (!MathEvalUtil.IsOne(coefficient))
            {
                exprs = exprs.Prepend(coefficient);
            }

            var reciprocal = reciprocal_term.ToMultExpr();

            if (!MathEvalUtil.IsOne(reciprocal))
            {
                exprs = exprs.Append(ReciprocalMathExpr.Create(reciprocal));
            }

            return MultMathExpr.Create(exprs);
        }

    }
}
