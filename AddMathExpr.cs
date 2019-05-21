using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder(Exprs[0].ToString());

            foreach (var expr in Exprs.Skip(1))
            {
                if (expr is NegateMathExpr negate)
                {
                    sb.Append(" - ");
                    sb.Append(negate.Expr.ToString());
                }
                else
                {
                    sb.Append(" + ");
                    sb.Append(expr.ToString());
                }
            }

            return sb.ToString();
        }

        public override MathExpr Derive(MathVariable v) => Create(Exprs.Select(expr => expr.Derive(v)));

        public override MathExpr Reduce()
        {
            var reduced_exprs = (from expr in Exprs
                                 let expr_reduced = expr.Reduce()
                                 where !MathEvalUtil.IsZero(expr_reduced)
                                 select expr_reduced is AddMathExpr add_expr ? add_expr.Exprs : new MathExpr[] { expr_reduced }
                ).SelectMany(exprs => exprs).ToList();

            var @const = reduced_exprs.OfType<ExactConstMathExpr>().Aggregate(0.0, (agg, expr) => agg + expr.Value);

            var other_exprs = reduced_exprs.Where(expr => !(expr is ExactConstMathExpr));

            if (@const == 0.0)
            {
                return other_exprs.Any() ? Create(other_exprs) : ExactConstMathExpr.ZERO;
            }

            return Create(other_exprs.Append(new ExactConstMathExpr(@const)));
        }


        public override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));
    }

}
