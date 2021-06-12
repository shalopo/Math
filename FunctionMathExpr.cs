using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public abstract class MathFunctionDef
    {
        protected MathFunctionDef(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract MathExpr Derive(MathVariable v);

        public MathExpr Call(MathExpr input) => new FunctionCallMathExpr(this, input);

        public override string ToString() => Name;

        public abstract MathExpr TryReduce(MathExpr input, ReduceOptions options);

        public abstract ConstComplexMathExpr ComplexEval(ConstComplexMathExpr ComplexEval);
        
        public static implicit operator Func<MathExpr, MathExpr>(MathFunctionDef func) => func.Call;

        public static MathVariable x1 = new MathVariable("x");
    }

    abstract class SimpleMathFunctionDef : MathFunctionDef
    {
        public SimpleMathFunctionDef(string name) : base(name) { }

        public override sealed MathExpr Derive(MathVariable v) => (v == x1) ? DeriveSingle() : GlobalMathDefs.ZERO;

        public override sealed MathExpr TryReduce(MathExpr input, ReduceOptions options) => TryReduceImpl(input, options);

        protected virtual MathExpr TryReduceImpl(MathExpr input, ReduceOptions options) => null;

        protected abstract MathExpr DeriveSingle();

        public sealed override ConstComplexMathExpr ComplexEval(ConstComplexMathExpr input)
        {
            if (input.HasImagPart)
            {
                throw new UndefinedMathBehavior("Complex not supported");
            }

            return ConstComplexMathExpr.Create(ExactEval(input.Real.ToDouble()), GlobalMathDefs.ZERO);
        }

        public abstract double ExactEval(double input);
    }

    public class ExpandableMathFunctionDef : MathFunctionDef
    {
        public ExpandableMathFunctionDef(string name, MathExpr definition) : base(name) => Definition = definition;

        public MathExpr Definition { get; }

        public override string ToString() => Definition.ToString();

        public override MathExpr Derive(MathVariable v) => Definition.Derive(v);

        public ExpandableMathFunctionDef Reduce(ReduceOptions options) => 
            new ExpandableMathFunctionDef(Name, Definition.Reduce(options));

        public override ConstComplexMathExpr ComplexEval(ConstComplexMathExpr input) => EvalCall(input).ComplexEval();

        private MathExpr EvalCall(MathExpr input) => Definition.Visit(new VariablesEvalTransformation((x1, input)));

        public override sealed MathExpr TryReduce(MathExpr input, ReduceOptions options) => 
            TryReduceImpl(input, options) ?? EvalCall(input).Reduce(options);

        protected virtual MathExpr TryReduceImpl(MathExpr input, ReduceOptions options) => null;
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
                return GlobalMathDefs.ZERO;
            }

            return input_derived * 
                Func.Derive(MathFunctionDef.x1).Visit(new VariablesEvalTransformation((MathFunctionDef.x1, Input)));
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
            hashCode = hashCode * -1521134295 + Func.GetHashCode();
            hashCode = hashCode * -1521134295 + Input.GetHashCode();
            return hashCode;
        }

        protected override MathExpr ReduceImpl(ReduceOptions options)
        {
            var input_reduced = Input.Reduce(options);
            return Func.TryReduce(input_reduced, options) ?? new FunctionCallMathExpr(Func, input_reduced);
        }

        internal override double Weight => Input.Weight + 1;
        internal override bool IsConst => Input.IsConst;
        internal override ConstComplexMathExpr ComplexEval() => Func.ComplexEval(Input.ComplexEval());

        public override string ToString() => $"{Func.Name}({Input})";

        internal override MathExpr Visit(IMathExprTransformer transformer) => new FunctionCallMathExpr(Func, Input.Visit(transformer));

        internal override MathExprMatch Match(MathExpr expr)
        {
            if (!(expr is FunctionCallMathExpr callExpr))
            {
                return null;
            }

            if (callExpr.Func != Func)
            {
                return null;
            }

            return Input.Match(callExpr.Input);
        }

    }
}
