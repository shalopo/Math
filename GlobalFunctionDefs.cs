using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    using MathFuncCaller = Func<MathExpr, MathExpr>;

    public static class GlobalFunctionDefs
    {
        public static MathFuncCaller SIN = new SinFunctionDef();
        public static MathFuncCaller COS = new CosFunctionDef();
        public static MathFuncCaller TAN = new TanFunctionDef();
        public static MathFuncCaller LN = new LnFunctionDef();
        public static MathFuncCaller SQR = new SqrFunctionDef();
    }
}
