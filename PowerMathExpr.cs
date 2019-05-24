using System;
using System.Collections.Generic;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class LnFunctionDef : SimpleMathFunctionDef
    {
        public override string Name => "ln";

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

    abstract class PowerMathExpr : MathExpr
    {
        public static MathExpr Create(MathExpr @base, MathExpr exponent) => exponent is ConstMathExpr const_exponent ?
            (IsOne(const_exponent) ? @base : new PolynomPowerMathExpr(@base, const_exponent)) :
            new GeneralPowerMathExpr(@base, exponent);

        public override bool RequiresPowScoping => true;

        protected abstract MathExpr GeneralizedBase { get; }

        protected abstract MathExpr GeneralizedExponent { get; }

        public override string ToString() => $"{GeneralizedBase.ToPowScopedString()}^{GeneralizedExponent.ToPowScopedString()}";

        public override MathExpr Visit(IMathExprTransformer transformer) => PowerMathExpr.Create(
            GeneralizedBase.Visit(transformer), 
            GeneralizedExponent.Visit(transformer));

        public override MathExpr Reduce()
        {
            var base_reduced = GeneralizedBase.Reduce();
            var exponent_reduced = GeneralizedExponent.Reduce();

            //TODO: bug with 0^0
            if (IsZero(exponent_reduced) || IsOne(base_reduced))
            {
                return ExactConstMathExpr.ONE;
            }

            if (IsOne(exponent_reduced))
            {
                return base_reduced;
            }

            if (IsZero(base_reduced) && 
                exponent_reduced is ExactConstMathExpr exact_exponent && exact_exponent.Value != 0) // make sure it cannot be 0^0
            {
                return ExactConstMathExpr.ZERO;
            }

            //if (base_reduced is ExactConstMathExpr base_exact && exponent_reduced is ExactConstMathExpr exponent_exact)
            //{
            //    return Math.Pow(base_exact.Value, exponent_exact.Value);
            //}

            return Create(base_reduced, exponent_reduced);
        }

        public override bool Equals(object obj)
        {
            return obj is PowerMathExpr expr &&
                   EqualityComparer<MathExpr>.Default.Equals(GeneralizedBase, expr.GeneralizedBase) &&
                   EqualityComparer<MathExpr>.Default.Equals(GeneralizedExponent, expr.GeneralizedExponent);
        }

        public override int GetHashCode()
        {
            var hashCode = 252740318;
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpr>.Default.GetHashCode(GeneralizedBase);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpr>.Default.GetHashCode(GeneralizedExponent);
            return hashCode;
        }
    }

    internal class GeneralPowerMathExpr : PowerMathExpr
    {
        public GeneralPowerMathExpr(MathExpr @base, MathExpr exponent) => (Base, Exponent) = (@base, exponent);

        public MathExpr Base { get; }
        public MathExpr Exponent { get; }

        protected override MathExpr GeneralizedBase => Base;
        protected override MathExpr GeneralizedExponent => Exponent;

        public override MathExpr Derive(MathVariable v)
        {
            var base_derived = Base.Derive(v);
            var exponent_derived = Exponent.Derive(v);

            var addition_exprs = new List<MathExpr>();

            if (!IsZero(exponent_derived))
            {
                addition_exprs.Add(exponent_derived * LN(Base));
            }

            if (!IsZero(base_derived))
            {
                addition_exprs.Add(Exponent * base_derived / Base);
            }

            return MultMathExpr.Create(AddMathExpr.Create(addition_exprs), this);
        }
    }

    internal class PolynomPowerMathExpr : PowerMathExpr
    {
        public PolynomPowerMathExpr(MathExpr @base, ConstMathExpr exponent) => (Base, Exponent) = (@base, exponent);

        public MathExpr Base { get; }
        public ConstMathExpr Exponent { get; }

        protected override MathExpr GeneralizedBase => Base;
        protected override MathExpr GeneralizedExponent => Exponent;

        public override MathExpr Derive(MathVariable v) => MultMathExpr.Create(new MathExpr[] {
            Exponent,
            Base.Derive(v),
            Create(Base, (Exponent - 1).Reduce())});
    }
}
