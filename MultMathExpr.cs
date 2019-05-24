using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class MultMathExpr : MathExpr
    {
        private MultMathExpr(IEnumerable<MathExpr> exprs) => Exprs = exprs.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Exprs { get; }

        public static MathExpr Create(params MathExpr[] exprs) => Create(exprs.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> exprs)
        {
            switch (exprs.Count())
            {
                case 0: return ExactConstMathExpr.ONE;
                case 1: return exprs.First();
                default: return new MultMathExpr(exprs);
            }
        }

        public override bool RequiresPowScoping => true;

        public override string ToString()
        {
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
            var reduced_exprs = (from expr in Exprs select expr.Reduce());

            var negative_coefficient = (reduced_exprs.OfType<NegateMathExpr>().Count() % 2 == 0) ? 1 : -1;

            reduced_exprs = (from expr in reduced_exprs
                             select expr is MultMathExpr mult_expr ? mult_expr.Exprs :
                                    expr is NegateMathExpr negate ? new MathExpr[] { negate.Expr } : 
                                    new MathExpr[] { expr }
                ).SelectMany(exprs => exprs).ToList();

            var coefficient = reduced_exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value);
            coefficient *= negative_coefficient;

            if (coefficient == 0.0)
            {
                return ExactConstMathExpr.ZERO;
            }

            var other_exprs = reduced_exprs.Where(expr => !(expr is ExactConstMathExpr) && !(expr is ReciprocalMathExpr));

            if (other_exprs.OfType<NegateMathExpr>().Count() % 2 != 0)
            {
                coefficient = -coefficient;
            }

            var reciprocal = MultMathExpr.Create(reduced_exprs.OfType<ReciprocalMathExpr>().Select(r => r.Expr)).Reduce();

            //TODO: find the const in a mult reciprocal
            if (reciprocal is ExactConstMathExpr exact_reciprocal)
            {
                (coefficient, reciprocal) = FractionUtil.ReduceFraction(coefficient, exact_reciprocal.Value);
            }

            other_exprs = (from expr in other_exprs
                           where !(expr is ReciprocalMathExpr)
                           select expr is NegateMathExpr negate ? negate.Expr : expr);

            if (!MathEvalUtil.IsOne(reciprocal))
            {
                other_exprs = other_exprs.Append(ReciprocalMathExpr.Create(reciprocal));
            }

            if (coefficient == 1.0)
            {
                return other_exprs.Any() ? Create(other_exprs) : ExactConstMathExpr.ONE;
            }

            return Create(other_exprs.Prepend(new ExactConstMathExpr(coefficient)));
        }

        public override MathExpr Visit(IMathExprTransformer transformer) => Create(Exprs.Select(expr => expr.Visit(transformer)));

        public override MathTerm AsTerm()
        {
            return new MathTerm(Create(Exprs.Where(expr => !(expr is ExactConstMathExpr))),
                Exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value));
        }
    }

}
