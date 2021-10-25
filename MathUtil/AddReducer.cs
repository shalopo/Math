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
            terms = Flatten(terms);
            terms = ReduceTerms(terms, options);

            if (options.AllowDistributeTerms)
            {
                terms = DistributeMultTerms(terms);
            }

            terms = CollectConsts(terms);

            if (options.AllowCommonFactorSearch)
            {
                terms = CommonFactorReducer.Reduce(terms);
            }

            if (options.AllowSearchIdentities)
            {
                terms = MathIdentityMatcher.Reduce(terms);
            }
            
            terms = CollectConsts(terms);

            return AddMathExpr.Create(terms);
        }

        private static IEnumerable<MathExpr> Flatten(IEnumerable<MathExpr> terms)
        {
            //TODO: Make this more efficient and only as needed, as it is rarely required
            return terms.SelectMany(expr => (expr is AddMathExpr addExpr) ? Flatten(addExpr.Terms) : new[] { expr });
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
            // It is possible that reduction of terms requires reflattening
            return Flatten(from expr in terms
                let exprReduced = expr.Reduce(options)
                where !MathEvalUtil.IsZero(exprReduced)
                select exprReduced);
        }

        private static IEnumerable<MathExpr> DistributeMultTerms(IEnumerable<MathExpr> terms)
        {
            return terms.Select(expr => (expr is MultMathExpr multExpr) ?  TryDistribute(multExpr) :  new[] { expr })
                .SelectMany(s => s);
        }

        private static IEnumerable<MathExpr> TryDistribute(MultMathExpr expr)
        {
            var addTerms = expr.Terms.OfType<AddMathExpr>();

            if (addTerms.Count() != 1)
            {
                return new[] { expr };
            }

            var addTerm = addTerms.First();
            var restOfTerms = MultMathExpr.Create(expr.Terms.Where(term => term != addTerm));

            return addTerm.Select(term => (term * restOfTerms).Reduce(ReduceOptions.LIGHT));
        }

    }
}
