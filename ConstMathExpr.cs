
using System;

namespace MathUtil
{

    public abstract class ConstMathExpr : MathExpr
    {
        internal override MathExpr Derive(MathVariable v) => ExactConstMathExpr.ZERO;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);
    }

    public class ExactConstMathExpr : ConstMathExpr
    {
        public ExactConstMathExpr(double value) => Value = value;

        public double Value { get; }

        internal override bool RequiresPowScoping => (Value < 0);

        public override string ToString() => Value.ToString();

        internal override MultTerm AsMultTerm() => new MultTerm(1, Value);

        public override bool Equals(object obj)
        {
            return obj is ExactConstMathExpr expr &&
                   Value == expr.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        public static readonly ExactConstMathExpr ZERO = new ExactConstMathExpr(0);
        public static readonly ExactConstMathExpr ONE = new ExactConstMathExpr(1);
        public static readonly ExactConstMathExpr TWO = new ExactConstMathExpr(2);
        public static readonly ExactConstMathExpr MINUS_ONE = new ExactConstMathExpr(-1);
    }

    public class KnownConstMathExpr : ConstMathExpr
    {
        public KnownConstMathExpr(string name, double value) => (Name, Value) = (name, value);

        public string Name { get; }
        public double Value { get; }

        internal override bool RequiresPowScoping => false;

        public override string ToString() => Name;

        public static readonly KnownConstMathExpr E = new KnownConstMathExpr("e", Math.E);
        public static readonly KnownConstMathExpr PI = new KnownConstMathExpr("π", Math.PI);
    }

}
