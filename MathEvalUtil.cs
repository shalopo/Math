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

        public static bool IsZero(MathExpr expr) => expr.Equals(ExactConstMathExpr.ZERO);
        public static bool IsOne(MathExpr expr) => expr.Equals(ExactConstMathExpr.ONE);

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

        public static MathExpr Reduce(MathExpr expr)
        {
            try
            {
                return expr.Reduce();
            }
            catch (UndefinedMathBehavior)
            {
                return UndefinedMathExpr.Instance;
            }
        }

        internal static MathExpr Eval(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            return expr.Visit(new VariablesTransformation(values)); 
        }

        public static MathExpr NumericalEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = Eval(expr, values);

            try
            {
                return evaled.Reduce();
            }
            catch (UndefinedMathBehavior)
            {
                return UndefinedMathExpr.Instance;
            }
        }

        public static double ExactEval(MathExpr expr)
        {
            return expr.ExactEval();
        }


        public static double ExactEvalWith(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = Eval(expr, values);
            return ExactEval(evaled);
        }
    }
}
