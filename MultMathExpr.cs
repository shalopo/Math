using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class MultMathExpr : MathExpr, IEnumerable<MathExpr>
    {
        private MultMathExpr(IEnumerable<MathExpr> terms) => Terms = terms.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Terms { get; }

        public static MathExpr Create(params MathExpr[] terms) => Create(terms.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> terms)
        {
            return (terms.Count()) switch
            {
                0 => GlobalMathDefs.ONE,
                1 => terms.First(),
                _ => new MultMathExpr(terms.SelectMany(expr => (expr is MultMathExpr multExpr) ?
                           multExpr.Terms : new[] { expr })),
            };
        }

        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            if (Terms.OfType<NumericalConstMathExpr>().Any())
            {
                return AsAdditiveTerm().ToString();
            }

            var negativePowers = Terms.OfType<PowerMathExpr>().Where(term => !term.Exponent.AsAdditiveTerm().Coefficient.IsPositive);
            var positivePowers = Terms.Where(term => !negativePowers.Contains(term));

            var sb = new StringBuilder();

            sb.Append(JoinString(positivePowers));

            if (negativePowers.Any())
            {
                if (!positivePowers.Any())
                {
                    sb.Append("1");
                }

                sb.Append("/");

                bool wrap = negativePowers.Count() > 1;

                if (wrap)
                {
                    sb.Append("(");
                }

                sb.Append(JoinString(negativePowers.Select(term => term.Reciprocate())));

                if (wrap)
                {
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        private static string JoinString(IEnumerable<MathExpr> terms)
        {
            return string.Join("*", terms.Select(term => term.ToPowScopedString()));
        }

        internal override MathExpr Derive(MathVariable v) => AddMathExpr.Create(
            from expr_to_derive_index in Enumerable.Range(0, Terms.Count)
            let derived_expr = Terms[expr_to_derive_index].Derive(v)
            where !MathEvalUtil.IsZero(derived_expr)
            select Create(
                (from other_expr_index in Enumerable.Range(0, Terms.Count)
                 where other_expr_index != expr_to_derive_index
                 select Terms[other_expr_index]).Prepend(derived_expr)
            ));

        protected override MathExpr ReduceImpl(ReduceOptions options) => MultReducer.Reduce(Terms, options);

        internal override double Weight => Terms.Aggregate(0.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Terms.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Mult(Terms.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Terms.Select(expr => expr.Visit(transformer)));

        internal override AdditiveTerm AsAdditiveTerm()
        {
            //TODO: causes pre-mature calculations
            var coefficient = NumericalConstMathExpr.Mult(Terms.OfType<NumericalConstMathExpr>());
            return new AdditiveTerm(Create(Terms.Where(expr => !(expr is NumericalConstMathExpr))), coefficient);
        }

        public override bool Equals(object other) => (other is MultMathExpr other_mult) && EqualityUtil.Equals(Terms, other_mult.Terms);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Terms, 407977119);

        internal override MathExprMatch Match(MathExpr expr)
        {
            //TODO: more advanced identity matching, e.g. 2sin(x)cos(x) = sin(2x)
            return null;
        }

        public IEnumerator<MathExpr> GetEnumerator() => Terms.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
