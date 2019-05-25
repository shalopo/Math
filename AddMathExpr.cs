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

        public override bool RequiresMultScoping => true;
        public override bool RequiresPowScoping => true;

        public override string ToString()
        {
            var sb = new StringBuilder(Exprs[0].ToString());

            foreach (var expr in Exprs.Skip(1))
            {
                var term = expr.AsAddTerm();

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

        public override MathExpr Derive(MathVariable v) => Create(Exprs.Select(expr => expr.Derive(v)));

        public override MathExpr Reduce()
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
                    var term = expr.AsAddTerm();

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


        public override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));
    }

}
