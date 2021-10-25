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

        ReduceOptions _options;

        public MathIdentityMatcher(ReduceOptions options) => _options = options.With(allowSearchIdentities: false);

        public static IEnumerable<MathExpr> Reduce(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            return new MathIdentityMatcher(options).DoReduce(terms);
        }

        private IEnumerable<MathExpr> DoReduce(IEnumerable<MathExpr> terms)
        {
            do
            {
                if (terms.All(term => term.IsConst))
                {
                    return terms;
                }

                var reducedExpr = TryReduce(terms);

                if (reducedExpr == null)
                {
                    break;
                }
                else
                {
                    terms = reducedExpr is AddMathExpr addExpr ? addExpr.Terms : new []{ reducedExpr };

                    //TODO: collect constants could be important.
                    // eaxmple: cos(2x) is better than 2cos(x)^2 - 1 ,   but 2cos(x)^2 could be better than cos(2x) + 1
                }
            }
            while (true);

            return terms;
        }

        private MathExpr TryReduce(IEnumerable<MathExpr> terms)
        {
            foreach (var identity in MathIdentityManager.Identities)
            {
                var maxIndex = Math.Min(MAX_IDENTITY_TERMS_TO_CHECK, identity.Terms.Count);

                for (int idTermIndex = 0; idTermIndex < maxIndex; idTermIndex++)
                {
                    if (terms.Count() < identity.Terms.Count - 1)
                    {
                        continue;
                    }

                    var reducedExpr = TryReduceByIdentity(terms, identity, idTermIndex);

                    if (reducedExpr != null)
                    {
                        return reducedExpr;
                    }
                }
            }

            return null;
        }

        private MathExpr TryReduceByIdentity(IEnumerable<MathExpr> terms, MathIdentity identity, int identityTermIndex)
        {
            var identityTerm = identity.Terms[identityTermIndex];

            if (identityTerm.IsConst)
            {
                return null;
            }

            foreach (var term in terms)
            {
                if (term.IsConst)
                {
                    continue;
                }

                var match = identityTerm.Match(term);

                if (match == null || match.IsTrivial)
                {
                    continue;
                }

                var identityExprWithCoefficient = GetIdentityWithCoefficient(identity, identityTerm, term);

                var transformedIdentity = match.Transform(identityExprWithCoefficient);

                var expr = AddMathExpr.Create(terms);
                var adjustedExpr = (expr - transformedIdentity).Reduce(_options.With(allowDistributeTerms: true));

                if (adjustedExpr.Weight < expr.Weight)
                {
                    return adjustedExpr;
                }
            }

            return null;
        }

        private AddMathExpr GetIdentityWithCoefficient(MathIdentity identity, MathExpr identityTerm, MathExpr term)
        {
            if (term.Coefficient.Equals(identityTerm.Coefficient))
            {
                return identity.AddExpr;
            }

            var coefficient = (term.Coefficient / identityTerm.Coefficient).Reduce(_options);

            var identityExprWithCoefficient = (AddMathExpr)AddMathExpr.Create(
                identity.Terms.Select(t => (-coefficient * t).Reduce(_options)));
            return identityExprWithCoefficient;
        }
    }
}
