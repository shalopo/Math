using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    static class GlobalFunctionDefs
    {
        public static Func<MathExpr, MathExpr> SIN = new SinFunctionDef().GetFunctor();
        public static Func<MathExpr, MathExpr> COS = new CosFunctionDef().GetFunctor();
        public static Func<MathExpr, MathExpr> TAN = new TanFunctionDef().GetFunctor();
        public static Func<MathExpr, MathExpr> LN = new LnFunctionDef().GetFunctor();
    }
}
