using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class MathVariable : MathExpr
    {
        public MathVariable(string name)
        {
            Name = name;
            Delta = new MathVariableDelta(this);
        }

        public string Name { get; }
        public MathVariableDelta Delta { get; }

        public override string ToString() => Name;

        internal override double Weight => 1;
        internal override bool RequiresPowScoping => false;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        internal override MathExpr Derive(MathVariable v) => v == this ? GlobalMathDefs.ONE : GlobalMathDefs.ZERO;

        internal override bool IsConst => false;
        internal override ConstComplexMathExpr ComplexEval() => throw new UndefinedMathBehavior("Cannot reduce");

        internal override MathExprMatch Match(MathExpr expr)
        {
            return new MathExprMatch(new VariablesTransformation((this, expr)));
        }
    }

    public class MathVariableDelta : MathExpr
    {
        public MathVariableDelta(MathVariable v) => Variable = v;

        public MathVariable Variable { get; }

        public override string ToString() => "d" + Variable.Name;

        internal override double Weight => 1;
        internal override bool RequiresPowScoping => false;
        
        internal override MathExpr Visit(IMathExprTransformer transformer) => throw new NotImplementedException();
        
        internal override MathExpr Derive(MathVariable v) => throw new UndefinedMathBehavior("Cannot derive " + ToString());

        internal override MathExprMatch Match(MathExpr expr) => throw new NotImplementedException();

        internal override bool IsConst => false;
        internal override ConstComplexMathExpr ComplexEval() => throw new UndefinedMathBehavior("Cannot reduce");
    }

}
