﻿using System;
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

        public MathExpr Reduce()
        {
            if (IsReduced)
            {
                return this;
            }

            var reduced = ReduceImpl();
            reduced.IsReduced = true;
            return reduced;
        }

        internal abstract MathExprMatch Match(MathExpr expr);

        protected virtual MathExpr ReduceImpl()
        {
            return this;
        }

        internal abstract double Weight { get; }
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

        internal virtual MultTerm AsMultTerm() => new MultTerm(this, ONE);
        internal virtual PowerMathExpr AsPowerExpr() => new PowerMathExpr(this, ONE);

        internal abstract MathExpr Visit(IMathExprTransformer transformer);

        public static implicit operator MathExpr(double value) => new ExactConstMathExpr(value);

        public static MathExpr operator +(MathExpr a, MathExpr b) => AddMathExpr.Create(a, b);
        public static MathExpr operator -(MathExpr a) => MINUS_ONE * a;
        public static MathExpr operator -(MathExpr a, MathExpr b) => AddMathExpr.Create(a, -b);
        public static MathExpr operator *(MathExpr a, MathExpr b) => MultMathExpr.Create(a, b);
        public static MathExpr operator /(MathExpr a, MathExpr b) => a * ReciprocalMathExpr.Create(b);

        // the ^ operator does not fit due to non-matching order of operations
        public MathExpr Pow(MathExpr exponent) => PowerMathExpr.Create(this,exponent);

        internal static readonly MathExpr[] EMPTY_ARRAY = new MathExpr[0];

        private bool IsReduced { get; set; } = false;
    }

}
