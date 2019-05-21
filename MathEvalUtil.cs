using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class MathEvalUtil
    {
        public static bool IsExact(MathExpr expr, double value) => expr is ExactConstMathExpr exact_const ? exact_const.Value == value : false;

        public static bool IsZero(MathExpr expr) => IsExact(expr, 0);
        public static bool IsOne(MathExpr expr) => IsExact(expr, 1);

        public static bool IsWholeNumber(double value) => Math.Abs(value % 1) <= (double.Epsilon * 100);

        public static MathExpr Eval(MathExpr expr, params (MathVariable v, MathExpr value)[] values)
        {
            var transformed = expr.Visit(new VariablesTransformation(values));
            var reduced = transformed.Reduce();
            return reduced;
        }
    }
}
