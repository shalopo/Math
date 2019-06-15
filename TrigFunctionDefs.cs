using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.KnownConstMathExpr;
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

            if (!IsPositive(input))
            {
                var minus_input = (-input).Reduce();
                return -(TryReduceImpl(minus_input) ?? SIN(minus_input));
            }

            if ((2 * input / PI).Reduce() is ExactConstMathExpr exact && IsWholeNumber(exact))
            {
                switch (Convert.ToInt64(exact.Value) % 4)
                {
                    case 0:
                    case 2:
                        return 0;
                    case 1:
                        return 1;
                    case 3:
                        return -1;
                }
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

            if (!IsPositive(input))
            {
                var minus_input = (-input).Reduce();
                return TryReduceImpl((-input).Reduce()) ?? COS(minus_input);
            }

            return new SinFunctionDef().TryReduce(input + ConstFractionMathExpr.HALF * PI);
        }
    }

    class TanFunctionDef : ExpandableMathFunctionDef
    {
        public TanFunctionDef() : base("tan", SIN(x1) / COS(x1))
        {
        }
    }
    

}
