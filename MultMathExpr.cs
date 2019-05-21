using System.Collections.Generic;
using System.Linq;

namespace MathUtil
{
    class MultMathExpr : MathExpr
    {
        private MultMathExpr(IEnumerable<MathExpr> exprs) => Exprs = exprs.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Exprs { get; }

        public static MathExpr Create(params MathExpr[] exprs) => Create(exprs.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> exprs)
        {
            var filtered_exprs = exprs.Where(expr => !MathEvalUtil.IsOne(expr));

            switch (filtered_exprs.Count())
            {
                case 0: return ExactConstMathExpr.ONE;
                case 1: return filtered_exprs.First();
                default: return new MultMathExpr(filtered_exprs);
            }
        }

        public override bool RequiresScopingAsExponentBase => true;

        public override string ToString() => string.Join("*", Exprs.Select(expr => expr is AddMathExpr ? $"({expr.ToString()})" : expr.ToString()));

        public override MathExpr Derive(MathVariable v) => AddMathExpr.Create(
            from expr_to_derive_index in Enumerable.Range(0, Exprs.Count)
            let derived_expr = Exprs[expr_to_derive_index].Derive(v)
            where !MathEvalUtil.IsZero(derived_expr)
            select Create(
                (from other_expr_index in Enumerable.Range(0, Exprs.Count)
                 where other_expr_index != expr_to_derive_index
                 select Exprs[other_expr_index]).Prepend(derived_expr)
            ));

        public override MathExpr Reduce()
        {
            var reduced_exprs = (from expr in Exprs
                                 let expr_reduced = expr.Reduce()
                                 select expr_reduced is MultMathExpr mult_expr ? mult_expr.Exprs : new MathExpr[] { expr_reduced }
                ).SelectMany(exprs => exprs).ToList();

            var factor = reduced_exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value);

            if (factor == 0.0)
            {
                return ExactConstMathExpr.ZERO;
            }

            var other_exprs = reduced_exprs.Where(expr => !(expr is ExactConstMathExpr));

            if (other_exprs.OfType<NegateMathExpr>().Count() % 2 != 0)
            {
                factor = -factor;
            }

            other_exprs = (from expr in other_exprs
                           select expr is NegateMathExpr negate ? negate.Expr : expr);

            if (factor == 1.0)
            {
                return other_exprs.Any() ? Create(other_exprs) : ExactConstMathExpr.ONE;
            }

            if (factor > 0)
            {
                return Create(other_exprs.Prepend(new ExactConstMathExpr(factor)));
            }
            else
            {
                return -Create(other_exprs.Prepend(new ExactConstMathExpr(-factor)));
            }

        }

        public override MathExpr Visit(IMathExprTransformer transformer) => Create(Exprs.Select(expr => expr.Visit(transformer)));
    }

}
