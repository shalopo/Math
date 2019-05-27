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

        internal override bool RequiresMultScoping => true;
        internal override bool RequiresPowScoping => true;

        internal override MathExpr Derive(MathVariable v) => -Expr.Derive(v);

        public override string ToString() => $"-{Expr.ToMultScopedString()}";

        internal override MathExpr Visit(IMathExprTransformer transformer) => -Expr.Visit(transformer);

        protected override MathExpr ReduceImpl()
        {
            var expr_reduced = Expr.Reduce();

            switch (expr_reduced)
            {
                case NegateMathExpr negate: return negate.Expr;
                case AddMathExpr add: return AddMathExpr.Create(add.Exprs.Select(NegateMathExpr.Create)).Reduce();
                case MultMathExpr mult: return MultMathExpr.Create(mult.Exprs.Prepend(-1)).Reduce();
            }

            return -expr_reduced;
        }

        internal override MathTerm AsMultTerm() => Expr.AsMultTerm() * (-1);

        public override bool Equals(object obj)
        {
            return obj is NegateMathExpr expr &&
                   EqualityComparer<MathExpr>.Default.Equals(Expr, expr.Expr);
        }

        public override int GetHashCode()
        {
            return 601397246 + Expr.GetHashCode();
        }
    }
}
