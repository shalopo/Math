using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class AddReducer
    {
        public static MathExpr Reduce(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            terms = Flatten(terms);
            terms = ReduceTerms(terms, options);

            if (options.AllowDistributeTerms)
            {
                //TODO: undo it if we end up adding weight (after perfoming the next reductions as well)
                //TODO: We may want to limit this or only distribute some of the terms / do this iteratively
                terms = DistributeMultTerms(terms);
            }

            terms = CollectConsts(terms);

            if (options.AllowCommonFactorSearch)
            {
                terms = CommonFactorReducer.Reduce(terms, options.AllowFullCoverageFactor);
            }

            if (options.AllowSearchIdentities)
            {
                terms = MathIdentityMatcher.Reduce(terms);
            }
            
            terms = CollectConsts(terms);

            return AddMathExpr.Create(terms);
        }

        private static IReadOnlyList<MathExpr> Flatten(IReadOnlyList<MathExpr> terms)
        {
            return FlattenInner(terms).ToList();

            static IEnumerable<MathExpr> FlattenInner(IReadOnlyList<MathExpr> terms)
            {
                return terms.SelectMany(expr =>
                    (expr is AddMathExpr addExpr) ? FlattenInner(addExpr.Terms) : expr.AsSingleExprEnumerable()
                ).ToList();
            }
        }

        private static IReadOnlyList<MathExpr> CollectConsts(IReadOnlyList<MathExpr> terms)
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

        private static IReadOnlyList<MathExpr> ReduceTerms(IReadOnlyList<MathExpr> terms, ReduceOptions options)
        {
            // It is possible that reduction of terms requires reflattening
            return Flatten(
                terms.Select(term => term.Reduce(options)).
                Where(term => !MathEvalUtil.IsZero(term)).ToList());
        }

        private static IReadOnlyList<MathExpr> DistributeMultTerms(IReadOnlyList<MathExpr> terms)
        {
            return terms.SelectMany(expr =>
                (expr is MultMathExpr multExpr) ? TryDistribute(multExpr) : expr.AsSingleExprEnumerable()).ToList();
        }

        private static IEnumerable<MathExpr> TryDistribute(MultMathExpr expr)
        {
            var addTerms = expr.Terms.OfType<AddMathExpr>().ToList();

            if (addTerms.Count != 1)
            {
                return expr.AsSingleExprEnumerable();
            }

            var addTerm = addTerms[0];
            var restOfTerms = MultMathExpr.Create(expr.Terms.Where(term => term != addTerm));

            return addTerm.Select(term => (term * restOfTerms).Reduce(ReduceOptions.LIGHT));
        }

    }
}
