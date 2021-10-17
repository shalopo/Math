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

        CommonFactorReducer(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            _terms = terms;

            _options = options;
            _optionsLight = _options.With(allowCommonFactorSearch: false, allowSearchIdentities: false);
        }

        public static MathExpr Reduce(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            return new CommonFactorReducer(terms, options).DoReduce();
        }

        private MathExpr DoReduce()
        {
            bool redo;

            do
            {
                redo = false;

                MapFactors();

                RemoveNonCommonFactors();

                var fullCoverageFactor = TryGetFullCoverageFactor();
                if (fullCoverageFactor != null)
                {
                    return fullCoverageFactor;
                }

                //TODO: JoinSimilarTerms();

                var newTerms = SearchSpeculatively();

                if (newTerms != null)
                {
                    _terms = newTerms;
                    redo = true;
                }
            } while (redo);

            return AddMathExpr.Create(_terms);
        }

        private void MapFactors()
        {
            _factors = new Dictionary<MathExpr, ExprPowers>();

            for (int termIndex = 0; termIndex < _terms.Count(); termIndex++)
            {
                var term = _terms[termIndex];
                var additiveTerm = term.AsAdditiveTerm();

                var multTerms = (additiveTerm.Expr is MultMathExpr multExpr) ? multExpr.Terms : new[] { additiveTerm.Expr };
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

                var innerTerms = (innerExpr is AddMathExpr innerAddExpr) ? innerAddExpr.Terms : new[] { innerExpr };
                
                var newTerms = innerTerms.Select(term => (factorPowerExpr * term).Reduce(_optionsLight)).ToList();

                newTerms.AddRange(_terms.Where((_, termIndex) => !factor.Value.Powers.ContainsKey(termIndex)));

                return newTerms;
            }

            return null;
        }

    }
}
