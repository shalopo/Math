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

        internal override double Weight => Expr.Weight + 1;
        internal override bool RequiresPowScoping => true;

        internal override MathExpr Derive(MathVariable v) => -Expr.Derive(v) * Expr.Pow(-2);

        protected override MathExpr ReduceImpl()
        {
            var expr_reduced = Expr.Reduce();

            switch (expr_reduced)
            {
                case ReciprocalMathExpr reciprocal: return reciprocal.Expr;
                case NumericalConstMathExpr numerical: return numerical.Reciprocate().Reduce();
                case MultMathExpr mult: return MultMathExpr.Create(mult.Exprs.Select(ReciprocalMathExpr.Create)).Reduce(); //TODO: wrong?
                case PowerMathExpr power: return PowerMathExpr.Create(power.Base, (-power.Exponent).Reduce());
                case NegateMathExpr negate: return NegateMathExpr.Create(ReciprocalMathExpr.Create(negate.Expr)).Reduce();
            }

            return Create(expr_reduced);
        }

        internal override bool IsConst => Expr.IsConst;
        internal override ConstComplexMathExpr ComplexEval() => Expr.ComplexEval().Reciprocate();

        internal override PowerMathExpr AsPowerExpr() => Expr.AsPowerExpr().Reciprocate();

        public override string ToString() => $"1/{Expr.ToPowScopedString()}";

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Expr.Visit(transformer));

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
