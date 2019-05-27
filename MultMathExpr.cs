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

        internal override bool RequiresPowScoping => true;

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

        internal override MathExpr Derive(MathVariable v) => AddMathExpr.Create(
            from expr_to_derive_index in Enumerable.Range(0, Exprs.Count)
            let derived_expr = Exprs[expr_to_derive_index].Derive(v)
            where !MathEvalUtil.IsZero(derived_expr)
            select Create(
                (from other_expr_index in Enumerable.Range(0, Exprs.Count)
                 where other_expr_index != expr_to_derive_index
                 select Exprs[other_expr_index]).Prepend(derived_expr)
            ));

        private static (IEnumerable<MathExpr>, int coefficient) ReduceNegatives(IEnumerable<MathExpr> exprs)
        {
            var result = new List<MathExpr>();
            int coefficient = 1;

            foreach (var expr in exprs)
            {
                if (expr is NegateMathExpr negate)
                {
                    coefficient = -coefficient;
                    result.Add(negate.Expr);
                }
                else
                {
                    result.Add(expr);
                }
            }

            return (result, coefficient);
        }

        protected override MathExpr ReduceImpl()
        {
            var exprs = (from expr in Exprs select expr.Reduce());

            int negative_coefficient;
            (exprs, negative_coefficient) = ReduceNegatives(exprs);

            exprs = (from expr in exprs
                     select (expr is ReciprocalMathExpr reciprocal_ && reciprocal_.Expr is MultMathExpr mult) ?
                     mult.Exprs.Select(mexpr => ReciprocalMathExpr.Create(mexpr)).AsEnumerable<MathExpr>() :
                     new MathExpr[] { expr }).SelectMany(s => s);

            int negative_coefficient2;
            (exprs, negative_coefficient2) = ReduceNegatives(exprs);
            negative_coefficient *= negative_coefficient2;

            exprs = (from expr in exprs
                     select expr is MultMathExpr mult_expr ? mult_expr.Exprs : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            var dict = new Dictionary<MathExpr, MathExpr>();
            foreach (var expr in exprs)
            {
                var term = expr.AsPowerTerm();
                if (dict.ContainsKey(term.Expr))
                {
                    dict[term.Expr] += term.Coefficient;
                }
                else
                {
                    dict.Add(term.Expr, term.Coefficient);
                }
            }

            exprs = dict.Select(item => PowerMathExpr.Create(item.Key, item.Value).Reduce());

            var coefficient = exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value);
            coefficient *= negative_coefficient;

            if (coefficient == 0.0)
            {
                return ExactConstMathExpr.ZERO;
            }

            var other_exprs = exprs.Where(expr => !(expr is ExactConstMathExpr) && !(expr is ReciprocalMathExpr));

            if (other_exprs.OfType<NegateMathExpr>().Count() % 2 != 0)
            {
                coefficient = -coefficient;
            }

            var reciprocal = MultMathExpr.Create(exprs.OfType<ReciprocalMathExpr>().Select(r => r.Expr)).Reduce();

            //TODO: doesn't work anymore - reciprocals are transformed into pow(-1)
            //TODO: find the const in a mult reciprocal
            if (reciprocal is ExactConstMathExpr exact_reciprocal)
            {
                (coefficient, reciprocal) = FractionUtil.ReduceFraction(coefficient, exact_reciprocal.Value);
            }

            other_exprs = (from expr in other_exprs
                           where !(expr is ReciprocalMathExpr)
                           select expr is NegateMathExpr negate ? negate.Expr : expr);

            //TODO: collect terms - with power

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

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Exprs.Select(expr => expr.Visit(transformer)));

        internal override MathTerm AsMultTerm()
        {
            return new MathTerm(Create(Exprs.Where(expr => !(expr is ExactConstMathExpr))),
                Exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value));
        }

        public override bool Equals(object other) => (other is MultMathExpr other_mult) && EqualityUtil.Equals(Exprs, other_mult.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 407977119);
    }

}
