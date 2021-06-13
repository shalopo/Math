using System;
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
        //TODO: -1 should be reciprocal

        internal override bool RequiresPowScoping => true;

        public MathExpr Base { get; }
        public MathExpr Exponent { get; }


        public override string ToString() => $"{Base.ToPowScopedString()}^{Exponent.ToPowScopedString()}";

        internal override MathExpr Visit(IMathExprTransformer transformer) => PowerMathExpr.Create(
            Base.Visit(transformer), 
            Exponent.Visit(transformer));

        protected override MathExpr ReduceImpl(ReduceOptions options)
        {
            var base_reduced = Base.Reduce(options);
            var exponent_reduced = Exponent.Reduce(options);

            //TODO: bug with 0^0
            if (IsZero(exponent_reduced) || IsOne(base_reduced))
            {
                return ONE;
            }

            if (IsOne(exponent_reduced))
            {
                return base_reduced;
            }

            if (base_reduced == I && IsWholeNumber(exponent_reduced))
            {
                switch (Math.Abs(Convert.ToInt64(((ExactConstMathExpr)exponent_reduced).Value)) % 4)
                {
                    case 0: return ONE;
                    case 1: return I;
                    case 2: return MINUS_ONE;
                    case 3: return -I;
                    default: throw new Exception("Invalid quarter");
                }
            }

            if (IsZero(base_reduced) && exponent_reduced is NumericalConstMathExpr numerical_exponent)
            {
                if (numerical_exponent.IsPositive)
                {
                    return ZERO;
                }
                else
                {
                    throw new UndefinedMathBehavior($"Divide by zero, exponent:{numerical_exponent}");
                }
            }

            if (exponent_reduced is ExactConstMathExpr exponent_exact)
            {
                if (base_reduced is ExactConstMathExpr base_exact &&
                    IsWholeNumber(base_exact.Value) && Math.Abs(base_exact.Value) <= 1024 &&
                    IsWholeNumber(exponent_exact.Value) && Math.Abs(exponent_exact.Value) <= 20)
                {
                    try
                    {
                        double pow = Math.Pow(base_exact.Value, exponent_exact.Value);
                        if (IsWholeNumber(pow) && pow <= 1024 * 1024)
                        {
                            return Convert.ToInt64(pow);
                        }
                    }
                    catch (OverflowException)
                    {
                    }
                }

                var term = base_reduced.AsAdditiveTerm();

                if (!term.Coefficient.IsPositive)
                {
                    if (IsEven(exponent_exact.Value))
                    {
                        return Create((-base_reduced).Reduce(options), exponent_reduced);
                    }
                    else if (IsOdd(exponent_exact.Value))
                    {
                        return -Create((-base_reduced).Reduce(options), exponent_reduced);
                    }
                }
            }

            if (base_reduced is ConstFractionMathExpr base_fraction)
            {
                return (Create(base_fraction.Top, exponent_reduced) / Create(base_fraction.Bottom, exponent_reduced)).Reduce(options);
            }

            if (base_reduced is PowerMathExpr base_power)
            {
                return Create(base_power.Base, (base_power.Exponent * exponent_reduced).Reduce(options));
            }

            return Create(base_reduced, exponent_reduced);
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
                return Exponent * baseDerived * Create(Base, (Exponent - ONE).Reduce(ReduceOptions.DEFAULT));
            }

            additionTerms.Add(exponentDerived * LN(Base));

            if (!IsZero(baseDerived))
            {
                additionTerms.Add(Exponent * baseDerived / Base);
            }

            return MultMathExpr.Create(AddMathExpr.Create(additionTerms), this);
        }

        internal override PowerMathExpr AsPowerExpr() => this;

        public PowerMathExpr Reciprocate() => new PowerMathExpr(Base, (-Exponent).Reduce(ReduceOptions.DEFAULT));

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
