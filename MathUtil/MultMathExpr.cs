using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class MultMathExpr : MathExpr, IEnumerable<MathExpr>
    {
        private MultMathExpr(IEnumerable<MathExpr> terms)
        {
            Terms = terms.ToList();
            _coefficient = NumericalConstMathExpr.Mult(Terms.OfType<NumericalConstMathExpr>());
        }

        public IReadOnlyList<MathExpr> Terms { get; }
        private readonly NumericalConstMathExpr _coefficient;

        public static MathExpr Create(params MathExpr[] terms) => Create(terms.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> terms)
        {
            return (terms.Count()) switch
            {
                0 => GlobalMathDefs.ONE,
                1 => terms.First(),
                _ => new MultMathExpr(terms)
            };
        }

        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            static void SortByWeight(List<MathExpr> exprs) => exprs.Sort((term1, term2) => term1.Weight.CompareTo(term2.Weight));

            var fractions = Terms.OfType<ConstFractionMathExpr>();

            var positivePowers = Terms.Where(term => term is not PowerMathExpr && term is not ConstFractionMathExpr).
                Concat(Terms.OfType<PowerMathExpr>().Where(term => !term.Exponent.Coefficient.IsNegative)).
                Concat(fractions.Where(f => f.Top != 1).Select(f => new ExactConstMathExpr(f.Top))).ToList();

            var negativePowers = Terms.
                OfType<PowerMathExpr>().Where(term => term.Exponent.Coefficient.IsNegative).Select(term => term.Reciprocate()).
                Concat(fractions.Select(f => new ExactConstMathExpr(f.Bottom))).ToList();

            if (IsReduced)
            {
                SortByWeight(positivePowers);
                SortByWeight(negativePowers);
            }

            var sb = new StringBuilder();

            sb.Append(JoinPositivePowers(positivePowers));

            if (negativePowers.Any())
            {
                if (!positivePowers.Any())
                {
                    sb.Append("1");
                }

                sb.Append("/");

                bool wrap = negativePowers.Count() > 1;

                if (wrap)
                {
                    sb.Append("(");
                }

                sb.Append(JoinNegativePowers(negativePowers));

                if (wrap)
                {
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        private static string JoinPositivePowers(IEnumerable<MathExpr> terms)
        {
            StringBuilder sb = new();

            if (terms.Any() && terms.First() is NumericalConstMathExpr numeric)
            {
                if (numeric.Equals(GlobalMathDefs.ONE))
                {
                    if (terms.Count() == 1)
                    {
                        sb.Append("1");
                    }
                }
                else if (numeric.Equals(GlobalMathDefs.MINUS_ONE))
                {
                    sb.Append(terms.Count() > 1 ? "-" : "-1");
                }
                else
                {
                    if (numeric.IsNegative)
                    {
                        sb.Append("-").Append(numeric.Negate().ToString());
                    }
                    else
                    {
                        sb.Append(numeric.ToString());
                    }

                    if (terms.Count() > 1)
                    {
                        sb.Append("*");
                    }
                }

                terms = terms.Skip(1);
            }

            sb.Append(string.Join("*", terms.Select(term => term.ToMultScopedString())));

            return sb.ToString();
        }

        private static string JoinNegativePowers(IEnumerable<MathExpr> terms)
        {
            return string.Join("*", terms.Select(term => term.ToPowScopedString()));
        }

        internal override MathExpr Derive(MathVariable v) => AddMathExpr.Create(
            from expr_to_derive_index in Enumerable.Range(0, Terms.Count)
            let derived_expr = Terms[expr_to_derive_index].Derive(v)
            where !MathEvalUtil.IsZero(derived_expr)
            select Create(
                (from other_expr_index in Enumerable.Range(0, Terms.Count)
                 where other_expr_index != expr_to_derive_index
                 select Terms[other_expr_index]).Prepend(derived_expr)
            ));

        protected override MathExpr ReduceImpl(ReduceOptions options) => MultReducer.Reduce(Terms, options);

        internal override double Weight => Terms.Aggregate(0.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Terms.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Mult(Terms.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => Create(Terms.Select(expr => expr.Visit(transformer)));

        internal override NumericalConstMathExpr Coefficient => _coefficient;

        public override bool Equals(object other) => (other is MultMathExpr other_mult) && EqualityUtil.Equals(Terms, other_mult.Terms);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Terms, 407977119);

        internal override MathExprMatch Match(MathExpr expr)
        {
            //TODO: more advanced identity matching, e.g. 2sin(x)cos(x) = sin(2x)
            return null;
        }

        public IEnumerator<MathExpr> GetEnumerator() => Terms.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
