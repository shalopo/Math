using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    internal class MathIdentityMatcher
    {
        public static MathExpr Reduce(AddMathExpr addExpr, MathIdentity identity)
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
                        var coefficient = (multTerm.Coefficient / identityMultTerm.Coefficient).Reduce();

                        var identityExprWithCoefficient = (AddMathExpr)AddMathExpr.Create(
                            identity.AddExpr.Exprs.Select(t => (-coefficient * t).Reduce()));

                        var transformedIdentity = identityExprWithCoefficient.Visit(match.Transformation);

                        //TODO - light reduction so we don't get an infinite loop!
                        var adjustedExpr = (addExpr + transformedIdentity).Reduce();
                        
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
