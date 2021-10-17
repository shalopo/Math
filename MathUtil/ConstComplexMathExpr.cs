using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.MathEvalUtil;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public sealed class ConstComplexMathExpr : ConstMathExpr
    {
        private ConstComplexMathExpr(NumericalConstMathExpr real, NumericalConstMathExpr imag) =>
            (Real, Imag, ReducedAddExpr) = (real, imag, ToAddExpr(real, imag));

        private static MathExpr ToAddExpr(NumericalConstMathExpr real, NumericalConstMathExpr imag)
        {
            if (IsZero(imag))
            {
                return real;
            }

            var imag_with_i = IsOne(imag) ? I : imag * I;

            return IsZero(real) ? imag_with_i : AddMathExpr.Create(real, imag_with_i);
        }

        public static ConstComplexMathExpr Create(NumericalConstMathExpr real, NumericalConstMathExpr imag) => new(real, imag);

        public static ConstComplexMathExpr CreatePolar(double size, double phase) => Create(size * Math.Cos(phase), size * Math.Sin(phase));

        public NumericalConstMathExpr Real { get; }
        public NumericalConstMathExpr Imag { get; }
        public MathExpr ReducedAddExpr { get; }

        public bool HasImagPart => !(IsZero(Imag));
        public bool HasRealPart => !(IsZero(Real));

        internal override double Weight => ReducedAddExpr.Weight;
        internal override bool RequiresMultScoping => ReducedAddExpr.RequiresMultScoping;
        internal override bool RequiresPowScoping => ReducedAddExpr.RequiresPowScoping;
        public override string ToString() => ReducedAddExpr.ToString();

        internal override ConstComplexMathExpr ComplexEval() => this;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        public override bool Equals(object obj)
        {
            return (obj is ConstComplexMathExpr expr && Real.Equals(expr.Real) && Imag.Equals(expr.Imag)) || 
                   (IsZero(Imag) &&  obj is ExactConstMathExpr exact &&  exact.Equals(Real));
        }

        public override int GetHashCode()
        {
            var hashCode = -1656204448;
            hashCode = hashCode * -1521134295 + Real.GetHashCode();
            hashCode = hashCode * -1521134295 + Imag.GetHashCode();
            return hashCode;
        }

        public double SizeSquared => CalcDistanceSquared(Real.ToDouble(), Imag.ToDouble());
        public double Size => Math.Sqrt(SizeSquared);

        private MathExpr PhaseExpr => ARCTAN2(this);
        public double Phase => PhaseExpr.RealEval().ToDouble();

        public ConstComplexMathExpr Negate() => Create(Real.Negate(), Imag.Negate());
        public ConstComplexMathExpr Conjugate() => Create(Real, Imag.Negate());
        public ConstComplexMathExpr Reciprocate() => Mult(Conjugate(), new ConstComplexMathExpr(1 / SizeSquared, ZERO));

        public static ConstComplexMathExpr Add(IEnumerable<ConstComplexMathExpr> terms) => Create(
            NumericalConstMathExpr.Add(terms.Select(expr => expr.Real)),
            NumericalConstMathExpr.Add(terms.Select(expr => expr.Imag)));

        public static ConstComplexMathExpr Mult(IEnumerable<ConstComplexMathExpr> terms) => 
            terms.Aggregate(ONE_COMPLEX, (agg, expr) => Mult(agg, expr));

        public static ConstComplexMathExpr Mult(ConstComplexMathExpr a, ConstComplexMathExpr b) =>
            Create(NumericalConstMathExpr.Add(NumericalConstMathExpr.Mult(a.Real, b.Real), NumericalConstMathExpr.Mult(a.Imag, b.Imag).Negate()),
                   NumericalConstMathExpr.Add(NumericalConstMathExpr.Mult(a.Real, b.Imag), NumericalConstMathExpr.Mult(a.Imag, b.Real)));

        public ConstComplexMathExpr EvalPow(ConstComplexMathExpr exponent)
        {
            var size_sqr = SizeSquared;

            if (size_sqr == 0)
            {
                if (exponent.SizeSquared == 0)
                {
                    throw new UndefinedMathBehavior("0^0 is undefined");
                }

                return ZERO_COMPLEX;
            }

            var c = exponent.Real.ToDouble();

            if (!exponent.HasImagPart)
            {
                if (IsWholeNumber(c))
                {
                    var whole_exponent = Convert.ToInt64(c);
                    if (Math.Abs(whole_exponent) <= 30)
                    {
                        var result = Mult(Enumerable.Range(0, Math.Abs((int)whole_exponent)).Select(i => this));
                        if (whole_exponent < 0)
                        {
                            return result.Reciprocate();
                        }
                    }
                }

                var reduceOptions = ReduceOptions.DEFAULT;

                var phase = PhaseExpr;
                var result_phase = (c * phase).Reduce(reduceOptions);
                var result_size = Math.Pow(size_sqr, c / 2);
                return Create((result_size * COS(result_phase)).Reduce(reduceOptions).RealEval(), 
                              (result_size * SIN(result_phase)).Reduce(reduceOptions).RealEval());
            }
            else
            {
                var d = exponent.Imag.ToDouble();
                var phase = Phase;
                var result_phase = d / 2 * Math.Log(size_sqr) + c * phase;
                var result_size = Math.Pow(size_sqr, c / 2) / Math.Pow(Math.E, d * phase);
                return CreatePolar(result_size, result_phase);
            }
        }

        public static readonly ConstComplexMathExpr ZERO_COMPLEX = Create(ZERO, ZERO);
        private static readonly ConstComplexMathExpr ONE_COMPLEX = Create(ONE, ZERO);
    }
}
