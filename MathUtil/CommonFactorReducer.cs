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
            public Dictionary<int, long> Powers { get; } = new(); // by originating term index

            //TODO: This is like finding the greatest common "factor" but in an additive way:
            // A*e^(x+2) + B*e^(2x+1)  => e^(x+1)*(A*e + B*e^x)
            public long MaxCommonExponent => Powers.Values.Min();

            public void Add(int originatingTermIndex, long exponent)
            {
                if (Powers.ContainsKey(originatingTermIndex))
                {
                    Powers[originatingTermIndex] += exponent;
                }
                else
                {
                    Powers[originatingTermIndex] = exponent;
                }
            }
        }


        IReadOnlyList<MathExpr> _terms;
        Dictionary<MathExpr, ExprPowers> _factors;
        readonly ReduceOptions _options;
        readonly ReduceOptions _optionsLight;

        CommonFactorReducer(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            _terms = terms.ToList();

            _options = options;
            _optionsLight = _options.With(allowCommonFactorSearch: false, allowSearchIdentities: false);
        }

        public static IEnumerable<MathExpr> Reduce(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            var result = new CommonFactorReducer(terms, options).DoReduce();

            return result is AddMathExpr addExpr ? addExpr.Terms : new[] { result };
        }

        private MathExpr DoReduce()
        {
            bool anyChanges = false;

            do
            {
                if (_terms.All(term => term.IsConst))
                {
                    break;
                }

                MapFactors();

                RemoveNonCommonFactors();

                var fullCoverageFactor = TryGetFullCoverageFactor();
                if (fullCoverageFactor != null)
                {
                    return fullCoverageFactor;
                }

                var newTerms = SearchSpeculatively();

                if (newTerms == null)
                {
                    break;
                }
                else
                {
                    anyChanges = true;
                    _terms = newTerms;
                }
            }
            while (true);

            var result = AddMathExpr.Create(_terms);

            return anyChanges ? result.Reduce(_optionsLight) : result;
        }

        private void MapFactors()
        {
            _factors = new Dictionary<MathExpr, ExprPowers>();

            for (int termIndex = 0; termIndex < _terms.Count(); termIndex++)
            {
                var term = _terms[termIndex];

                var multTerms = (term is MultMathExpr multExpr) ? multExpr.Terms : new[] { term };
                var powerTerms = multTerms.Select(expr => (expr is PowerMathExpr powerExpr) ?
                                                  powerExpr : new PowerMathExpr(expr, GlobalMathDefs.ONE));

                foreach (var powerTerm in powerTerms)
                {
                    if (powerTerm.Exponent is ExactConstMathExpr exactExponent &&
                        exactExponent.IsWholeNumber)
                    {
                        if (!_factors.TryGetValue(powerTerm.Base, out ExprPowers powers))
                        {
                            powers = new ExprPowers();
                            _factors[powerTerm.Base] = powers;
                        }

                        powers.Add(termIndex, exactExponent.AsWholeNumber.Value);
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

        private MathExpr TryGetFullCoverageFactor()
        {
            //TODO: this does not cover e.g. e^x since it looks for an integer value
            if (_factors.Count == 0 || !_factors.All(factor => factor.Value.Powers.Count == _terms.Count()))
            {
                return null;
            }

            var commonFactor = MultMathExpr.Create(_factors.Select(factor => 
                                                   PowerMathExpr.Create(factor.Key, factor.Value.MaxCommonExponent)));

            var dividedTerms = _terms.Select(term => (term / commonFactor).Reduce(_optionsLight));
            var innerExpr = AddMathExpr.Create(dividedTerms).Reduce(_options);

            return (commonFactor * innerExpr).Reduce(_optionsLight);
        }

        private IReadOnlyList<MathExpr> SearchSpeculatively()
        {
            //TODO: in descending order

            foreach (var factor in _factors)
            {
                var factorPowerExpr = PowerMathExpr.Create(factor.Key, factor.Value.MaxCommonExponent);
                
                var dividedTerms = factor.Value.Powers.Keys.
                    Select(termIndex => (_terms[termIndex] / factorPowerExpr).Reduce(_optionsLight));
                
                var preReducedInnerExpr = AddMathExpr.Create(dividedTerms);
                var innerExpr = preReducedInnerExpr.Reduce(_options);

                if (innerExpr.Weight >= preReducedInnerExpr.Weight)
                {
                    continue;
                }

                var newTerms = _terms.Where((_, termIndex) => !factor.Value.Powers.ContainsKey(termIndex)).ToList();

                if (!MathEvalUtil.IsZero(innerExpr))
                {
                    var innerTerms = (innerExpr is AddMathExpr innerAddExpr) ? innerAddExpr.Terms : new []{ innerExpr };

                    newTerms.AddRange(innerTerms.Select(term => (factorPowerExpr * term).Reduce(_optionsLight)));
                }

                return newTerms;
            }

            return null;
        }

    }
}
