using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class VariablesTransformation : IMathExprTransformer
    {
        public VariablesTransformation(params (MathVariable v, MathExpr transformed)[] transformations)
        {
            Sources = transformations.Select(t => t.v).ToList().AsReadOnly();
            Transformations = new ReadOnlyDictionary<MathVariable, MathExpr>(transformations.ToDictionary(t => t.v, t => t.transformed));
        }

        public static readonly VariablesTransformation TRIVIAL = new();

        public MathExpr Transform(MathVariable v) => Transformations.TryGetValue(v, out MathExpr expr) ? expr : v;

        public MathExpr Transform(MathExpr expr) => expr is MathVariable v ? Transform(v) : expr;

        public MathExpr this[MathVariable v] => Transformations[v];

        public ReadOnlyCollection<MathVariable> Sources { get; }
        private ReadOnlyDictionary<MathVariable, MathExpr> Transformations { get; }

        public bool IsTrivial => Transformations.Count == 0;
    }

    public class VariablesEvalTransformation : VariablesTransformation
    {
        public VariablesEvalTransformation(params (MathVariable v, MathExpr transformed)[] transformation) :
            base(transformation)
        {
        }
    }

    public class VariablesChangeTransformation : VariablesTransformation
    {
        public VariablesChangeTransformation(MathVariable[] targets, params (MathVariable v, MathExpr transformed)[] transformation) :
            base(transformation)
        {
            Targets = Array.AsReadOnly(targets);
        }

        public ReadOnlyCollection<MathVariable> Targets { get; }
    }
}
