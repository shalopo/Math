using System;
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
        internal virtual MathExpr Reduce() => this;

        internal virtual MathTerm AsMultTerm() => new MathTerm(this, 1);
        internal virtual MathTerm AsPowerTerm() => new MathTerm(this, 1);

        internal abstract MathExpr Visit(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => NegateMathExpr.Create(a);
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * ReciprocalMathExpr.Create(b);
        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        internal static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];
    }

}
