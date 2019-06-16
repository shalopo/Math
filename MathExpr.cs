using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public abstract class MathExpr
    {
        internal virtual bool RequiresMultScoping => false;
        internal virtual bool RequiresPowScoping => false;

        internal string ToMultScopedString() => RequiresMultScoping ? $"({this})" : ToString();
        internal string ToPowScopedString() => RequiresPowScoping ? $"({this})" : ToString();

        public abstract override string ToString();

        internal abstract MathExpr Derive(MathVariable v);

        internal MathExpr Reduce()
        {
            if (IsReduced)
            {
                return this;
            }

            var reduced = ReduceImpl();
            reduced.IsReduced = true;
            return reduced;
        }

        protected virtual MathExpr ReduceImpl()
        {
            return this;
        }

        internal abstract double ExactEval();

        internal virtual MultTerm AsMultTerm() => new MultTerm(this, 1);
        internal virtual PowerMathExpr AsPowerExpr() => new PowerMathExpr(this, 1);

        internal abstract MathExpr Visit(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => NegateMathExpr.Create(a);
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * ReciprocalMathExpr.Create(b);

        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        internal static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];

        private bool IsReduced { get; set; } = false;
    }

}
