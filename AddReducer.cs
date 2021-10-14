using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class AddReducer
    {
        public static MathExpr Reduce(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            terms = ReduceTerms(terms, options);

            terms = DistributeMultTerms(terms, options);
            
            var constTerm = NumericalConstMathExpr.Add(terms.OfType<NumericalConstMathExpr>());

            terms = CollectLikeTerms(terms, options);

            if (options.AllowReduceToConstComplex)
            {
                var constComplex = TryReduceToConstComplex(terms, constTerm);

                if (constComplex != null)
                {
                    return constComplex;
                }
            }

            if (!MathEvalUtil.IsZero(constTerm))
            {
                terms = terms.Append(constTerm);
            }

            var reducedExpr = AddMathExpr.Create(terms);
            
            if (options.AllowCommonFactorSearch && reducedExpr is AddMathExpr addExpr)
            {
                reducedExpr = CommonFactorReducer.Reduce(addExpr.Terms, options);
            }

            if (options.AllowSearchIdentities)
            {
                reducedExpr = MathIdentityMatcher.Reduce(reducedExpr, options);
            }

            return reducedExpr;
        }

        private static IEnumerable<MathExpr> ReduceTerms(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            return (from expr in terms
                     let exprReduced = expr.Reduce(options)
                     where !MathEvalUtil.IsZero(exprReduced)
                     select exprReduced is AddMathExpr addExpr ? addExpr.Terms : new[] { exprReduced }
            ).SelectMany(s => s);
        }

        private static IEnumerable<MathExpr> DistributeMultTerms(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            return terms.Select(expr => (expr is MultMathExpr multExpr) ? 
                                TryDistribute(multExpr, options) : 
                                new[] { expr })
                .SelectMany(s => s);
        }

        private static IEnumerable<MathExpr> TryDistribute(MultMathExpr expr, ReduceOptions options)
        {
            var addTerms = expr.Terms.OfType<AddMathExpr>();

            if (addTerms.Count() != 1)
            {
                return new[] { expr };
            }

            var addTerm = addTerms.First();
            var restOfTerms = MultMathExpr.Create(expr.Terms.Where(term => term != addTerm));

            return addTerm.Select(term => (term * restOfTerms).Reduce(options));
        }

        private static IEnumerable<MathExpr> CollectLikeTerms(IEnumerable<MathExpr> terms, ReduceOptions options)
        {
            var multiples = CollectMultiples(terms);
            terms = AggregateMultiples(options, multiples);
            return terms;
        }

        private static Dictionary<MathExpr, List<MathExpr>> CollectMultiples(IEnumerable<MathExpr> terms)
        {
            var multiples = new Dictionary<MathExpr, List<MathExpr>>();

            foreach (var expr in terms)
            {
                if (!(expr is NumericalConstMathExpr))
                {
                    var term = expr.AsAdditiveTerm();

                    if (multiples.ContainsKey(term.Expr))
                    {
                        multiples[term.Expr].Add(term.Coefficient);
                    }
                    else
                    {
                        multiples.Add(term.Expr, new List<MathExpr> { term.Coefficient });
                    }
                }
            }

            return multiples;
        }

        private static IEnumerable<MathExpr> AggregateMultiples(ReduceOptions options, Dictionary<MathExpr, List<MathExpr>> multiples)
        {
            return (from item in multiples
                    let expr = item.Key
                    let multiple = AddMathExpr.Create(item.Value).Reduce(options)
                    where !MathEvalUtil.IsZero(expr) && !MathEvalUtil.IsZero(multiple)
                    select MathEvalUtil.IsOne(expr) ? multiple :
                           MathEvalUtil.IsOne(multiple) ? expr : (multiple * expr).Reduce(options));
        }

        private static MathExpr TryReduceToConstComplex(IEnumerable<MathExpr> terms, NumericalConstMathExpr constTerm)
        {
            if (terms.Count() == 1)
            {
                var expr = terms.First();
                if (expr.IsConst && (!(expr is ConstMathExpr) || expr.Equals(ImaginaryMathExpr.Instance)))
                {
                    var complex = expr.ComplexEval();
                    var real_part = (constTerm + complex.Real).RealEval();

                    return complex.HasImagPart ? ConstComplexMathExpr.Create(real_part, complex.Imag) : (MathExpr)real_part;
                }
            }

            return null;
        }

    }
}
