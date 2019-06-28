using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class AddMathExpr : MathExpr
    {
        private AddMathExpr(IEnumerable<MathExpr> exprs) => Exprs = exprs.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Exprs { get; }

        public static MathExpr Create(params MathExpr[] exprs) => Create(exprs.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> exprs)
        {
            switch (exprs.Count())
            {
                case 0: return GlobalMathDefs.ZERO;
                case 1: return exprs.First();
                default: return new AddMathExpr(exprs);
            }
        }

        internal override bool RequiresMultScoping => true;
        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            var sb = new StringBuilder(Exprs[0].AsMultTerm().ToString());

            foreach (var expr in Exprs.Skip(1))
            {
                sb.Append(" ").Append(expr.AsMultTerm().ToAddedString());
            }

            return sb.ToString();
        }

        internal override MathExpr Derive(MathVariable v) => Create(Exprs.Select(expr => expr.Derive(v)));

        protected override MathExpr ReduceImpl() => ReduceAdd(true);

        public MathExpr ReduceAdd(bool allow_reduce_to_const_complex = true)
        {
            var exprs = (from expr in Exprs
                         let expr_reduced = expr.Reduce()
                         where !MathEvalUtil.IsZero(expr_reduced)
                         select expr_reduced is AddMathExpr add_expr ? add_expr.Exprs : new MathExpr[] { expr_reduced }
                ).SelectMany(s => s);

            var multiples = new Dictionary<MathExpr, List<MathExpr>>();

            foreach (var expr in exprs)
            {
                if (!(expr is NumericalConstMathExpr))
                {
                    var term = expr.AsMultTerm();

                    if (multiples.ContainsKey(term.Expr))
                    {
                        multiples[term.Expr].Add(term.Coefficient);
                    }
                    else
                    {
                        multiples.Add(term.Expr, new List<MathExpr> { term.Coefficient });
                    }
                }
            }

            var @const = NumericalConstMathExpr.Add(exprs.OfType<NumericalConstMathExpr>());

            exprs = (from item in multiples
                     let expr = item.Key
                     let multiple = Create(item.Value).Reduce()
                     where !MathEvalUtil.IsZero(expr)
                     select MathEvalUtil.IsOne(expr) ? multiple :
                            MathEvalUtil.IsOne(multiple) ? expr : (multiple * expr).Reduce());

            if (allow_reduce_to_const_complex && exprs.Count() == 1)
            {
                var expr = exprs.First();
                if (expr.IsConst && (!(expr is ConstMathExpr) || expr.Equals(ImaginaryMathExpr.Instance)))
                {
                    var complex = expr.ComplexEval();
                    var real_part = (@const + complex.Real).RealEval();
                    return complex.HasImagPart ? ConstComplexMathExpr.Create(real_part, complex.Imag) : (MathExpr)real_part;
                }
            }

            if (!MathEvalUtil.IsZero(@const))
            {
                exprs = exprs.Append(@const);
            }

            return Create(exprs);
        }

        internal override bool IsConst => Exprs.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Add(Exprs.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));

        public override bool Equals(object other) => (other is AddMathExpr other_add) && EqualityUtil.Equals(Exprs, other_add.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 982734678);
    }
}
