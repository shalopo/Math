using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    internal class MathIdentityMatcher
    {
        private const int MAX_IDENTITY_TERMS_TO_CHECK = 2;

        public static MathExpr Reduce(MathExpr expr, ReduceOptions options)
        {
            options = options.With(allowSearchIdentities: false);

            foreach (var identity in MathIdentityManager.Identities)
            {
                var maxIndex = Math.Min(MAX_IDENTITY_TERMS_TO_CHECK, identity.MultTerms.Length);

                for (int idTermIndex1 = 0; idTermIndex1 < MAX_IDENTITY_TERMS_TO_CHECK; idTermIndex1++)
                {
                    expr = TryReduceByIdentity(expr, identity, identity.MultTerms[idTermIndex1], options);
                }
            }

            return expr;
        }
    
        private static MathExpr TryReduceByIdentity(MathExpr expr, MathIdentity identity, MultTerm identityMultTerm,
            ReduceOptions options)
        {
            bool redo;

            do
            {
                redo = false;

                var subExprs = (expr is AddMathExpr addExpr) ? addExpr.Exprs : new[] { expr };

                if (subExprs.Count < identity.AddExpr.Exprs.Count - 1)
                {
                    return expr;
                }

                foreach (var addTerm in subExprs)
                {
                    var multTerm = addTerm.AsMultTerm();
                    var match = identityMultTerm.Expr.Match(multTerm.Expr);

                    if (match == null)
                    {
                        continue;
                    }

                    var coefficient = (multTerm.Coefficient / identityMultTerm.Coefficient).Reduce(options);

                    var identityExprWithCoefficient = (AddMathExpr)AddMathExpr.Create(
                        identity.AddExpr.Exprs.Select(t => (-coefficient * t).Reduce(options)));

                    var transformedIdentity = identityExprWithCoefficient.Visit(match.Transformation);

                    var adjustedExpr = AddReducer.Reduce(((AddMathExpr)(expr + transformedIdentity)).Exprs, options);

                    if (adjustedExpr.Weight < expr.Weight)
                    {
                        expr = adjustedExpr;
                        redo = true;
                        break;
                    }
                }
            }
            while (redo);

            return expr;
        }
    }
}
