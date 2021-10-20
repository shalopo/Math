using System;
using System.Linq;
using System.Collections.Generic;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    sealed class LnFunctionDef : SimpleMathFunctionDef
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

    sealed class SqrFunctionDef : ExpandableMathFunctionDef
    {
        public SqrFunctionDef() : base("sqr", x1.Pow(TWO)) { }
    }

    sealed class SqrtFunctionDef : ExpandableMathFunctionDef
    {
        public SqrtFunctionDef() : base("sqrt", x1.Pow(HALF)) { }
    }

    sealed class PowerMathExpr : MathExpr
    {
        public PowerMathExpr(MathExpr @base, MathExpr exponent) => (Base, Exponent) = (@base, exponent);
        public static MathExpr Create(MathExpr @base, MathExpr exponent) => IsOne(exponent) ? @base : new PowerMathExpr(@base, exponent);

        internal override bool RequiresPowScoping => false;

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
                long quarter = (Convert.ToInt64(((ExactConstMathExpr)exponentReduced).Value)) % 4;

                if (quarter < 0)
                {
                    quarter += 4;
                }

                return quarter switch
                {
                    0 => ONE,
                    1 => I,
                    2 => MINUS_ONE,
                    3 => -I,
                    _ => throw new Exception("Invalid quarter"),
                };
            }

            // TODO: e^(i*pi*n)

            if (IsZero(baseReduced) && exponentReduced is NumericalConstMathExpr numericalExponent)
            {
                if (numericalExponent.IsNegative)
                {
                    throw new UndefinedMathBehavior($"Divide by zero, exponent:{numericalExponent}");
                }
                else
                {
                    return ZERO;
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

                if (baseReduced.Coefficient.IsNegative)
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

            if (exponentReduced.Equals(MINUS_ONE))
            {
                if (baseReduced is MultMathExpr multBase)
                {
                    return MultMathExpr.Create(multBase.Select(term => term.Pow(exponentReduced).Reduce(ReduceOptions.LIGHT)));
                }
                else if (baseReduced is ExactConstMathExpr baseExact && baseExact.IsWholeNumber)
                {
                    return ConstFractionMathExpr.Create(top: 1, bottom: baseExact.AsWholeNumber.Value);
                }
            }

            if (baseReduced is ConstFractionMathExpr base_fraction)
            {
                if (exponentReduced is NumericalConstMathExpr expNumerical && expNumerical.IsNegative)
                {
                    return Create(base_fraction.Reciprocate(), expNumerical.Negate());
                }
                else
                {
                    return (Create(base_fraction.Top, exponentReduced) / Create(base_fraction.Bottom, exponentReduced));
                }
            }

            if (baseReduced is PowerMathExpr basePower)
            {
                return Create(basePower.Base, (basePower.Exponent * exponentReduced).Reduce(options));
            }

            return Create(baseReduced, exponentReduced);
        }

        internal override double Weight => Base.Weight + Exponent.Weight;
        internal override bool IsConst => Base.IsConst && Exponent.IsConst;
        internal override ConstComplexMathExpr ComplexEval() => Base.ComplexEval().EvalPow(Exponent.ComplexEval());

        public override bool Equals(object obj)
        {
            return obj is PowerMathExpr expr && 
                   Base.Equals(expr.Base) &&
                   Exponent.Equals(expr.Exponent);
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
            if (expr is not PowerMathExpr powerExpr)
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
