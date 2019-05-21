using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class MathVariable
    {
        public MathVariable(string name) => Name = name;

        public String Name { get; }

        public override string ToString() => Name;
    }

    class VariableMathExpr : MathExpr
    {
        public VariableMathExpr(MathVariable v) => Variable = v;

        public MathVariable Variable { get; }

        public static implicit operator MathVariable(VariableMathExpr expr) => expr.Variable;

        public override bool RequiresScopingAsExponentBase => false;

        public override string ToString() => Variable.ToString();

        public override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        public override MathExpr Derive(MathVariable v) => v == Variable ? ExactConstMathExpr.ONE : ExactConstMathExpr.ZERO;
    }
}
