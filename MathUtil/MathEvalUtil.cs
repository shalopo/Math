using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class MathEvalUtil
    {
        public static long? AsWholeNumber(double value)
        {
            // Beyond this value (positive or negative), doubles lose the precision of whole numbers
            const double MAX_VALUE = 1E15;

            if (Math.Abs(value) >= MAX_VALUE)
            {
                return null;
            }

            long converted = Convert.ToInt64(value);

            return (Math.Abs(value - converted) <= (double.Epsilon * 100)) ? converted : null;
        }

        public static bool IsZero(MathExpr expr) => expr.Equals(GlobalMathDefs.ZERO);
        public static bool IsOne(MathExpr expr) => expr.Equals(GlobalMathDefs.ONE);

        public static double GetWeight(MathExpr expr) => expr.Weight;
        public static double SumWeights(IEnumerable<MathExpr> terms) => terms.Aggregate(0.0, (agg, expr) => agg + expr.Weight);

        public static bool IsWholeNumber(double value) => AsWholeNumber(value).HasValue;
        public static bool IsWholeNumber(MathExpr expr) => expr is ExactConstMathExpr exact && IsWholeNumber(exact.Value);

        public static bool IsEven(double value)
        {
            if (!IsWholeNumber(value))
            {
                return false;
            }

            try
            {
                return Convert.ToInt64(Math.Abs(value)) % 2 == 0;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        public static bool IsOdd(double value)
        {
            if (!IsWholeNumber(value))
            {
                return false;
            }

            try
            {
                return Convert.ToInt64(Math.Abs(value)) % 2 != 0;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        public static double CalcDistanceSquared(double dx, double dy) => dx * dx + dy * dy;
        public static double CalcDistance(double dx, double dy) => Math.Sqrt(CalcDistanceSquared(dx, dy));

        public static MathExpr Transform(MathExpr expr, IMathExprTransformer transformation)
        {
            return expr.Visit(transformation);
        }

        public static MathExpr Transform(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            return Transform(expr, new VariablesEvalTransformation(values)); 
        }

        public static MathExpr NumericalEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = Transform(expr, values);
            return evaled.Reduce(ReduceOptions.DEFAULT);
        }

        public static ConstComplexMathExpr ComplexEval(MathExpr expr)
        {
            return expr.ComplexEval();
        }

        public static ConstComplexMathExpr ComplexEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = Transform(expr, values);
            return ComplexEval(evaled);
        }

        public static MathExpr Reduce(MathExpr expr)
        {
            return expr.Reduce(ReduceOptions.DEFAULT.With(allowFullCoverageFactor: true));
        }

    }
}
