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
            (Real, Imag, AddExpr) = (real, imag, ((AddMathExpr)(real + imag * I)).ReduceAdd(false));

        public static ConstComplexMathExpr Create(NumericalConstMathExpr real, NumericalConstMathExpr imag) => new ConstComplexMathExpr(real, imag);

        public static ConstComplexMathExpr CreatePolar(double size, double arg) => Create(size * Math.Cos(arg), size * Math.Sin(arg));

        public static ConstComplexMathExpr CreatePolar(double size, MathExpr arg) => Create(
            size * COS(arg).Reduce().RealEval().ToDouble(), 
            size * SIN(arg).Reduce().RealEval().ToDouble());


        public NumericalConstMathExpr Real { get; }
        public NumericalConstMathExpr Imag { get; }
        MathExpr AddExpr { get; }

        public bool HasImagPart => !(IsZero(Imag));
        public bool HasRealPart => !(IsZero(Real));

        internal override bool RequiresMultScoping => AddExpr.RequiresMultScoping;
        internal override bool RequiresPowScoping => AddExpr.RequiresPowScoping;
        public override string ToString() => AddExpr.ToString();

        internal override ConstComplexMathExpr ComplexEval() => this;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        public override bool Equals(object obj)
        {
            return obj is ConstComplexMathExpr expr &&
                   Real.Equals(expr.Real) &&
                   Imag.Equals(expr.Imag);
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

        private MathExpr ArgExpr => ARCTAN2(this);
        public double Arg => ArgExpr.RealEval().ToDouble();

        public ConstComplexMathExpr Negate() => Create(Real.Negate(), Imag.Negate());
        public ConstComplexMathExpr Conjugate() => Create(Real, Imag.Negate());
        public ConstComplexMathExpr Reciprocate() => Mult(Conjugate(), new ConstComplexMathExpr(1 / SizeSquared, ZERO));

        public static ConstComplexMathExpr Add(IEnumerable<ConstComplexMathExpr> exprs) => Create(
            NumericalConstMathExpr.Add(exprs.Select(expr => expr.Real)),
            NumericalConstMathExpr.Add(exprs.Select(expr => expr.Imag)));

        public static ConstComplexMathExpr Mult(IEnumerable<ConstComplexMathExpr> exprs) => 
            exprs.Aggregate(ONE_COMPLEX, (agg, expr) => Mult(agg, expr));

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
                    var whole_exponent = Convert.ToUInt64(c);
                    if (whole_exponent <= 30)
                    {
                        return Mult(Enumerable.Range(0, (int)whole_exponent).Select(i => this));
                    }
                }

                var arg = ArgExpr;
                var result_arg = (c * arg).Reduce();
                var result_size = Math.Pow(size_sqr, c / 2);
                return Create((result_size * COS(result_arg)).Reduce().RealEval(), 
                              (result_size * SIN(result_arg)).Reduce().RealEval());
            }
            else
            {
                var d = exponent.Imag.ToDouble();
                var arg = Arg;
                var result_arg = d / 2 * Math.Log(size_sqr) + c * arg;
                var result_size = Math.Pow(size_sqr, c / 2) / Math.Pow(Math.E, d * arg);
                return CreatePolar(result_size, result_arg);
            }
        }

        private static ConstComplexMathExpr ZERO_COMPLEX = Create(ZERO, ZERO);
        private static ConstComplexMathExpr ONE_COMPLEX = Create(ONE, ZERO);
    }
}
