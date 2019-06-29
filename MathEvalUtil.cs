using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class MathEvalUtil
    {
        private static bool IsConvertibleToLong(double value)
        {
            try
            {
                Convert.ToInt64(value);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        public static bool IsZero(MathExpr expr) => expr.Equals(GlobalMathDefs.ZERO);
        public static bool IsOne(MathExpr expr) => expr.Equals(GlobalMathDefs.ONE);

        public static bool IsWholeNumber(double value) => Math.Abs(value % 1) <= (double.Epsilon * 100) && IsConvertibleToLong(value);
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

        public static bool IsPositive(MathExpr expr) => expr.AsMultTerm().Coefficient.IsPositive;

        public static double CalcDistanceSquared(double dx, double dy) => dx * dx + dy * dy;
        public static double CalcDistance(double dx, double dy) => Math.Sqrt(CalcDistanceSquared(dx, dy));

        public static MathExpr Reduce(MathExpr expr)
        {
            return expr.Reduce();
        }

        internal static MathExpr EvalTransformVariables(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            return expr.Visit(new VariablesEvalTransformation(values)); 
        }

        public static MathExpr NumericalEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = EvalTransformVariables(expr, values);
            return evaled.Reduce();
        }

        public static ConstComplexMathExpr ComplexEval(MathExpr expr)
        {
            return expr.ComplexEval();
        }

        public static ConstComplexMathExpr ComplexEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = EvalTransformVariables(expr, values);
            return ComplexEval(evaled);
        }
    }
}
