using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class MathExprMatch
    {
        public MathExprMatch(VariablesTransformation transformation)
        {
            Transformation = transformation;
        }

        public bool IsTrivial => Transformation.IsTrivial;

        public MathExpr Transform(MathExpr expr)
        {
            return expr.Visit(Transformation);
        }

        public bool IsConsistent(MathExpr source, MathExpr target)
        {
            return Transform(source).Equals(target);
        }

        public static readonly MathExprMatch TRIVIAL = new(VariablesTransformation.TRIVIAL);

        private VariablesTransformation Transformation { get; set; }
    }

}
