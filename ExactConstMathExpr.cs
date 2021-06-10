using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public sealed class ExactConstMathExpr : NumericalConstMathExpr
    {
        public ExactConstMathExpr(double value) => Value = value;

        public static implicit operator ExactConstMathExpr(double value) => new ExactConstMathExpr(value);

        public double Value { get; }

        internal override double Weight => MathEvalUtil.IsWholeNumber(Value) ? 1 : 2;
        internal override bool RequiresPowScoping => (Value < 0);

        public override bool IsPositive => (Value > 0);

        public override double ToDouble() => Value;

        public override NumericalConstMathExpr Negate() => new ExactConstMathExpr(-Value);
        public override NumericalConstMathExpr Reciprocate()
        {
            if (Math.Abs(Value) == 1.0)
            {
                return Value;
            }

            if (MathEvalUtil.IsWholeNumber(Value))
            {
                var long_value = Convert.ToInt64(Value);
                return long_value >= 0 ? ConstFractionMathExpr.Create(1, long_value) : ConstFractionMathExpr.Create(-1, -long_value);
            }

            return 1.0 / Value;
        }

        public override string ToString() => Value.ToString();

        public override bool Equals(object obj)
        {
            return obj is ExactConstMathExpr expr &&
                   Value == expr.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }
    }

}
