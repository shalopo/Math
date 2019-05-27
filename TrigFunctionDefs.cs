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

            if (input == KnownConstMathExpr.PI)
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

            if (input == KnownConstMathExpr.PI)
            {
                return ExactConstMathExpr.MINUS_ONE;
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
