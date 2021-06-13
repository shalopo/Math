using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    class AddMathExpr : MathExpr, IEnumerable<MathExpr>
    {
        private AddMathExpr(IEnumerable<MathExpr> terms) => Terms = terms.ToList().AsReadOnly();

        public IReadOnlyList<MathExpr> Terms { get; }

        public static MathExpr Create(params MathExpr[] terms) => Create(terms.AsEnumerable());

        public static MathExpr Create(IEnumerable<MathExpr> terms)
        {
            switch (terms.Count())
            {
                case 0: return GlobalMathDefs.ZERO;
                case 1: return terms.First();
                default: return new AddMathExpr(terms.SelectMany(expr => (expr is AddMathExpr addExpr)?  
                                                addExpr.Terms : new[]{ expr }));
            }
        }

        internal override bool RequiresMultScoping => true;
        internal override bool RequiresPowScoping => true;

        public override string ToString()
        {
            var sb = new StringBuilder(Terms[0].AsMultTerm().ToString());

            foreach (var expr in Terms.Skip(1))
            {
                sb.Append(" ").Append(expr.AsMultTerm().ToAddedString());
            }

            return sb.ToString();
        }

        internal override MathExpr Derive(MathVariable v) => Create(Terms.Select(expr => expr.Derive(v)));

        protected override MathExpr ReduceImpl(ReduceOptions options) => AddReducer.Reduce(Terms, options);

        internal override double Weight => Terms.Aggregate(0.0, (agg, expr) => agg + expr.Weight);
        internal override bool IsConst => Terms.All(expr => expr.IsConst);
        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Add(Terms.Select(expr => expr.ComplexEval()));

        internal override MathExpr Visit(IMathExprTransformer transformer) => AddMathExpr.Create(Terms.Select(expr => expr.Visit(transformer)));

        public override bool Equals(object other) => (other is AddMathExpr other_add) && EqualityUtil.Equals(Terms, other_add.Terms);
        public override int GetHashCode() => EqualityUtil.GetHashCode(Terms, 982734678);

        internal override MathExprMatch Match(MathExpr expr)
        {
            //TODO: more advanced identity matching, e.g. sin(x) + i * cos(x) = e^(i*x)
            return null;
        }

        public IEnumerator<MathExpr> GetEnumerator() => Terms.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
