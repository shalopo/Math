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
             //E.Pow(x)
             //_2/(_4*3)
             //-_2 * (-_4)  / (-4*x*E.Pow(x))
             //E.Pow(SIN(x))
             //SIN(E.Pow(x) - 1)
             (_1 / _2) + _(0.5)
             //SIN(-PI/(_1/2) + 0.5*PI + 0.5*PI)
             //SQRT(SIN(E.Pow(x) + PI/2 - 1))
             ;

            var base_input = 0;

            Console.WriteLine($"f    = {f}");

            f = Reduce(f);
            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();
            Console.WriteLine($"f(0) = {MathEvalUtil.EvalReduce(f, (x, base_input))}");

            Console.WriteLine();

            if (!(f is UndefinedMathExpr))
            {
                int derivative_number = 1;
                var derived = DerivativeUtil.Derive(f, x, derivative_number);
                Console.WriteLine($"d^{derivative_number} f / dx^{derivative_number}  = {derived}");
                Console.WriteLine();
                Console.WriteLine($"derived(0) = {MathEvalUtil.EvalReduce(derived, (x, base_input))}");
                Console.WriteLine();

                int taylor_derivatives = 30;
                var taylor = TaylorExpansionUtil.Expand(f, x, base_input, taylor_derivatives);
                Console.WriteLine($"taylor  = {taylor}");
            }

            Console.ReadLine();
        }
    }
}
