using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class UndefinedMathExpr : MathExpr
    {
        internal override MathExpr Derive(MathVariable v) => throw new NotImplementedException("Cannot derive undefined");

        public override string ToString() => "undefined";

        internal override MathExpr Visit(IMathExprTransformer transformer) => Instance;

        internal override bool IsConst => throw new NotImplementedException("Undefined is neither const or non-const");
        internal override double ExactEval() => throw new NotImplementedException("Cannot reduce undefined");

        public static UndefinedMathExpr Instance = new UndefinedMathExpr();
    }
}
