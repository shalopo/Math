using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public class MathIdentity
    {
        public MathIdentity(MathExpr expr)
        {
            expr = expr.Reduce(ReduceOptions.DEFAULT.With(allowSearchIdentities: false));

            if (!(expr is AddMathExpr addExpr))
            {
                throw new NotImplementedException("Expression not supported");
            }

            AddExpr = addExpr;

            AdditiveTerms = AddExpr.Select(term => term.AsAdditiveTerm()).ToArray();
        }

        internal AddMathExpr AddExpr { get; }
        internal AdditiveTerm[] AdditiveTerms { get; }

        public MathExpr Expr => AddExpr;
    }

    public static class MathIdentityManager
    {
        private static readonly List<MathIdentity> identities = new List<MathIdentity>();

        public static IReadOnlyCollection<MathIdentity> Identities => identities;

        public static void Register(MathIdentity identity)
        {
            identities.Add(identity);
        }

        public static void Register(List<MathIdentity> identities)
        {
            foreach (var identity in identities)
            {
                Register(identity);
            }
        }

        static MathIdentityManager()
        {
            Register(TrigIdentities.Get());
        }

    }

}
