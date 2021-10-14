using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    sealed class SinFunctionDef : SimpleMathFunctionDef
    {
        public SinFunctionDef() : base("sin") { }

        private static readonly MathExpr SIN_QUARTER_PI = new ExactConstMathExpr(2).Pow(-new ExactConstMathExpr(1) / 2);

        protected override MathExpr DeriveSingle() => COS(x1);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (IsZero(input))
            {
                return ZERO;
            }

            if (!IsPositive(input))
            {
                return (-SIN(-input)).Reduce(options);
            }

            if (input.IsConst && (FOUR * input / PI).Reduce(options) is ExactConstMathExpr exact && IsWholeNumber(exact))
            {
                switch (Convert.ToInt64(exact.Value) % 8)
                {
                    case 0: return ZERO;
                    case 1: return SIN_QUARTER_PI;
                    case 2: return ONE;
                    case 3: return SIN_QUARTER_PI;
                    case 4: return ZERO;
                    case 5: return -SIN_QUARTER_PI;
                    case 6: return MINUS_ONE;
                    case 7: return -SIN_QUARTER_PI;
                }
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Sin(input);
    }

    sealed class CosFunctionDef : SimpleMathFunctionDef
    {
        public CosFunctionDef() : base("cos") { }
        
        protected override MathExpr DeriveSingle() => -SIN(x1);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (IsZero(input))
            {
                return ONE;
            }

            if (!IsPositive(input))
            {
                return COS(-input).Reduce(options);
            }

            return new SinFunctionDef().TryReduce((input + HALF * PI).Reduce(options), options);
        }

        public override double ExactEval(double input) => Math.Cos(input);
    }

    sealed class TanFunctionDef : ExpandableMathFunctionDef
    {
        public TanFunctionDef() : base("tan", SIN(x1) / COS(x1))
        {
        }

        protected override bool PreferNonExpandedForm => true;
    }

    sealed class CotFunctionDef : ExpandableMathFunctionDef
    {
        public CotFunctionDef() : base("cot", COS(x1) / SIN(x1))
        {
        }

        protected override bool PreferNonExpandedForm => true;
    }

    sealed class ArcTanFunctionDef : SimpleMathFunctionDef
    {
        public ArcTanFunctionDef() : base("arctan")
        {
        }

        protected override MathExpr DeriveSingle() => (ONE + x1.Pow(2)).Pow(MINUS_ONE);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (IsZero(input))
            {
                return ZERO;
            }

            if (IsOne(input))
            {
                return QUARTER * PI;
            }

            if (!IsPositive(input))
            {
                return (-ARCTAN(-input)).Reduce(options);
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Atan(input);
    }

    sealed class ArcTan2FunctionDef : MathFunctionDef
    {
        public ArcTan2FunctionDef() : base("arctan2")
        {
        }

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            switch (input)
            {
                case NumericalConstMathExpr numerical:
                    return TryReduce(ConstComplexMathExpr.Create(numerical, ZERO), options);

                case ConstComplexMathExpr z:
                    if (!z.HasImagPart)
                    {
                        if (!z.HasRealPart)
                        {
                            return ZERO;
                        }

                        return z.Real.IsPositive ? (MathExpr)ZERO : PI;
                    }

                    if (!z.HasRealPart)
                    {
                        return z.Imag.IsPositive ? (PI / 2) : (3 * PI / 2);
                    }

                    if (z.Real.Equals(z.Imag))
                    {
                        return z.Real.IsPositive ? (PI / 4) : (5 * PI / 4);
                    }
                    else if (z.Real.Equals(z.Imag.Negate()))
                    {
                        return z.Real.IsPositive ? (3 * PI / 4) : (- PI / 4);
                    }

                    if (!z.Imag.IsPositive)
                    {
                        return TryReduce(z.Conjugate(), options) ?? -ARCTAN2(z.Conjugate());
                    }

                    break;
            }

            return null;
        }

        public override ConstComplexMathExpr ComplexEval(ConstComplexMathExpr z) => ConstComplexMathExpr.Create(
            Math.Atan2(z.Imag.ToDouble(), z.Real.ToDouble()), 
            ZERO);

        public override MathExpr Derive(MathVariable v) => throw new NotImplementedException("Derivative of arctan2 is not implemented");
    }

    sealed class ArcSinFunctionDef : SimpleMathFunctionDef
    {
        public ArcSinFunctionDef() : base("arcsin")
        {
        }

        protected override MathExpr DeriveSingle() => (ONE - x1.Pow(2)).Pow(-HALF);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (IsZero(input))
            {
                return ZERO;
            }

            if (IsOne(input))
            {
                return HALF * PI;
            }

            if (!IsPositive(input))
            {
                return (-ARCSIN(-input)).Reduce(options);
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Asin(input);
    }

    sealed class ArcCosFunctionDef : SimpleMathFunctionDef
    {
        public ArcCosFunctionDef() : base("arccos")
        {
        }

        protected override MathExpr DeriveSingle() => -(ONE - x1.Pow(2)).Pow(-HALF);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (IsZero(input))
            {
                return HALF * PI;
            }

            if (IsOne(input))
            {
                return ZERO;
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Acos(input);
    }

}
