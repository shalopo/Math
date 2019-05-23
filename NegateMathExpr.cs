using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class NegateMathExpr : MathExpr
    {
        private NegateMathExpr(MathExpr expr) => Expr = expr;

        public static MathExpr Create(MathExpr expr) => expr is ExactConstMathExpr exact ? (MathExpr)(-exact.Value) : new NegateMathExpr(expr);

        public MathExpr Expr { get; }

        public override bool RequiresMultScoping => true;
        public override bool RequiresPowScoping => true;

        public override MathExpr Derive(MathVariable v) => -Expr.Derive(v);

        public override string ToString() => $"-{Expr.ToMultScopedString()}";

        public override MathExpr Visit(IMathExprTransformer transformer) => -Expr.Visit(transformer);

        public override MathExpr Reduce()
        {
            var expr_reduced = Expr.Reduce();

            switch (expr_reduced)
            {
                case NegateMathExpr negate: return negate.Expr;
                case AddMathExpr add: return AddMathExpr.Create(add.Exprs.Select(NegateMathExpr.Create)).Reduce();
            }

            return -expr_reduced;
        }
    }
}
