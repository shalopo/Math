
using System;
using System.Linq;
using System.Collections.Generic;

namespace MathUtil
{

    public abstract class ConstMathExpr : MathExpr
    {
        internal override MathExpr Derive(MathVariable v) => ExactConstMathExpr.ZERO;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);
    }

    public abstract class NumericalConstMathExpr : ConstMathExpr
    {
        public static implicit operator NumericalConstMathExpr(double value) => new ExactConstMathExpr(value);

        internal override MultTerm AsMultTerm() => new MultTerm(1, this);

        public abstract bool IsPositive { get; }

        public abstract double ToDouble();

        public abstract NumericalConstMathExpr Negate();
        public abstract NumericalConstMathExpr Reciprocate();


        public static NumericalConstMathExpr Mult(IEnumerable<NumericalConstMathExpr> exprs)
        {
            var fractions = exprs.OfType<ConstFractionMathExpr>();
            var exacts = exprs.OfType<ExactConstMathExpr>();

            if (fractions.Any() && exacts.All(exact => MathEvalUtil.IsWholeNumber(exact.Value)))
            {
                long top = exacts.Aggregate(1L, (agg, exact) => agg * Convert.ToInt64(exact.ToDouble())) *
                           fractions.Aggregate(1L, (agg, fraction) => agg * fraction.Top);
                long bottom = fractions.Aggregate(1L, (agg, fraction) => agg * fraction.Bottom);

                return ConstFractionMathExpr.Create(top, bottom).NumericalReduce();
            }
            else
            {
                return exprs.Aggregate(1.0, (agg, expr) => agg * expr.ToDouble());
            }
        }
    
        public static NumericalConstMathExpr Add(IEnumerable<NumericalConstMathExpr> exprs)
        {
            var fractions = exprs.OfType<ConstFractionMathExpr>().ToList();
            var exacts = exprs.OfType<ExactConstMathExpr>();

            if (fractions.Any() && exacts.All(exact => MathEvalUtil.IsWholeNumber(exact.Value)))
            {
                var lcm = Convert.ToInt64(fractions.Aggregate(1UL, (agg, fraction) => FractionUtil.LCM(agg, (ulong)fraction.Bottom)));
                var fractions_top = fractions.Aggregate(0L, (agg, fraction) => agg + (lcm / fraction.Bottom) * fraction.Top);
                var exacts_top = exacts.Aggregate(0L, (agg, exact) => agg + Convert.ToInt64(exact.Value) * lcm);

                return ConstFractionMathExpr.Create(fractions_top + exacts_top, lcm).NumericalReduce();
            }
            else
            {
                return exprs.Aggregate(0.0, (agg, expr) => agg + expr.ToDouble());
            }
        }
    }

    public sealed class ExactConstMathExpr : NumericalConstMathExpr
    {
        public ExactConstMathExpr(double value) => Value = value;

        public static implicit operator ExactConstMathExpr(double value) => new ExactConstMathExpr(value);

        public double Value { get; }

        internal override bool RequiresPowScoping => (Value < 0);

        public override bool IsPositive => (Value > 0);

        public override double ToDouble() => Value;
        internal override double ExactEval() => Value;

        public override NumericalConstMathExpr Negate() => new ExactConstMathExpr(-Value);
        public override NumericalConstMathExpr Reciprocate()
        {
            if (Math.Abs(Value) == 1.0)
            {
                return Value;
            }

            if (MathEvalUtil.IsWholeNumber(Value))
            {
                long long_value = (long)Value;
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

        public static readonly ExactConstMathExpr ZERO = new ExactConstMathExpr(0);
        public static readonly ExactConstMathExpr ONE = new ExactConstMathExpr(1);
        public static readonly ExactConstMathExpr TWO = new ExactConstMathExpr(2);
        public static readonly ExactConstMathExpr MINUS_ONE = new ExactConstMathExpr(-1);
    }

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

        internal override bool RequiresPowScoping => true;

        public override double ToDouble() => Top / ((double)Bottom);

        public override NumericalConstMathExpr Negate() => new ConstFractionMathExpr(-Top, Bottom);
        public override NumericalConstMathExpr Reciprocate() => new ConstFractionMathExpr(Bottom, Top);

        public override string ToString() => $"{Top}/{Bottom}";

        public override bool IsPositive => (Math.Sign(Top) * Math.Sign(Bottom) > 0);

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

        protected override MathExpr ReduceImpl() => NumericalReduce();

        public NumericalConstMathExpr NumericalReduce()
        {
            if (Bottom == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            if (Top == 0)
            {
                return ExactConstMathExpr.ZERO;
            }

            var sign = Math.Sign(Top) * Math.Sign(Bottom);

            (long top_reduced, long bottom_reduced) = (Math.Abs(Top), Math.Abs(Bottom));

            (top_reduced, bottom_reduced) = FractionUtil.ReduceFraction(top_reduced, bottom_reduced);

            if (bottom_reduced == 1)
            {
                return new ExactConstMathExpr(sign * top_reduced);
            }

            return Create(sign * top_reduced, bottom_reduced);
        }

        internal override double ExactEval() => Top / ((double)Bottom);

        public static readonly ConstFractionMathExpr HALF = new ConstFractionMathExpr(1, 2);
    }

    public class KnownConstMathExpr : ConstMathExpr
    {
        public KnownConstMathExpr(string name, double value) => (Name, Value) = (name, value);

        public string Name { get; }
        public double Value { get; }

        internal override bool RequiresPowScoping => false;

        internal override double ExactEval() => Value;

        public override string ToString() => Name;

        public static readonly KnownConstMathExpr E = new KnownConstMathExpr("e", Math.E);
        public static readonly KnownConstMathExpr PI = new KnownConstMathExpr("π", Math.PI);
    }

}
