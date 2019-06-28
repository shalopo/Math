using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    using MathFuncCaller = Func<MathExpr, MathExpr>;

    public static class GlobalMathDefs
    {
        public static readonly ExactConstMathExpr ZERO = new ExactConstMathExpr(0);
        public static readonly ExactConstMathExpr ONE = new ExactConstMathExpr(1);
        public static readonly ExactConstMathExpr TWO = new ExactConstMathExpr(2);
        public static readonly ExactConstMathExpr MINUS_ONE = new ExactConstMathExpr(-1);
        public static readonly ConstFractionMathExpr HALF = ConstFractionMathExpr.Create(1, 2);
        public static readonly KnownConstMathExpr E = new KnownConstMathExpr("e", Math.E);
        public static readonly KnownConstMathExpr PI = new KnownConstMathExpr("π", Math.PI);
        public static readonly ImaginaryMathExpr I = ImaginaryMathExpr.Instance;

        public static MathFuncCaller SIN = new SinFunctionDef();
        public static MathFuncCaller COS = new CosFunctionDef();
        public static MathFuncCaller TAN = new TanFunctionDef();
        public static MathFuncCaller ARCTAN = new ArcTanFunctionDef();
        public static MathFuncCaller ARCTAN2 = new ArcTan2FunctionDef();
        public static MathFuncCaller ARCSIN = new ArcSinFunctionDef();
        public static MathFuncCaller ARCCOS = new ArcCosFunctionDef();
        public static MathFuncCaller LN = new LnFunctionDef();
        public static MathFuncCaller SQR = new SqrFunctionDef();
        public static MathFuncCaller SQRT = new SqrtFunctionDef();
    }
}
