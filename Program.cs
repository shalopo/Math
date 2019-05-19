using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.KnownConstMathExpr;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    class Program
    {
        static ExactConstMathExpr _(double value) => new ExactConstMathExpr(value);

        static void Main(string[] args)
        {
            var x = new VariableMathExpr(new MathVariable("x"));

            var f = E.Pow(x.Pow(4) * 2);

            Console.WriteLine($"f    = {f}");
            Console.WriteLine($"f(0) = {Eval(f, (x, 0))}");

            f = f.Reduce();
            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();

            var f_derived = f.Derive(x);

            Console.WriteLine($"f'  = {f_derived}");
            Console.WriteLine($"f'* = {f_derived.Reduce()}");

            Console.ReadLine();
        }
    }
}
