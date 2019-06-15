﻿using System;
using System.Collections.Generic;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class LnFunctionDef : SimpleMathFunctionDef
    {
        public LnFunctionDef() : base("ln") { }

        protected override MathExpr DeriveSingle() => 1 / x1;

        protected override MathExpr TryReduceImpl(MathExpr input)
        {
            if (input == KnownConstMathExpr.E)
            {
                return ExactConstMathExpr.ONE;
            }

            if (IsOne(input))
            {
                return ExactConstMathExpr.ZERO;
            }

            return null;
        }
    }

    class SqrFunctionDef : ExpandableMathFunctionDef
    {
        public SqrFunctionDef() : base("sqr", x1.Pow(2)) { }
    }

    class SqrtFunctionDef : ExpandableMathFunctionDef
    {
        public SqrtFunctionDef() : base("sqrt", x1.Pow(0.5)) { }
    }

    class PowerMathExpr : MathExpr
    {
        public PowerMathExpr(MathExpr @base, MathExpr exponent) => (Base, Exponent) = (@base, exponent);
        public static MathExpr Create(MathExpr @base, MathExpr exponent) => IsOne(exponent) ? @base : new PowerMathExpr(@base, exponent);

        internal override bool RequiresPowScoping => true;

        public MathExpr Base { get; }
        public MathExpr Exponent { get; }


        public override string ToString() => $"{Base.ToPowScopedString()}^{Exponent.ToPowScopedString()}";

        internal override MathExpr Visit(IMathExprTransformer transformer) => PowerMathExpr.Create(
            Base.Visit(transformer), 
            Exponent.Visit(transformer));

        protected override MathExpr ReduceImpl()
        {
            var base_reduced = Base.Reduce();
            var exponent_reduced = Exponent.Reduce();

            //TODO: bug with 0^0
            if (IsZero(exponent_reduced) || IsOne(base_reduced))
            {
                return ExactConstMathExpr.ONE;
            }

            if (IsOne(exponent_reduced))
            {
                return base_reduced;
            }

            if (IsZero(base_reduced) && exponent_reduced is NumericalConstMathExpr numerical_exponent)
            {
                if (numerical_exponent.IsPositive)
                {
                    return ExactConstMathExpr.ZERO;
                }
                else
                {
                    throw new UndefinedMathBehavior($"Divide by zero, exponent:{numerical_exponent}");
                }
            }

            //TODO: numerical const
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

                var term = base_reduced.AsMultTerm();

                if (!term.Coefficient.IsPositive)
                {
                    if (IsEven(exponent_exact.Value))
                    {
                        return Create((-base_reduced).Reduce(), exponent_reduced);
                    }
                    else if (IsOdd(exponent_exact.Value))
                    {
                        return -Create((-base_reduced).Reduce(), exponent_reduced);
                    }
                }
            }

            if (base_reduced is PowerMathExpr base_power)
            {
                return Create(base_power.Base, (base_power.Exponent * exponent_reduced).Reduce());
            }

            return Create(base_reduced, exponent_reduced);
        }

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
            var base_derived = Base.Derive(v);
            var exponent_derived = Exponent.Derive(v);

            var addition_exprs = new List<MathExpr>();

            if (IsZero(exponent_derived))
            {
                return Exponent * base_derived * Create(Base, (Exponent - 1).Reduce());
            }

            addition_exprs.Add(exponent_derived * LN(Base));

            if (!IsZero(base_derived))
            {
                addition_exprs.Add(Exponent * base_derived / Base);
            }

            return MultMathExpr.Create(AddMathExpr.Create(addition_exprs), this);
        }

        internal override PowerMathExpr AsPowerExpr() => this;

        public PowerMathExpr Reciprocate() => new PowerMathExpr(Base, (-Exponent).Reduce());
    }
}
