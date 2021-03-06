using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public abstract class NumericalConstMathExpr : ConstMathExpr
    {
        public static implicit operator NumericalConstMathExpr(double value) => new ExactConstMathExpr(value);

        internal override NumericalConstMathExpr Coefficient => this;
        public abstract bool IsNegative { get; }

        public abstract double ToDouble();

        internal sealed override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Create(new ExactConstMathExpr(ToDouble()), GlobalMathDefs.ZERO);

        public abstract NumericalConstMathExpr Negate();
        public abstract NumericalConstMathExpr Reciprocate();


        public static NumericalConstMathExpr Mult(IEnumerable<NumericalConstMathExpr> terms)
        {
            if (terms.Count() == 1)
            {
                return terms.First();
            }

            checked
            {
                var fractions = terms.OfType<ConstFractionMathExpr>().ToList();

                if (fractions.Any())
                {
                    var exacts = terms.OfType<ExactConstMathExpr>().ToList();

                    if (exacts.All(exact => MathEvalUtil.IsWholeNumber(exact.Value)))
                    {
                        try
                        {
                            checked
                            {
                                long top = exacts.Aggregate(1L, (agg, exact) => agg * exact.AsWholeNumber.Value) *
                                            fractions.Aggregate(1L, (agg, fraction) => agg * fraction.Top);
                                long bottom = fractions.Aggregate(1L, (agg, fraction) => agg * fraction.Bottom);

                                return ConstFractionMathExpr.Create(top, bottom).NumericalReduce();
                            }
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                }

                return terms.Aggregate(1.0, (agg, expr) => agg * expr.ToDouble());
            }
        }

        public static NumericalConstMathExpr Mult(NumericalConstMathExpr a, NumericalConstMathExpr b)
        {
            return Mult(new NumericalConstMathExpr[] { a, b });
        }

        public static NumericalConstMathExpr Add(IEnumerable<NumericalConstMathExpr> terms)
        {
            var fractions = terms.OfType<ConstFractionMathExpr>();
            var exacts = terms.OfType<ExactConstMathExpr>();

            if (fractions.Any() && exacts.All(exact => MathEvalUtil.IsWholeNumber(exact.Value)))
            {
                try
                {
                    checked
                    {
                        var lcm = Convert.ToInt64(fractions.Aggregate(1UL, (agg, fraction) => FractionUtil.LCM(agg, (ulong)Math.Abs(fraction.Bottom))));
                        var fractions_top = fractions.Aggregate(0L, (agg, fraction) => agg + (lcm / fraction.Bottom) * fraction.Top);
                        var exacts_top = exacts.Aggregate(0L, (agg, exact) => agg + Convert.ToInt64(exact.Value) * lcm);

                        return ConstFractionMathExpr.Create(fractions_top + exacts_top, lcm).NumericalReduce();
                    }
                }
                catch (OverflowException)
                {
                }
            }

            return terms.Aggregate(0.0, (agg, expr) => agg + expr.ToDouble());
        }

        public static NumericalConstMathExpr Add(NumericalConstMathExpr a, NumericalConstMathExpr b)
        {
            return Add(new NumericalConstMathExpr[] { a, b });
        }

    }

}
