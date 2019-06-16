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

            if (input.IsConst && (2 * input / PI).Reduce() is ExactConstMathExpr exact && IsWholeNumber(exact))
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

        public override double ExactEval(double input) => Math.Sin(input);
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

        public override double ExactEval(double input) => Math.Cos(input);
    }

    class TanFunctionDef : ExpandableMathFunctionDef
    {
        public TanFunctionDef() : base("tan", SIN(x1) / COS(x1))
        {
        }
    }

    class ArcTanFunctionDef : SimpleMathFunctionDef
    {
        public ArcTanFunctionDef() : base("arctan")
        {
        }

        protected override MathExpr DeriveSingle() => 1 / (1 + SQR(x1));

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Atan(input);
    }

    class ArcSinFunctionDef : SimpleMathFunctionDef
    {
        public ArcSinFunctionDef() : base("arcsin")
        {
        }

        protected override MathExpr DeriveSingle() => 1 / SQRT(1 - SQR(x1));

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (IsZero(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Asin(input);
    }

    class ArcCosFunctionDef : SimpleMathFunctionDef
    {
        public ArcCosFunctionDef() : base("arccos")
        {
        }

        protected override MathExpr DeriveSingle() => -1 / SQRT(1 - SQR(x1));

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (IsOne(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Acos(input);
    }


}
