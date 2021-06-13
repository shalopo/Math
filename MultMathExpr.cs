using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class MultMathExpr : MathExpr, IEnumerable<MathExpr>
    {
        private MultMathExpr(IEnumerable<MathExpr> exprs) => Exprs = exprs.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Exprs { get; }

        public static MathExpr Create(params MathExpr[] exprs) => Create(exprs.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> exprs)
        {
            switch (exprs.Count())
            {
                case 0: return GlobalMathDefs.ONE;
                case 1: return exprs.First();
                default: return new MultMathExpr(exprs.SelectMany(expr => (expr is MultMathExpr multExpr) ?
                                                 multExpr.Exprs : new[]{ expr }));
            }
        }

        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            if (Exprs.OfType<NumericalConstMathExpr>().Any())
            {
                return AsMultTerm().ToString();
            }

            var sb = new StringBuilder(Exprs[0].ToMultScopedString());

            foreach (var expr in Exprs.Skip(1))
            {
                if (expr is ReciprocalMathExpr reciprocal)
                {
                    sb.Append("/");
                    sb.Append(reciprocal.Expr.ToPowScopedString());
                }
                else
                {
                    sb.Append("*");
                    sb.Append(expr.ToMultScopedString());
                }
            }

            return sb.ToString();
        }

        internal override MathExpr Derive(MathVariable v) => AddMathExpr.Create(
            from expr_to_derive_index in Enumerable.Range(0, Exprs.Count)
            let derived_expr = Exprs[expr_to_derive_index].Derive(v)
            where !MathEvalUtil.IsZero(derived_expr)
            select Create(
                (from other_expr_index in Enumerable.Range(0, Exprs.Count)
                 where other_expr_index != expr_to_derive_index
                 select Exprs[other_expr_index]).Prepend(derived_expr)
            ));

        protected override MathExpr ReduceImpl(ReduceOptions options) => MultReducer.Reduce(Exprs, options);

        internal override double Weight => Exprs.Aggregate(0.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Exprs.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Mult(Exprs.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Exprs.Select(expr => expr.Visit(transformer)));

        internal override MultTerm AsMultTerm()
        {
            var coefficient = NumericalConstMathExpr.Mult(Exprs.OfType<NumericalConstMathExpr>());
            return new MultTerm(Create(Exprs.Where(expr => !(expr is NumericalConstMathExpr))), coefficient);
        }

        public override bool Equals(object other) => (other is MultMathExpr other_mult) && EqualityUtil.Equals(Exprs, other_mult.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 407977119);

        internal override MathExprMatch Match(MathExpr expr)
        {
            //TODO: more advanced identity matching, e.g. 2sin(x)cos(x) = sin(2x)
            return null;
        }

        public IEnumerator<MathExpr> GetEnumerator() => Exprs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
