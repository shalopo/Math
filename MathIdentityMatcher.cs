using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    internal class MathIdentityMatcher
    {
        public static MathExpr Reduce(MathExpr expr, ReduceOptions options)
        {
            if (!(expr is AddMathExpr addExpr))
            {
                return expr;
            }

            options = options.With(allowSearchIdentities: false);

            foreach (var identity in MathIdentityManager.Identities)
            {
                MathExpr newExpr = ReduceByIdentity(addExpr, identity, options);

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
    
        private static MathExpr ReduceByIdentity(AddMathExpr addExpr, MathIdentity identity, ReduceOptions options)
        {
            if (addExpr.Exprs.Count < identity.AddExpr.Exprs.Count - 1)
            {
                return addExpr;
            }

            // No point in matching the last term if all the others failed
            for (int idTermIndex1 = 0; idTermIndex1 < identity.AddExpr.Exprs.Count - 1; idTermIndex1++)
            {
                var identityMultTerm = identity.AddExpr.Exprs[idTermIndex1].AsMultTerm();

                foreach (var addTerm in addExpr.Exprs)
                {
                    var multTerm = addTerm.AsMultTerm();
                    var match = identityMultTerm.Expr.Match(multTerm.Expr);

                    if (match != null)
                    {
                        var coefficient = (multTerm.Coefficient / identityMultTerm.Coefficient).Reduce(options);

                        var identityExprWithCoefficient = (AddMathExpr)AddMathExpr.Create(
                            identity.AddExpr.Exprs.Select(t => (-coefficient * t).Reduce(options)));

                        var transformedIdentity = identityExprWithCoefficient.Visit(match.Transformation);

                        var adjustedExpr = AddReducer.Reduce(((AddMathExpr)(addExpr + transformedIdentity)).Exprs, options);
                        
                        if (adjustedExpr.Weight < addExpr.Weight)
                        {
                            if (adjustedExpr is AddMathExpr adjustedAddExpr)
                            {
                                //TODO - block the used transformation
                                //TODO - can't loop while changing
                                addExpr = adjustedAddExpr;
                            }
                            else
                            {
                                return adjustedExpr;
                            }
                        }
                    }
                }
            }

            return addExpr;
        }
    }
}
