using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public sealed class ConstFractionMathExpr : NumericalConstMathExpr
    {
        public static ConstFractionMathExpr Create(long top, long bottom)
        {
            if (bottom == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            return new ConstFractionMathExpr(top, bottom);
        }

        private ConstFractionMathExpr(long top, long bottom) => (Top, Bottom) = (top, bottom);

        public long Top { get; }
        public long Bottom { get; }

        internal override double Weight => 2;
        internal override bool RequiresPowScoping => true;
        internal override bool RequiresMultScoping => Top < 0;

        public override double ToDouble() => Top / ((double)Bottom);

        public override NumericalConstMathExpr Negate() => new ConstFractionMathExpr(-Top, Bottom);
        public override NumericalConstMathExpr Reciprocate() => new ConstFractionMathExpr(Bottom, Top);

        public override string ToString() => $"{TopAsString}/{BottomAsString}";
        public string TopAsString => $"{Top:n0}";
        public string BottomAsString => $"{Bottom:n0}";

        public override bool IsNegative => (Math.Sign(Top) * Math.Sign(Bottom) < 0);

        public override bool Equals(object obj)
        {
            return obj is ConstFractionMathExpr other &&
                   Top == other.Top &&
                   Bottom == other.Bottom;
        }

        public override int GetHashCode()
        {
            var hashCode = -1910534778;
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            return hashCode;
        }

        protected override MathExpr ReduceImpl(ReduceOptions options) => NumericalReduce();

        public NumericalConstMathExpr NumericalReduce()
        {
            if (Bottom == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            if (Top == 0)
            {
                return ZERO;
            }

            long top_reduced;
            long bottom_reduced;
            try
            {
                (top_reduced, bottom_reduced) = FractionUtil.ReduceFraction(Top, Bottom);
            }
            catch
            {
                return Top / ((double)Bottom);
            }

            if (bottom_reduced == 1)
            {
                return new ExactConstMathExpr(top_reduced);
            }

            if (bottom_reduced == -1)
            {
                return new ExactConstMathExpr(-top_reduced);
            }

            return Create(top_reduced, bottom_reduced);
        }
    }

}
