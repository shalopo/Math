using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathUtil;
using static MathUtil.GlobalFunctionDefs;
using static MathUtil.KnownConstMathExpr;
using static MathUtil.MathEvalUtil;

namespace MathTest
{
    class Program
    {
        static ExactConstMathExpr _(double value) => new ExactConstMathExpr(value);

        static void Main(string[] args)
        {
            var x = new VariableMathExpr(new MathVariable("x"));

            MathExpr f = 
               E.Pow(x)*(x.Pow(2) / 3)
            ;

            Console.WriteLine($"f    = {f}");
            //Console.WriteLine($"f(0) = {Eval(f, (x, 0))}");

            f = f.Reduce();

            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();

            var f_derived = DerivativeUtil.Derive(f, x);
            Console.WriteLine($"f'  = {f_derived}");

            var f_derived2 = DerivativeUtil.Derive(f_derived, x);
            Console.WriteLine($"f''  = {f_derived2}");
            Console.WriteLine($"f''(1) = {Eval(f_derived2, (x, 0))}");

            var taylor = TaylorExpansionUtil.Expand(f, x, 0, 6);
            Console.WriteLine($"taylor  = {taylor}");

            Console.ReadLine();
        }
    }
}
