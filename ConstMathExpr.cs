using System;

namespace MathUtil
{

    abstract class ConstMathExpr : MathExpr
    {
        public override MathExpr Derive(MathVariable v) => ExactConstMathExpr.ZERO;

        public override MathExpr Transform(IMathExprTransformer transformer) => transformer.Transform(this);

        protected abstract ConstMathExpr Add(long value);
    }

    class ExactConstMathExpr : ConstMathExpr
    {
        public ExactConstMathExpr(long value) => Value = value;

        public long Value { get; }

        public override bool RequiresScopingAsExponentBase => false;

        public override string ToString() => Value.ToString();

        protected override ConstMathExpr Add(long value) => new ExactConstMathExpr(Value + value);

        public static readonly ExactConstMathExpr ZERO = new ExactConstMathExpr(0);
        public static readonly ExactConstMathExpr ONE = new ExactConstMathExpr(1);
        public static readonly ExactConstMathExpr MINUS_ONE = new ExactConstMathExpr(-1);
    }

    class DoubleConstMathExpr : ConstMathExpr
    {
        public DoubleConstMathExpr(double value) => Value = value;

        public double Value { get; }
        
        public override bool RequiresScopingAsExponentBase => false;

        public override string ToString() => Value.ToString();

        protected override ConstMathExpr Add(long value) => new DoubleConstMathExpr(Value + value);
    }

    class KnownConstMathExpr : ConstMathExpr
    {
        public KnownConstMathExpr(string name, double value) => (Name, Value) = (name, value);

        public string Name { get; }
        public double Value { get; }

        public override bool RequiresScopingAsExponentBase => false;

        public override string ToString() => Name;

        protected override ConstMathExpr Add(long value) => new DoubleConstMathExpr(Value + value);

        public static readonly KnownConstMathExpr E = new KnownConstMathExpr("e", Math.E);
        public static readonly KnownConstMathExpr PI = new KnownConstMathExpr("π", Math.PI);
    }

}
