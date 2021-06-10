using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public class ImaginaryMathExpr : ConstMathExpr
    {
        private ImaginaryMathExpr()
        {
        }

        public static ImaginaryMathExpr Instance = new ImaginaryMathExpr();

        internal override double Weight => 1;

        public override string ToString() => "i";

        internal override ConstComplexMathExpr ComplexEval() => ConstComplexMathExpr.Create(ZERO, ONE);
    }
}
