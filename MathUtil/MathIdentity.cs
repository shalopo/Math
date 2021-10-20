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
            expr = expr.Reduce(ReduceOptions.LIGHT);

            if (expr is not AddMathExpr addExpr)
            {
                throw new NotImplementedException("Expression not supported");
            }

            AddExpr = addExpr;
        }

        public override string ToString()
        {
            return $"{AddMathExpr.Create(Terms)} = 0";
        }

        internal IReadOnlyList<MathExpr> Terms => AddExpr.Terms;
        public MathExpr Expr => AddExpr;

        internal AddMathExpr AddExpr { get; private set; }
    }

    public static class MathIdentityManager
    {
        private static readonly List<MathIdentity> identities = new();

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
