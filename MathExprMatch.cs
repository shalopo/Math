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

        public static readonly MathExprMatch IDENTICAL = new MathExprMatch(VariablesTransformation.TRIVIAL);

        public VariablesTransformation Transformation { get; private set; }
    }

}
