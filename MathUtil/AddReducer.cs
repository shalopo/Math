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

            terms = terms.SelectMany(expr => (expr is AddMathExpr addExpr) ? addExpr.Terms : new[] { expr });

            terms = CollectConsts(terms);

            if (terms.Count() <= 1)
            {
                return AddMathExpr.Create(terms);
            }

            terms = DistributeMultTerms(terms, options);

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

        private static IEnumerable<MathExpr> CollectConsts(IEnumerable<MathExpr> terms)
        {
            var leftoverTerms = new List<MathExpr>();

            List<ConstComplexMathExpr> complexes = new();
            List<NumericalConstMathExpr> numericals = new();

            foreach (var term in terms)
            {
                if (term is NumericalConstMathExpr numericalTerm)
                {
                    numericals.Add(numericalTerm);
                }
                else if (term is ConstComplexMathExpr complextTerm)
                {
                    complexes.Add(complextTerm);
                }
                else
                {
                    leftoverTerms.Add(term);
                }
            }

            var numericalSum = NumericalConstMathExpr.Add(numericals);

            if (complexes.Count == 0)
            {
                if (!MathEvalUtil.IsZero(numericalSum))
                {
                    leftoverTerms.Add(numericalSum);
                }
                    
                return leftoverTerms;
            }

            complexes.Add(ConstComplexMathExpr.Create(numericalSum, GlobalMathDefs.ZERO));

            var complexSum = ConstComplexMathExpr.Add(complexes);

            if (!complexSum.Equals(ConstComplexMathExpr.ZERO_COMPLEX))
            {
                // Avoid adding zeroes
                leftoverTerms.Add(complexSum.ReducedAddExpr is AddMathExpr ? complexSum : complexSum.ReducedAddExpr);
            }   
            
            return leftoverTerms;
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

    }
}
