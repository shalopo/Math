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
        static ExactConstMathExpr _6 = _(6);

        static void Main(string[] args)
        {
            var x = MathFunctionDef.x1;

            var f = new ExpandableMathFunctionDef("f",
            //0.5 -E.Pow(-2 * x)
            //_2/(_4*3)
            //-_2 * (-_4)  / (-4*x*E.Pow(x))
            //E.Pow(1 / (1 - x))
            LN(x + 1)
            //(x+1).Pow(_1 / 3)
            //E.Pow(x)
            //E.Pow(SIN(x))
            //x * (_1 / _2) + x * (_1 / _3) + x * 0.5
            //SIN(-PI/(_1/2) + 0.5*PI + 0.5*PI)
            //SQRT(SIN(E.Pow(x) + PI / 2 - 1))
            //_1 / 100000000 + _1 / 100000001
            );

            var base_input = 0;
            var eval_at = base_input + 1;
            int taylor_derivatives = 21;

            Console.WriteLine($"f    = {f}");

            f = f.Reduce();
            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();
            Console.WriteLine($"f(0) = {NumericalEvalWith(f.Definition, (x, base_input))}");

            Console.WriteLine();

            if (!(f.Definition is UndefinedMathExpr))
            {
                //int derivative_number = 1;
                //var derived = DerivativeUtil.Derive(f.Definition, x, derivative_number);
                //Console.WriteLine($"d^{derivative_number} f / dx^{derivative_number}  = {derived}");
                //Console.WriteLine();
                //Console.WriteLine($"derived(0) = {EvalReduce(derived, (x, base_input))}");
                //Console.WriteLine();

                var taylor = TaylorExpansionUtil.Expand(f, taylor_derivatives, base_input);
                Console.WriteLine();
                Console.WriteLine($"taylor  = {taylor}");
                Console.WriteLine();

                var taylor_evaled = NumericalEvalWith(taylor, (x, eval_at));
                var taylor_exact_evaled = ExactEval(taylor_evaled);
                Console.WriteLine($"f({eval_at}) via taylor ~= {taylor_evaled} = {taylor_exact_evaled}");

                var direct_exact_eval = ExactEvalWith(f.Definition, (x, eval_at));
                Console.WriteLine();
                Console.WriteLine($"f({eval_at}) via direct = {direct_exact_eval}");

                var err = Math.Abs(taylor_exact_evaled - direct_exact_eval);
                Console.WriteLine();
                Console.WriteLine($"err = {err}");
            }

            Console.WriteLine();
            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
