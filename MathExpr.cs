using System;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    abstract class MathExpr
    {
        public abstract bool RequiresScopingAsExponentBase { get; }

        public abstract override string ToString();

        public abstract MathExpr Derive(MathVariable v);
        public virtual MathExpr Reduce() => this;

        public virtual MathExpr Eval() => this;

        public abstract MathExpr Transform(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => MultMathExpr.Create(ExactConstMathExpr.MINUS_ONE, a);
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * b.Pow(-1);
        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        public static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];
    }

}
