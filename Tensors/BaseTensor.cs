using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class BaseTensor : MathExpr
    {
        internal override double Weight => 1;

        internal override bool IsConst => throw new NotImplementedException();

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        internal override ConstComplexMathExpr ComplexEval()
        {
            throw new NotImplementedException();
        }

        internal override MathExpr Derive(MathVariable v)
        {
            throw new NotImplementedException();
        }

        internal override MathExpr Visit(IMathExprTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public static Tensor operator *(BaseTensor a, BaseTensor b)
        {
            throw new NotImplementedException();
        }
    }
}
