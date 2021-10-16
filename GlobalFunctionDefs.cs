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
        public static readonly ExactConstMathExpr ZERO = new(0);
        public static readonly ExactConstMathExpr ONE = new(1);
        public static readonly ExactConstMathExpr TWO = new(2);
        public static readonly ExactConstMathExpr FOUR = new(4);
        public static readonly ExactConstMathExpr MINUS_ONE = new(-1);
        public static readonly ExactConstMathExpr MINUS_TWO = new(-2);
        public static readonly ConstFractionMathExpr HALF = ConstFractionMathExpr.Create(1, 2);
        public static readonly ConstFractionMathExpr QUARTER = ConstFractionMathExpr.Create(1, 4);
        public static readonly KnownConstMathExpr E = new("e", Math.E);
        public static readonly KnownConstMathExpr PI = new("π", Math.PI);
        public static readonly ImaginaryMathExpr I = ImaginaryMathExpr.Instance;

        private static readonly Dictionary<string, MathFunctionDef> s_functions = new();
        public static IReadOnlyDictionary<string, MathFunctionDef> Functions => s_functions;

        private static MathFunctionDef RegisterFunc(MathFunctionDef f)
        {
            s_functions.Add(f.Name.ToLower(), f);
            return f;
        }

        public static MathFuncCaller SIN = RegisterFunc(new SinFunctionDef());
        public static MathFuncCaller COS = RegisterFunc(new CosFunctionDef());
        public static MathFuncCaller TAN = RegisterFunc(new TanFunctionDef());
        public static MathFuncCaller COT = RegisterFunc(new CotFunctionDef());
        public static MathFuncCaller ARCTAN = RegisterFunc(new ArcTanFunctionDef());
        public static MathFuncCaller ARCTAN2 = RegisterFunc(new ArcTan2FunctionDef());
        public static MathFuncCaller ARCSIN = RegisterFunc(new ArcSinFunctionDef());
        public static MathFuncCaller ARCCOS = RegisterFunc(new ArcCosFunctionDef());
        public static MathFuncCaller LN = RegisterFunc(new LnFunctionDef());
        public static MathFuncCaller SQRT = RegisterFunc(new SqrtFunctionDef());
        public static MathFuncCaller SQR = RegisterFunc(new SqrFunctionDef());

    }
}
