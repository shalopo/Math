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
            expr = expr.Reduce();

            if (!(expr is AddMathExpr addExpr))
            {
                throw new NotImplementedException("Expression not supported");
            }

            AddExpr = addExpr;
        }

        internal AddMathExpr AddExpr { get; }

        public MathExpr Expr => AddExpr;
    }

    public static class MathIdentitiesManager
    {
        private static List<MathIdentity> Identities { get; } = new List<MathIdentity>();

        public static void Register(MathIdentity identity)
        {
            Identities.Add(identity);
        }

        public static void Register(List<MathIdentity> identities)
        {
            foreach (var identity in identities)
            {
                Register(identity);
            }
        }

        static MathIdentitiesManager()
        {
            Register(TrigIdentities.Get());
        }

        public static MathExpr Reduce(MathExpr expr)
        {
            if (!(expr is AddMathExpr addExpr))
            {
                return expr;
            }

            foreach (var identity in Identities)
            {
                MathExpr newExpr = MathIdentityMatcher.Reduce(addExpr, identity);

                if (newExpr is AddMathExpr adjustedAddExpr)
                {
                    expr = newExpr;
                }
                else
                {
                    return newExpr;
                }
            }

            return expr;
        }
    }

}
