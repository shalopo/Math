using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class EqualityUtil
    {
        private static HashSet<MathExpr> ToSet(IEnumerable<MathExpr> terms)
        {
            var set = new HashSet<MathExpr>(terms);

            if (set.Count() != terms.Count())
            {
                throw new UnreducedMathExprException(terms);
            }

            return set;
        }

        public static bool Equals(IEnumerable<MathExpr> lhs, IEnumerable<MathExpr> rhs)
        {
            return ToSet(lhs).SetEquals(ToSet(rhs));
        }

        public static int GetHashCode(IEnumerable<MathExpr> exprs, int seed)
        {
            return exprs.Aggregate(seed, (agg, expr) => agg - 1521134295 * expr.GetHashCode());
        }
    }
}
