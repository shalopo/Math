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
            terms = (from expr in terms select expr.Reduce(options)).AsEnumerable();

            //TODO: Simplify as in AddReducer

            terms = (from expr in terms select expr is MultMathExpr mult_expr ? mult_expr.Terms : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            Dictionary<MathExpr, List<MathExpr>> powers = CalculatePowers(terms);

            terms = (from item in powers
                     let @base = item.Key
                     let exponent = AddMathExpr.Create(item.Value).Reduce(options)
                     select PowerMathExpr.Create(@base, exponent).Reduce(options));

            terms = (from expr in terms select expr is MultMathExpr mult_expr ? mult_expr.Terms : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            var coefficient = NumericalConstMathExpr.Mult(terms.OfType<NumericalConstMathExpr>());

            if (MathEvalUtil.IsZero(coefficient))
            {
                return GlobalMathDefs.ZERO;
            }

            terms = terms.Where(expr => !(expr is NumericalConstMathExpr));

            //slupu: const * i => ConstComplex

            if (!MathEvalUtil.IsOne(coefficient))
            {
                terms = terms.Prepend(coefficient);
            }

            return MultMathExpr.Create(terms);
        }

        private static Dictionary<MathExpr, List<MathExpr>> CalculatePowers(IEnumerable<MathExpr> terms)
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
