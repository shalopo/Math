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
        static ExactConstMathExpr _0 = _(0);
        static ExactConstMathExpr _1 = _(1);
        static ExactConstMathExpr _2 = _(2);
        static ExactConstMathExpr _3 = _(3);
        static ExactConstMathExpr _4 = _(4);
        static ExactConstMathExpr _5 = _(5);

        static void Main(string[] args)
        {
            var x = new VariableMathExpr(new MathVariable("x"));

            MathExpr f = 
               (_3/(_5/_3))
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
