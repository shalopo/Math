using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class MathVariable : MathExpr
    {
        public MathVariable(string name) => Name = name;

        public string Name { get; }

        public override string ToString() => Name;

        internal override bool RequiresPowScoping => false;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        internal override MathExpr Derive(MathVariable v) => v == this ? GlobalMathDefs.ONE : GlobalMathDefs.ZERO;

        internal override bool IsConst => false;
        internal override ConstComplexMathExpr ComplexEval() => throw new UndefinedMathBehavior("Cannot reduce");
    }
}
