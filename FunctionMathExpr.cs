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
        public override bool Equals(object obj) => obj is MathFunctionDef funcExpr && funcExpr.Name == Name;
        public override int GetHashCode() => 539060726 + Name.GetHashCode();

        public MathExpr TryReduce(MathExpr input, ReduceOptions options) => 
            TryReduceImpl(input, options.With(allowCommonFactorSearch: false, allowSearchIdentities: false));

        protected virtual MathExpr TryReduceImpl(MathExpr input, ReduceOptions options) => null;

        public abstract ConstComplexMathExpr ComplexEval(ConstComplexMathExpr ComplexEval);

        public static implicit operator Func<MathExpr, MathExpr>(MathFunctionDef func) => func.Call;

        public static MathVariable x1 = new("X");
    }

    abstract class SimpleMathFunctionDef : MathFunctionDef
    {
        public SimpleMathFunctionDef(string name) : base(name) { }

        public override sealed MathExpr Derive(MathVariable v) => (v == x1) ? DeriveSingle() : GlobalMathDefs.ZERO;

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
        public ExpandableMathFunctionDef(string name, MathExpr definition, MathVariable arg = null) : base(name) => 
            (Definition, Arg) = (definition, arg ?? x1);

        public MathVariable Arg { get; }

        public MathExpr Definition { get; }
        protected virtual bool PreferNonExpandedForm => false;

        public override string ToString() => Definition.ToString();
        public string Signature => $"{Name}({Arg})";

        public sealed override MathExpr Derive(MathVariable v)
        {
            if (v != Arg)
            { 
                return GlobalMathDefs.ZERO;
            }

            return CustomDerive() ?? Definition.Derive(v);
        }

        protected virtual MathExpr CustomDerive() => null;

        public ExpandableMathFunctionDef Reduce(ReduceOptions options) => new(Name, Definition.Reduce(options), Arg);

        public override ConstComplexMathExpr ComplexEval(ConstComplexMathExpr input) => EvalCall(input).ComplexEval();

        private MathExpr EvalCall(MathExpr input)
        {
            return Definition.Visit(new VariablesEvalTransformation((Arg, input))).Reduce(ReduceOptions.DEFAULT);
        }

        protected override MathExpr TryReduceImpl(MathExpr input, ReduceOptions options)
        {
            return PreferNonExpandedForm ? null : EvalCall(input);
        }
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
                   Func.Equals(expr.Func) &&
                   Input.Equals(expr.Input);
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
            if (expr is not FunctionCallMathExpr callExpr)
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
