﻿using System.Collections.Generic;
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

        protected override MathExpr ReduceImpl() => MultReducer.Reduce(Exprs);

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Exprs.Select(expr => expr.Visit(transformer)));

        internal override MultTerm AsMultTerm()
        {
            var coefficient = Exprs.OfType<ExactConstMathExpr>().Aggregate(1.0, (agg, expr) => agg * expr.Value);
            return new MultTerm(Create(Exprs.Where(expr => !(expr is ExactConstMathExpr))), coefficient);
        }

        public override bool Equals(object other) => (other is MultMathExpr other_mult) && EqualityUtil.Equals(Exprs, other_mult.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 407977119);
    }

}
