using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class MathEvalUtil
    {
        public static bool IsExact(MathExpr expr, double value) => expr is ExactConstMathExpr exact_const ? exact_const.Value == value : false;

        public static bool IsZero(MathExpr expr) => IsExact(expr, 0);
        public static bool IsOne(MathExpr expr) => IsExact(expr, 1);

        public static bool IsWholeNumber(double value) => Math.Abs(value % 1) <= (double.Epsilon * 100);

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

        public static bool IsNegative(MathExpr expr) => expr.AsMultTerm().Coefficient is ExactConstMathExpr exact && exact.Value < 0;

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

        public static MathExpr EvalReduce(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var evaled = Eval(expr, values);
            var reduced = Reduce(evaled);
            return reduced;
        }
    }
}
