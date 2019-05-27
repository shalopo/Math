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

        public override string ToString() => "undef";

        internal override MathExpr Visit(IMathExprTransformer transformer) => Instance;

        public static UndefinedMathExpr Instance = new UndefinedMathExpr();
    }
}
