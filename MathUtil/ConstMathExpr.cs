
using System;
using System.Linq;
using System.Collections.Generic;

namespace MathUtil
{

    public abstract class ConstMathExpr : MathExpr
    {
        internal override MathExpr Derive(MathVariable v) => GlobalMathDefs.ZERO;

        internal override MathExpr Visit(IMathExprTransformer transformer) => transformer.Transform(this);

        internal override bool IsConst => true;

        internal override MathExprMatch Match(MathExpr expr)
        {
            return Equals(expr) ? MathExprMatch.IDENTICAL : null;
            //return new MathExprMatch((ConstMathExpr)((this / constExpr).Reduce()), VariablesTransformation.TRIVIAL);
        }
    }

}
