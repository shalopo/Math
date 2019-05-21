using System;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    abstract class MathExpr
    {
        public virtual bool RequiresMultScoping => false;
        public virtual bool RequiresPowScoping => false;

        public string ToMultScopedString() => RequiresMultScoping ? $"({this})" : ToString();
        public string ToPowScopedString() => RequiresPowScoping ? $"({this})" : ToString();

        public abstract override string ToString();

        public abstract MathExpr Derive(MathVariable v);
        public virtual MathExpr Reduce() => this;

        public abstract MathExpr Visit(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => NegateMathExpr.Create(a);
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * ReciprocalMathExpr.Create(b);
        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        public static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];
    }

}
