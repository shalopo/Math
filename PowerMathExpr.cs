using System.Collections.Generic;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class LnFunctionDef : SingleArgMathFunctionDef
    {
        public override string Name => "ln";

        protected override MathExpr DeriveSingle() => 1 / x1;

        public override MathExpr TryReduce(MathExpr input)
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
            (IsOne(const_exponent) ? @base : (MathExpr)new PolynomPowerMathExpr(@base, const_exponent)) :
            (MathExpr)new GeneralPowerMathExpr(@base, exponent);

        public override bool RequiresScopingAsExponentBase => true;

        protected abstract MathExpr GetBase();
        protected abstract MathExpr GetExponent();

        public override string ToString() => (GetBase().RequiresScopingAsExponentBase ? $"({GetBase()})" : GetBase().ToString()) + "^" +
            ((GetExponent() is ConstMathExpr || GetExponent() is VariableMathExpr) ? GetExponent().ToString() : $"({GetExponent()})");

        public override MathExpr Transform(IMathExprTransformer transformer) => PowerMathExpr.Create(GetBase().Transform(transformer), GetExponent());

        public override MathExpr Reduce()
        {
            var base_reduced = GetBase().Reduce();
            var exponent_reduced = GetExponent().Reduce();

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

            return Create(base_reduced, exponent_reduced);
        }
    }

    internal class GeneralPowerMathExpr : PowerMathExpr
    {
        public GeneralPowerMathExpr(MathExpr @base, MathExpr exponent) => (Base, Exponent) = (@base, exponent);

        public MathExpr Base { get; }
        public MathExpr Exponent { get; }

        protected override MathExpr GetBase() => Base;
        protected override MathExpr GetExponent() => Exponent;

        public override MathExpr Derive(MathVariable v)
        {
            var base_derived = Base.Derive(v);
            var exponent_derived = Exponent.Derive(v);

            var addition_exprs = new List<MathExpr>();

            if (!IsZero(exponent_derived))
            {
                addition_exprs.Add(MultMathExpr.Create(exponent_derived, LN(Base)));
            }

            if (!IsZero(base_derived))
            {
                addition_exprs.Add(MultMathExpr.Create(Exponent, base_derived, Base.Pow(-1)));
            }

            return MultMathExpr.Create(AddMathExpr.Create(addition_exprs), this);
        }
    }

    internal class PolynomPowerMathExpr : PowerMathExpr
    {
        public PolynomPowerMathExpr(MathExpr @base, ConstMathExpr exponent) => (Base, Exponent) = (@base, exponent);

        public MathExpr Base { get; }
        public ConstMathExpr Exponent { get; }

        protected override MathExpr GetBase() => Base;
        protected override MathExpr GetExponent() => Exponent;

        public override MathExpr Derive(MathVariable v) => MultMathExpr.Create(new MathExpr[] {
            Exponent,
            Base.Derive(v),
            Create(Base, (Exponent - 1).Reduce())});
    }
}
