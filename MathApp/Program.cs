using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathUtil;
using static MathUtil.GlobalMathDefs;
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
            Console.WriteLine(Math.Atan2(0,0));
            var f = new ExpandableMathFunctionDef("f",
            //2 * ARCSIN(x)
            //LN(x + 1)
            //E.Pow(x)
            //1/(1-x/4)
            //(-1+2*I).Pow(3-5*I)
            //COS(PI / 2)
            //(SQRT(2) / 2 + I * SQRT(2) / 2).Pow(2)
            //E.Pow(I * x) / (COS(x) + I * SIN(x))
            //SIN(2*x) / 2*SIN(x)*COS(x)
            LN(x + 1)
            );

            var base_input = 0;
            var eval_at = 1;
            int taylor_derivatives = 10;

            Console.WriteLine($"f    = {f}");

            f = f.Reduce();
            Console.WriteLine($"f*  = {f}");
            Console.WriteLine();

            var numerical_eval = NumericalEvalWith(f.Definition, (x, base_input));
            var complex_eval = ComplexEvalWith(f.Definition, (x, base_input));
            Console.WriteLine($"f(0) = {numerical_eval} = {complex_eval}");

            Console.WriteLine();

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
            var taylor_exact_evaled = ComplexEval(taylor_evaled);
            Console.WriteLine($"f({eval_at}) via taylor ~= {taylor_evaled} = {taylor_exact_evaled}");

            var direct_exact_eval = ComplexEvalWith(f.Definition, (x, eval_at));
            Console.WriteLine();
            Console.WriteLine($"f({eval_at}) via direct = {direct_exact_eval}");

            var err = ComplexEval(taylor_exact_evaled - direct_exact_eval).Size;
            Console.WriteLine();
            Console.WriteLine($"err = {err}");

            Console.WriteLine();
            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
