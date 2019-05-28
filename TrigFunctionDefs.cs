using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class SinFunctionDef : SimpleMathFunctionDef
    {
        public SinFunctionDef() : base("sin") { }

        protected override MathExpr DeriveSingle() => COS(x1);

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            if (IsNegative(input))
            {
                var minus_input = (-input).Reduce();
                return -(TryReduceImpl(minus_input) ?? SIN(minus_input));
            }

            if ((2 * input).Reduce().Equals(KnownConstMathExpr.PI))
            {
                return ExactConstMathExpr.ONE;
            }

            if (input.Equals(KnownConstMathExpr.PI))
            {
                return ExactConstMathExpr.ZERO;
            }

            if (input.Equals(2 * KnownConstMathExpr.PI))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }
    }

    class CosFunctionDef : SimpleMathFunctionDef
    {
        public CosFunctionDef() : base("cos") { }
        
        protected override MathExpr DeriveSingle() => -SIN(x1);

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ONE;
            }

            if (IsNegative(input))
            {
                return TryReduceImpl((-input).Reduce());
            }

            if (input == KnownConstMathExpr.PI)
            {
                return ExactConstMathExpr.MINUS_ONE;
            }

            if ((2*input).Reduce().Equals(KnownConstMathExpr.PI))
            {
                return ExactConstMathExpr.ZERO;
            }

            if (input.Equals(2 * KnownConstMathExpr.PI))
            {
                return ExactConstMathExpr.ONE;
            }

            return null;
        }
    }

    class TanFunctionDef : ExpandableMathFunctionDef
    {
        public TanFunctionDef() : base("tan", SIN(x1) / COS(x1))
        {
        }
    }
    

}
