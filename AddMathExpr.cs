using System.Collections.Generic;
using System.Linq;

namespace MathUtil
{
    class AddMathExpr : MathExpr
    {
        private AddMathExpr(IEnumerable<MathExpr> exprs) => Exprs = exprs.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Exprs { get; }

        public static MathExpr Create(params MathExpr[] exprs) => Create(exprs.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> exprs)
        {
            switch (exprs.Count())
            {
                case 0: return ExactConstMathExpr.ZERO;
                case 1: return exprs.First();
                default: return new AddMathExpr(exprs);
            }
        }

        public override bool RequiresScopingAsExponentBase => true;

        public override string ToString() => string.Join(" + ", Exprs.Select(expr => expr.ToString()));

        public override MathExpr Derive(MathVariable v) => Create(Exprs.Select(expr => expr.Derive(v)));

        public override MathExpr Reduce()
        {
            var reduced_exprs = (from expr in Exprs
                                 let expr_reduced = expr.Reduce()
                                 where !MathEvalUtil.IsZero(expr_reduced)
                                 select expr_reduced is AddMathExpr add_expr ? add_expr.Exprs : new MathExpr[] { expr_reduced }
                ).SelectMany(exprs => exprs).ToList();

            var exact_add = reduced_exprs.OfType<ExactConstMathExpr>().Aggregate(0L, (agg, expr) => agg + expr.Value);
            var double_add = reduced_exprs.OfType<DoubleConstMathExpr>().Aggregate(0.0, (agg, expr) => agg + expr.Value);

            var other_exprs = reduced_exprs.Where(expr => !(expr is ExactConstMathExpr) && !(expr is DoubleConstMathExpr));

            if (exact_add + double_add == 0.0)
            {
                return other_exprs.Any() ? Create(other_exprs) : ExactConstMathExpr.ZERO;
            }

            return Create(other_exprs.Append(
                double_add == 0 ? (MathExpr)new ExactConstMathExpr(exact_add) : (MathExpr)new DoubleConstMathExpr(exact_add + double_add)));
        }


        public override MathExpr Transform(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Transform(transformer)));
    }

}
