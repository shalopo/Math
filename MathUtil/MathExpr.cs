using MathUtil.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public abstract class MathExpr
    {
        internal virtual bool RequiresMultScoping => false;
        internal virtual bool RequiresPowScoping => false;

        internal string ToMultScopedString() => RequiresMultScoping ? $"({this})" : ToString();
        internal string ToPowScopedString() => RequiresPowScoping ? $"({this})" : ToString();

        public abstract override string ToString();

        internal abstract MathExpr Derive(MathVariable v);

        public MathExpr Reduce(ReduceOptions options)
        {
            if (IsReduced)
            {
                return this;
            }

            var reduced = ReduceImpl(options);
            reduced.IsReduced = true;
            return reduced;
        }

        internal abstract MathExprMatch Match(MathExpr expr);

        protected virtual MathExpr ReduceImpl(ReduceOptions options)
        {
            return this;
        }

        internal abstract double WeightImpl { get; }
        private double? _weightLazy;

        internal double Weight
        {
            get
            {
                if (!_weightLazy.HasValue)
                {
                    _weightLazy = WeightImpl;
                }

                return _weightLazy.Value;
            }
        }

        internal abstract bool IsConst { get; }

        // TODO: extend this to eval into anything (e.g. consider matrices)
        internal abstract ConstComplexMathExpr ComplexEval();

        internal NumericalConstMathExpr RealEval()
        {
            var complex = ComplexEval();

            if (complex.HasImagPart)
            {
                throw new UndefinedMathBehavior($"Not real: {complex}");
            }

            return complex.Real;
        }

        internal virtual NumericalConstMathExpr Coefficient => ONE;
        internal virtual PowerMathExpr AsPowerExpr() => new(this, ONE);

        internal abstract MathExpr Visit(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => MINUS_ONE * a;
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * b.Pow(MINUS_ONE);

        // the ^ operator does not fit due to non-matching order of operations
        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        internal static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];

        protected bool IsReduced { get; set; } = false;

        private MathExpr[] _singleExprArrayLazy;

        internal IEnumerable<MathExpr> AsSingleExprEnumerable()
        {
            if (_singleExprArrayLazy == null)
            {
                _singleExprArrayLazy = new MathExpr[] { this };
            }

            return _singleExprArrayLazy;
        }

        internal virtual IEnumerable<MathExpr> AsAdditiveTerms() => AsSingleExprEnumerable();
        internal virtual IEnumerable<MathExpr> AsMultTerms() => AsSingleExprEnumerable();
    }

}
