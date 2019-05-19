using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    abstract class MathFunctionDef
    {



        public abstract string Name { get; }

        public abstract MathExpr Derive(MathVariable v);

        public FunctionCallMathExpr Call(MathExpr input) => new FunctionCallMathExpr(this, input);

        public virtual MathExpr TryReduce(MathExpr input) => null;

        public Func<MathExpr, MathExpr> GetFunctor() => Call;

        public static VariableMathExpr x1 = new VariableMathExpr(new MathVariable("@1"));
    }

    abstract class SingleArgMathFunctionDef : MathFunctionDef
    {
        public override sealed MathExpr Derive(MathVariable v) => (v == x1.Variable) ? DeriveSingle() : ExactConstMathExpr.ZERO;

        protected abstract MathExpr DeriveSingle();
    }

    abstract class ExpandableMathFunctionDef : MathFunctionDef
    {
        public abstract MathExpr Definition { get; }

        public override MathExpr Derive(MathVariable v) => Definition.Derive(v);
    }

    class FunctionCallMathExpr : MathExpr
    {
        public FunctionCallMathExpr(MathFunctionDef func, MathExpr input) => (Func, Input) = (func, input);

        public MathFunctionDef Func { get; }
        public MathExpr Input { get; }

        public override bool RequiresScopingAsExponentBase => false;

        public override MathExpr Derive(MathVariable v) =>
            Input.Derive(v) * 
            (Func.Derive(MathFunctionDef.x1)
                .Transform(new VariablesTransformation(new Dictionary<MathVariable, MathExpr>() { { MathFunctionDef.x1, Input } })));

        public override MathExpr Reduce() => Func.TryReduce(Input) ?? this;

        public override string ToString() => $"{Func.Name}({Input})";

        public override MathExpr Transform(IMathExprTransformer transformer) => new FunctionCallMathExpr(Func, Input.Transform(transformer));
    }
}
