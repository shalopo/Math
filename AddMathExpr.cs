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

        protected override MathExpr ReduceImpl() => AddReducer.Reduce(Exprs, true);

        internal override double Weight => Exprs.Aggregate(0.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Exprs.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Add(Exprs.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Exprs.Select(expr => expr.Visit(transformer)));

        public override bool Equals(object other) => (other is AddMathExpr other_add) && EqualityUtil.Equals(Exprs, other_add.Exprs);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Exprs, 982734678);
    }
}
