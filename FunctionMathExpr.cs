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

        public abstract MathExpr TryReduce(MathExpr input);

        public Func<MathExpr, MathExpr> GetFunctor() => Call;

        public static VariableMathExpr x1 = new VariableMathExpr(new MathVariable("@1"));
    }

    abstract class SimpleMathFunctionDef : MathFunctionDef
    {
        public override sealed MathExpr Derive(MathVariable v) => (v == x1.Variable) ? DeriveSingle() : ExactConstMathExpr.ZERO;

        public override sealed MathExpr TryReduce(MathExpr input) => TryReduceImpl(input);

        protected virtual MathExpr TryReduceImpl(MathExpr input) => null;

        protected abstract MathExpr DeriveSingle();
    }

    class ExpandableMathFunctionDef : MathFunctionDef
    {
        public ExpandableMathFunctionDef(string name, MathExpr definition) => (m_name, Definition) = (name, definition);

        protected string m_name;

        public override string Name => m_name;

        public MathExpr Definition { get; }

        public override MathExpr Derive(MathVariable v) => Definition.Derive(v);

        public override sealed MathExpr TryReduce(MathExpr input) => 
            TryReduceImpl(input) ?? 
            Definition.Visit(new VariablesTransformation((x1, input))).Reduce();

        protected virtual MathExpr TryReduceImpl(MathExpr input) => null;
    }

    class FunctionCallMathExpr : MathExpr
    {
        public FunctionCallMathExpr(MathFunctionDef func, MathExpr input) => (Func, Input) = (func, input);

        public MathFunctionDef Func { get; }
        public MathExpr Input { get; }

        internal override bool RequiresPowScoping => false;

        internal override MathExpr Derive(MathVariable v)
        {
            var input_derived = Input.Derive(v);
            if (MathEvalUtil.IsZero(input_derived))
            {
                return ExactConstMathExpr.ZERO;
            }

            return input_derived * 
                Func.Derive(MathFunctionDef.x1).Visit(new VariablesTransformation((MathFunctionDef.x1, Input)));
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionCallMathExpr expr &&
                   EqualityComparer<MathFunctionDef>.Default.Equals(Func, expr.Func) &&
                   EqualityComparer<MathExpr>.Default.Equals(Input, expr.Input);
        }

        public override int GetHashCode()
        {
            var hashCode = 1879674458;
            hashCode = hashCode * -1521134295 + EqualityComparer<MathFunctionDef>.Default.GetHashCode(Func);
            hashCode = hashCode * -1521134295 + EqualityComparer<MathExpr>.Default.GetHashCode(Input);
            return hashCode;
        }

        internal override MathExpr Reduce()
        {
            var input_reduced = Input.Reduce();
            return Func.TryReduce(input_reduced) ?? new FunctionCallMathExpr(Func, input_reduced);
        }

        public override string ToString() => $"{Func.Name}({Input})";

        internal override MathExpr Visit(IMathExprTransformer transformer) => new FunctionCallMathExpr(Func, Input.Visit(transformer));


    }
}
