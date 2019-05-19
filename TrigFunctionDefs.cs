using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class SinFunctionDef : SingleArgMathFunctionDef
    {
        public override string Name => "sin";
        protected override MathExpr DeriveSingle() => COS(x1);

        public override MathExpr TryReduce(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }
    }

    class CosFunctionDef : SingleArgMathFunctionDef
    {
        public override string Name => "cos";
        protected override MathExpr DeriveSingle() => -SIN(x1);

        public override MathExpr TryReduce(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ONE;
            }

            return null;
        }
    }

    class TanFunctionDef : ExpandableMathFunctionDef
    {
        public override string Name => "tan";
        public override MathExpr Definition => SIN(x1) / COS(x1);
    }

}
