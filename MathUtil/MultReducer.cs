using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class MultReducer
    {
        public static MathExpr Reduce(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            terms = Flatten(terms);
            terms = ReduceTerms(terms, options);

            int sign;
            (terms, sign) = GetUnsignedTerms(terms);

            //TODO: Simplify as in AddReducer

            Dictionary<MathExpr, List<MathExpr>> powers = MapPowers(terms);

            terms = (from item in powers
                     let @base = item.Key
                     let exponent = AddMathExpr.Create(item.Value).Reduce(ReduceOptions.DEFAULT)
                     select PowerMathExpr.Create(@base, exponent).Reduce(ReduceOptions.DEFAULT));

            terms = (from expr in terms select expr is MultMathExpr mult_expr ? mult_expr.Terms : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            var coefficient = NumericalConstMathExpr.Mult(terms.OfType<NumericalConstMathExpr>());

            if (MathEvalUtil.IsZero(coefficient))
            {
                return GlobalMathDefs.ZERO;
            }

            terms = terms.Where(expr => !(expr is NumericalConstMathExpr));

            //slupu: const * i => ConstComplex

            if (sign < 0)
            {
                coefficient = coefficient.Negate();
            }

            if (!MathEvalUtil.IsOne(coefficient))
            {
                terms = terms.Prepend(coefficient);
            }

            return MultMathExpr.Create(terms);
        }

        private static (IEnumerable<MathExpr>, int) GetUnsignedTerms(IEnumerable<MathExpr> terms)
        {
            // Avoid missed equalities due to signs. In cases such as (-0.1)/(-0.1),  the 1/(-0.1) is reduced to -1/0.1,
            // thus -0.1*(-1)/0.1 hides that 0.1 and -0.1 are essentially the same

            int sign = 1;

            var newTerms = terms.Select(term =>
            {
                if (term is ExactConstMathExpr numeric && numeric.IsNegative)
                {
                    sign = -sign;
                    return numeric.Negate();
                }
                else
                {
                    return term;
                }
            }).ToList(); // force it to run so the sign is updated

            return (newTerms, sign);
        }

        private static IEnumerable<MathExpr> Flatten(IEnumerable<MathExpr> terms)
        {
            return terms.SelectMany(expr => (expr is MultMathExpr multExpr) ? Flatten(multExpr.Terms) : new[] { expr });
        }

        private static IEnumerable<MathExpr> ReduceTerms(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            // It is possible that reduction of terms requires reflattening
            return Flatten(
                from expr in terms
                let exprReduced = expr.Reduce(options)
                where !MathEvalUtil.IsOne(exprReduced)
                select exprReduced);
        }

        private static Dictionary<MathExpr, List<MathExpr>> MapPowers(IEnumerable<MathExpr> terms)
        {
            var powers = new Dictionary<MathExpr, List<MathExpr>>();
            foreach (var expr in terms)
            {
                var pow = expr.AsPowerExpr();
                if (powers.ContainsKey(pow.Base))
                {
                    //TODO: input range validity: x/x is 1 for x!=0
                    powers[pow.Base].Add(pow.Exponent);
                }
                else
                {
                    powers.Add(pow.Base, new List<MathExpr> { pow.Exponent });
                }
            }

            return powers;
        }
    }
}
