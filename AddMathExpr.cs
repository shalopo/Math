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
                var term = expr.AsTerm();
                var sign = (term.Coefficient >= 0) ? "+" : "-";
                var multiplier = (Math.Abs(term.Coefficient) == 1) ? "" : $"{Math.Abs(term.Coefficient)}*";

                sb.Append($" {sign} {multiplier}{term.Expr}");
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

            var dict = new Dictionary<MathExpr, double>();

            foreach (var expr in reduced_exprs)
            {
                var term = expr.AsTerm();

                if (dict.ContainsKey(term.Expr))
                {
                    dict[term.Expr] += term.Coefficient;
                }
                else
                {
                    dict.Add(term.Expr, term.Coefficient);
                }
            }

            return Create(dict.Select(item => (item.Key * item.Value).Reduce()));
        }


        public override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));
    }

}
