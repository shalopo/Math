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
               //x * SIN(E.Pow(x))
            //x.Pow(x).Pow(x)
            //x* x*x + x * x + x * x
            //x.Pow(2) + x.Pow(3) + _2.Pow(2) + _2.Pow(x) + x.Pow(x) + x/x + x.Pow(1) + _1.Pow(x)
             //COS(1) + COS(1)
             ;

            var base_input = 1;

            Console.WriteLine($"f    = {f}");
            //Console.WriteLine($"f(0) = {Eval(f, (x, 0))}");

            f = f.Reduce();

            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();

            int derivative_number = 1;
            var derived = DerivativeUtil.Derive(f, x, derivative_number);
            Console.WriteLine($"d^{derivative_number} f / dx^{derivative_number}  = {derived}");
            Console.WriteLine($"derived(0) = {MathEvalUtil.Eval(derived, (x, base_input))}");

            var taylor = TaylorExpansionUtil.Expand(f, x, base_input, derivative_number);
            Console.WriteLine($"taylor  = {taylor}");

            Console.ReadLine();
        }
    }
}
