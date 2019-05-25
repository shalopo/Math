using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class ReciprocalMathExpr : MathExpr
    {
        public static ReciprocalMathExpr Create(MathExpr expr) => new ReciprocalMathExpr(expr);
        private ReciprocalMathExpr(MathExpr expr) => Expr = expr;

        public MathExpr Expr { get; }

        public override bool RequiresPowScoping => true;

        public override MathExpr Derive(MathVariable v) => -Expr.Derive(v) * Expr.Pow(-2);

        public override MathExpr Reduce()
        {
            var expr_reduced = Expr.Reduce();

            switch (expr_reduced)
            {
                case ReciprocalMathExpr reciprocal: return reciprocal.Expr;
                case ExactConstMathExpr exact when Math.Abs(exact.Value) == 1: return exact.Value;
                //case MultMathExpr mult: return MultMathExpr.Create(mult.Exprs.Select(ReciprocalMathExpr.Create)).Reduce();
                case PowerMathExpr power: return PowerMathExpr.Create(power.Base, (-power.Exponent).Reduce());
                case NegateMathExpr negate: return NegateMathExpr.Create(1 / negate.Expr);
            }

            return Create(expr_reduced);
        }

        public override MathTerm AsMultTerm() => Expr.AsMultTerm() * (-1);

        public override string ToString() => $"1/{Expr.ToPowScopedString()}";

        public override MathExpr Visit(IMathExprTransformer transformer) => Create(Expr.Visit(transformer));

        public override bool Equals(object obj)
        {
            return obj is ReciprocalMathExpr expr &&
                   EqualityComparer<MathExpr>.Default.Equals(Expr, expr.Expr);
        }

        public override int GetHashCode()
        {
            return 601397246 + EqualityComparer<MathExpr>.Default.GetHashCode(Expr);
        }
    }
}
