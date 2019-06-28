using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class KnownConstMathExpr : ConstMathExpr
    {
        public KnownConstMathExpr(string name, double value) => (Name, Exact) = (name, value);

        public string Name { get; }
        private ExactConstMathExpr Exact { get; }
        public double Value { get => Exact.Value; }

        internal override bool RequiresPowScoping => false;

        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Create(Exact, GlobalMathDefs.ZERO);

        public override string ToString() => Name;
    }
}
