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

        public static implicit operator ExactConstMathExpr(double value) => new(value);

        public double Value { get; }

        public bool IsWholeNumber => MathEvalUtil.IsWholeNumber(Value);
        public long? AsWholeNumber => IsWholeNumber ? Convert.ToInt64(Value) : (long?)null;

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

            long? wholeNumber = AsWholeNumber;
            
            if (wholeNumber != null)
            {
                return wholeNumber >= 0 ? ConstFractionMathExpr.Create(1, wholeNumber.Value) : 
                    ConstFractionMathExpr.Create(-1, -wholeNumber.Value);
            }

            return 1.0 / Value;
        }

        public override string ToString() => string.Format("{0:#,##0.###############}", Value);

        public override bool Equals(object obj)
        {
            if (obj is ExactConstMathExpr expr)
            {
                return (Value == expr.Value);
            }

            // Being the lowest form of expressions, it can leave it to higher forms to determine whether they are equal
            return obj.Equals(this); 
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }
    }

}
