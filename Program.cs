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
        static ExactConstMathExpr _(long value) => new ExactConstMathExpr(value);

        static void Main(string[] args)
        {
            var x = new VariableMathExpr(new MathVariable("x"));

            var f = SIN(x);


            Console.WriteLine($"f    = {f}");
            Console.WriteLine($"f(0) = {Eval(f, (x, 0))}");

            //var f_reduced = f.Reduce();

            //Console.WriteLine($"f*  = {f_reduced}");
            //Console.WriteLine();

            //var f_derived = f_reduced.Derive(x);

            //Console.WriteLine($"f'  = {f_derived}");
            //Console.WriteLine($"f'* = {f_derived.Reduce()}");

            Console.ReadLine();
        }
    }
}
