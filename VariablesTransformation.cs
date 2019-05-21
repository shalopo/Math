using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class VariablesTransformation : IMathExprTransformer
    {
        public VariablesTransformation(params (MathVariable v, MathExpr transformed)[] transformation) :
            this(transformation.ToDictionary(t => t.v, t => t.transformed))
        {
        }

        public VariablesTransformation(IReadOnlyDictionary<MathVariable, MathExpr> transformation) => Transformation = transformation;

        public MathExpr Transform(VariableMathExpr v) => Transformation.TryGetValue(v, out MathExpr expr) ? expr : v;

        public MathExpr Transform(MathExpr expr) => expr is VariableMathExpr var_expr ? Transform(var_expr) : expr;

        IReadOnlyDictionary<MathVariable, MathExpr> Transformation { get; }
    }
}
