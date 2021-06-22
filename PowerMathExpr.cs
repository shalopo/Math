using System;
using System.Linq;
using System.Collections.Generic;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class LnFunctionDef : SimpleMathFunctionDef
    {
        public LnFunctionDef() : base("ln") { }

        protected override MathExpr DeriveSingle() => x1.Pow(MINUS_ONE);

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            if (input == E)
            {
                return ONE;
            }

            if (IsOne(input))
            {
                return ZERO;
            }

            return null;
        }

        public override double ExactEval(double input) => Math.Log(input);
    }

    class SqrFunctionDef : ExpandableMathFunctionDef
    {
        public SqrFunctionDef() : base("sqr", x1.Pow(TWO)) { }
    }

    class SqrtFunctionDef : ExpandableMathFunctionDef
    {
        public SqrtFunctionDef() : base("sqrt", x1.Pow(HALF)) { }
    }

    class PowerMathExpr : MathExpr
    {
        public PowerMathExpr(MathExpr @base, MathExpr exponent) => (Base, Exponent) = (@base, exponent);
        public static MathExpr Create(MathExpr @base, MathExpr exponent) => IsOne(exponent) ? @base : new PowerMathExpr(@base, exponent);

        internal override bool RequiresPowScoping => true;

        public MathExpr Base { get; }
        public MathExpr Exponent { get; }


        public override string ToString()
        {
            if (Exponent.Equals(MINUS_ONE))
            {
                return $"1/{Base.ToPowScopedString()}";
            }

            return $"{Base.ToPowScopedString()}^{Exponent.ToPowScopedString()}";
        }

        internal override MathExpr Visit(IMathExprTransformer transformer) => PowerMathExpr.Create(
            Base.Visit(transformer), 
            Exponent.Visit(transformer));

        protected override MathExpr ReduceImpl(ReduceOptions options)
        {
            var baseReduced = Base.Reduce(options);
            var exponentReduced = Exponent.Reduce(options);

            //TODO: bug with 0^0
            if (IsZero(exponentReduced) || IsOne(baseReduced))
            {
                return ONE;
            }

            if (IsOne(exponentReduced))
            {
                return baseReduced;
            }

            if (baseReduced == I && IsWholeNumber(exponentReduced))
            {
                switch (Math.Abs(Convert.ToInt64(((ExactConstMathExpr)exponentReduced).Value)) % 4)
                {
                    case 0: return ONE;
                    case 1: return I;
                    case 2: return MINUS_ONE;
                    case 3: return -I;
                    default: throw new Exception("Invalid quarter");
                }
            }

            if (IsZero(baseReduced) && exponentReduced is NumericalConstMathExpr numericalExponent)
            {
                if (numericalExponent.IsPositive)
                {
                    return ZERO;
                }
                else
                {
                    throw new UndefinedMathBehavior($"Divide by zero, exponent:{numericalExponent}");
                }
            }

            if (exponentReduced is ExactConstMathExpr exponentExact)
            {
                if (baseReduced is ExactConstMathExpr base_exact &&
                    IsWholeNumber(base_exact.Value) && Math.Abs(base_exact.Value) <= 1024 &&
                    IsWholeNumber(exponentExact.Value) && Math.Abs(exponentExact.Value) <= 20)
                {
                    try
                    {
                        double pow = Math.Pow(base_exact.Value, exponentExact.Value);
                        if (IsWholeNumber(pow) && pow <= 1024 * 1024)
                        {
                            return Convert.ToInt64(pow);
                        }
                    }
                    catch (OverflowException)
                    {
                    }
                }

                var term = baseReduced.AsAdditiveTerm();

                if (!term.Coefficient.IsPositive)
                {
                    if (IsEven(exponentExact.Value))
                    {
                        return Create((-baseReduced).Reduce(options), exponentReduced);
                    }
                    else if (IsOdd(exponentExact.Value))
                    {
                        return -Create((-baseReduced).Reduce(options), exponentReduced);
                    }
                }
            }

            if (baseReduced is ConstFractionMathExpr base_fraction)
            {
                return (Create(base_fraction.Top, exponentReduced) / Create(base_fraction.Bottom, exponentReduced)).Reduce(options);
            }

            if (baseReduced is PowerMathExpr basePower)
            {
                return Create(basePower.Base, (basePower.Exponent * exponentReduced).Reduce(options));
            }

            if (baseReduced is MultMathExpr multBase)
            {
                if (exponentReduced.Equals(MINUS_ONE))
                {
                    return MultMathExpr.Create(multBase.Select(term => term.Pow(exponentReduced).Reduce(ReduceOptions.LIGHT)));
                }
            }

            return Create(baseReduced, exponentReduced);
        }

        internal override double Weight => Base.Weight + Exponent.Weight;
        internal override bool IsConst => Base.IsConst && Exponent.IsConst;
        internal override ConstComplexMathExpr ComplexEval() => Base.ComplexEval().EvalPow(Exponent.ComplexEval());

        public override bool Equals(object obj)
        {
            return obj is PowerMathExpr expr &&
                   EqualityComparer<MathExpr>.Default.Equals(Base, expr.Base) &&
                   EqualityComparer<MathExpr>.Default.Equals(Exponent, expr.Exponent);
        }

        public override int GetHashCode()
        {
            var hashCode = 252740318;
            hashCode = hashCode * -1521134295 + Base.GetHashCode();
            hashCode = hashCode * -1521134295 + Exponent.GetHashCode();
            return hashCode;
        }

        internal override MathExpr Derive(MathVariable v)
        {
            var baseDerived = Base.Derive(v);
            var exponentDerived = Exponent.Derive(v);

            var additionTerms = new List<MathExpr>();

            if (IsZero(exponentDerived))
            {
                return Exponent * baseDerived * Create(Base, (Exponent - ONE).Reduce(ReduceOptions.LIGHT));
            }

            additionTerms.Add(exponentDerived * LN(Base));

            if (!IsZero(baseDerived))
            {
                additionTerms.Add(Exponent * baseDerived / Base);
            }

            return MultMathExpr.Create(AddMathExpr.Create(additionTerms), this);
        }

        internal override PowerMathExpr AsPowerExpr() => this;

        //TODO: undesired reduction - need to reduce only the minus
        public MathExpr Reciprocate() => Create(Base, (-Exponent).Reduce(ReduceOptions.LIGHT));

        internal override MathExprMatch Match(MathExpr expr)
        {
            if (!(expr is PowerMathExpr powerExpr))
            {
                return null;
            }

            var match = Base.Match(powerExpr.Base);
            
            if (match != null)
            {
                return match;
            }
            
            match = Exponent.Match(powerExpr.Exponent);
            
            return match;
        }
    }
}
