using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class MultReducer
    {
        public static MathExpr Reduce(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            terms = Flatten(terms);
            terms = ReduceTerms(terms, options);

            int sign;
            (terms, sign) = GetUnsignedTerms(terms);

            //TODO: Simplify as in AddReducer

            Dictionary<MathExpr, List<MathExpr>> powers = MapPowers(terms);

            terms = powers.Select(item => PowerMathExpr.Create(item.Key,
                                                               AddMathExpr.Create(item.Value).Reduce(ReduceOptions.DEFAULT)).
                                  Reduce(ReduceOptions.DEFAULT)
                ).SelectMany(term => term.AsMultTerms()).ToList();

            var coefficient = NumericalConstMathExpr.Mult(terms.OfType<NumericalConstMathExpr>());

            if (MathEvalUtil.IsZero(coefficient))
            {
                return GlobalMathDefs.ZERO;
            }

            terms = terms.Where(expr => !(expr is NumericalConstMathExpr)).ToList();

            //slupu: const * i => ConstComplex

            if (sign < 0)
            {
                coefficient = coefficient.Negate();
            }

            if (!MathEvalUtil.IsOne(coefficient))
            {
                terms = new []{ coefficient }.Concat(terms).ToList();
            }

            return MultMathExpr.Create(terms);
        }

        private static (IReadOnlyList<MathExpr>, int) GetUnsignedTerms(IReadOnlyList<MathExpr> terms)
        {
            // Avoid missed equalities due to signs. In cases such as (-0.1)/(-0.1),  the 1/(-0.1) is reduced to -1/0.1,
            // thus -0.1*(-1)/0.1 hides that 0.1 and -0.1 are essentially the same

            int sign = 1;

            List<MathExpr> newTerms = new(terms.Count);

            foreach (var term in terms)
            {
                if (term is ExactConstMathExpr numeric && numeric.IsNegative)
                {
                    sign = -sign;
                    newTerms.Add(numeric.Negate());
                }
                else
                {
                    newTerms.Add(term);
                }
            }

            return (newTerms, sign);
        }

        private static IReadOnlyList<MathExpr> Flatten(IReadOnlyList<MathExpr> terms)
        {
            return FlattenInner(terms).ToList();

            static IEnumerable<MathExpr> FlattenInner(IReadOnlyList<MathExpr> terms)
            {
                return terms.SelectMany(term => 
                    (term is MultMathExpr multExpr) ? FlattenInner(multExpr.Terms) : 
                    (term is PowerMathExpr powerExpr && powerExpr.Base is MultMathExpr baseMultExpr) ?
                        baseMultExpr.Terms.Select(b => PowerMathExpr.Create(b, powerExpr.Exponent))
                    : term.AsSingleExprEnumerable()
                ).ToList();
            }
        }

        private static IReadOnlyList<MathExpr> ReduceTerms(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            // It is possible that reduction of terms requires reflattening
            return Flatten(
                terms.Select(term => term.Reduce(options)).
                Where(term => !MathEvalUtil.IsOne(term)).ToList());
        }

        private static Dictionary<MathExpr, List<MathExpr>> MapPowers(IReadOnlyList<MathExpr> terms)
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
