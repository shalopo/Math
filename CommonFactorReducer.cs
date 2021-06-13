using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class CommonFactorReducer
    {
        class ExprPowers
        {
            public ExprPowers()
            {
                Powers = new Dictionary<AdditiveTerm, long>();
            }

            public Dictionary<AdditiveTerm, long> Powers { get; }

            public long MaxCommonExponent => Powers.Values.Min();

            public void Add(AdditiveTerm originatingTerm, long exponent)
            {
                if (Powers.ContainsKey(originatingTerm))
                {
                    Powers[originatingTerm] += exponent;
                }
                else
                {
                    Powers[originatingTerm] = exponent;
                }
            }
        }


        Dictionary<MathExpr, ExprPowers> _factors;
        readonly ReduceOptions _options;
        readonly ReduceOptions _optionsLight;

        CommonFactorReducer(ReduceOptions options)
        {
            _options = options;
            _optionsLight = _options.With(allowCommonFactorSearch: false, allowSearchIdentities: false);
        }

        public static MathExpr Reduce(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            return new CommonFactorReducer(options).DoReduce(terms);
        }

        private MathExpr DoReduce(IEnumerable<MathExpr> terms)
        {
            MapFactors(terms);

            RemoveNonCommonFactors();

            var fullCoverageFactor = TryGetFullCoverageFactor(terms);
            if (fullCoverageFactor != null)
            {
                return fullCoverageFactor;
            }

            return AddMathExpr.Create(terms);
        }

        private void MapFactors(IEnumerable<MathExpr> terms)
        {
            _factors = new Dictionary<MathExpr, ExprPowers>();

            var additiveTerms = terms.Select(expr => expr.AsAdditiveTerm());

            foreach (var additiveTerm in additiveTerms)
            {
                var multTerms = (additiveTerm.Expr is MultMathExpr multExpr) ? multExpr.Terms : new[] { additiveTerm.Expr };
                var powerTerms = multTerms.Select(term => (term is PowerMathExpr powerExpr) ?
                                                  powerExpr : new PowerMathExpr(term, GlobalMathDefs.ONE));

                foreach (var powerTerm in powerTerms)
                {
                    if (powerTerm.Exponent is ExactConstMathExpr exactExponent &&
                        exactExponent.IsWholeNumber)
                    {
                        ExprPowers powers;
                        if (!_factors.TryGetValue(powerTerm.Base, out powers))
                        {
                            powers = new ExprPowers();
                            _factors[powerTerm.Base] = powers;
                        }

                        powers.Add(additiveTerm, exactExponent.AsWholeNumber.Value);
                    }
                }
            }
        }

        private void RemoveNonCommonFactors()
        {
            var factorsToRemove = _factors.Where(factor => factor.Value.Powers.Count <= 1).Select(factor => factor.Key).ToList();

            foreach (var factor in factorsToRemove)
            {
                _factors.Remove(factor);
            }
        }

        private MathExpr TryGetFullCoverageFactor(IEnumerable<MathExpr> terms)
        {
            if (_factors.Count == 0 || !_factors.All(factor => factor.Value.Powers.Count == terms.Count()))
            {
                return null;
            }

            var commonFactor = MultMathExpr.Create(_factors.Select(factor => 
                                                   PowerMathExpr.Create(factor.Key, factor.Value.MaxCommonExponent)));

            var dividedTerms = terms.Select(term => (term / commonFactor).Reduce(_optionsLight));

            return (commonFactor * AddMathExpr.Create(dividedTerms).Reduce(_options)).Reduce(_optionsLight);
        }

    }
}
