using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class AddMathExpr : MathExpr, IEnumerable<MathExpr>
    {
        private AddMathExpr(IEnumerable<MathExpr> terms) => Terms = terms.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Terms { get; }

        public static MathExpr Create(params MathExpr[] terms) => Create(terms.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> terms)
        {
            return (terms.Count()) switch
            {
                0 => GlobalMathDefs.ZERO,
                1 => terms.First(),
                _ => new AddMathExpr(terms)
            };
        }

        internal override bool RequiresMultScoping => true;
        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            var terms = Terms.ToList();

            // If the expression is not reduced, keep the order as given
            if (IsReduced)
            {
                terms.Sort((term1, term2) => term2.Weight.CompareTo(term1.Weight));
            }

            return ToStringInner(terms);
        }

        private static string ToStringInner(IReadOnlyList<MathExpr> terms)
        {
            var sb = new StringBuilder(terms[0].ToString());

            foreach (var term in terms.Skip(1))
            {
                sb.Append(" ");

                var termString = term.ToString();

                if (termString[0] == '-')
                {
                    // insert space
                    sb.Append($"- {termString.Substring(1)}");
                }
                else
                {
                    sb.Append($"+ {termString}");
                }
            }

            return sb.ToString();
        }

        internal override MathExpr Derive(MathVariable v) => Create(Terms.Select(expr => expr.Derive(v)));

        protected override MathExpr ReduceImpl(ReduceOptions options) => AddReducer.Reduce(Terms, options);

        internal override double WeightImpl => Terms.Aggregate(2.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Terms.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Add(Terms.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Terms.Select(expr => expr.Visit(transformer)));

        public override bool Equals(object other) => (other is AddMathExpr other_add) && EqualityUtil.Equals(Terms, other_add.Terms);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Terms, 982734678);

        internal override MathExprMatch Match(MathExpr expr)
        {
            //TODO: more advanced identity matching, e.g. sin(x) + i * cos(x) = e^(i*x)
            return null;
        }

        public IEnumerator<MathExpr> GetEnumerator() => Terms.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
