using System;
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

        internal override bool RequiresMultScoping => true;
        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            var sb = new StringBuilder(Exprs[0].ToString());

            foreach (var expr in Exprs.Skip(1))
            {
                var term = expr.AsMultTerm();

                switch (term.Coefficient)
                {
                    case ExactConstMathExpr exact:
                    {
                        if (exact.Value == 1)
                        {
                            sb.Append($" + {term.Expr}");
                        }
                        else if (exact.Value == -1)
                        {
                            sb.Append($" - {term.Expr.ToMultScopedString()}");
                        }
                        else if (exact.Value >= 0)
                        {
                            sb.Append($" + {exact.Value}*{term.Expr.ToMultScopedString()}");
                        }
                        else
                        {
                            sb.Append($" - {-exact.Value}*{term.Expr.ToMultScopedString()}");
                        }

                        break;
                    }
                    default:
                    {
                        sb.Append($" + {term.Coefficient.ToMultScopedString()}*{term.Expr.ToMultScopedString()}");
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        internal override MathExpr Derive(MathVariable v) => Create(Exprs.Select(expr => expr.Derive(v)));

        protected override MathExpr ReduceImpl()
        {
            var exprs = (from expr in Exprs
                         let expr_reduced = expr.Reduce()
                         where !MathEvalUtil.IsZero(expr_reduced)
                         select expr_reduced is AddMathExpr add_expr ? add_expr.Exprs : new MathExpr[] { expr_reduced }
                ).SelectMany(s => s);

            var dict = new Dictionary<MathExpr, MathExpr>();

            foreach (var expr in exprs)
            {
                if (!(expr is ExactConstMathExpr))
                {
                    var term = expr.AsMultTerm();

                    if (dict.ContainsKey(term.Expr))
                    {
                        dict[term.Expr] += term.Coefficient;
                    }
                    else
                    {
                        dict.Add(term.Expr, term.Coefficient);
                    }
                }
            }

            var @const = exprs.OfType<ExactConstMathExpr>().Aggregate(0.0, (agg, expr) => agg + expr.Value);

            exprs = dict.Select(item => MathEvalUtil.IsOne(item.Key) ? item.Value : (item.Key * item.Value).Reduce()).Concat(
                @const == 0 ? MathExpr.EMPTY_ARRAY : new MathExpr[] { @const }
            );

            return Create(exprs);
        }


        internal override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));

        public override bool Equals(object other) => (other is AddMathExpr other_add) && EqualityUtil.Equals(Exprs, other_add.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 982734678);
    }
}
