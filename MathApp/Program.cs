using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathUtil;
using MathUtil.Tensors;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathTest
{
    class Program
    {
        static void TestReduceIdentities(MathExpr expr)
        {
            Console.WriteLine(MathIdentitiesManager.Reduce(expr.Reduce()));
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var x = new MathVariable("x");
            var y = new MathVariable("y");

            //TestReduceIdentities(SIN(x).Pow(2) + COS(x).Pow(2));
            TestReduceIdentities(4 * COS(3 * x / 2).Pow(2) - 4 * SIN(3 * x / 2).Pow(2) + 2 * SIN(y).Pow(2) + 2 * COS(y).Pow(2));
            //Console.TestReduceIdentities(2 * PI * SIN(x).Pow(2) + 2 * PI * COS(x).Pow(2));

            //TensorTestIdentity();
            //TensorTestPolar2d();
            TensorTestPolar3d();
            //TaylorTest();

            Console.WriteLine();
            Console.WriteLine("done.");
            Console.ReadLine();
        }

        public static void TensorTestIdentity()
        {
            var t = new MathVariable("t");
            var x = new MathVariable("x");
            var y = new MathVariable("y");
            var z = new MathVariable("z");

            var tensor = new MetricTensor(new MathVariable[] { t, x, y, z });
            tensor[t, t] = MINUS_ONE;
            tensor[x, x] = ONE;
            tensor[y, y] = ONE;
            tensor[z, z] = ONE;
            Console.WriteLine(tensor);

            var transformation = new VariablesChangeTransformation(new MathVariable[] { t, x, y, z }, (t, t), (x, x), (y, y), (z, z));

            var triviallyTransformedTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(triviallyTransformedTensor);
            Console.WriteLine();
        }

        public static void TensorTestPolar2d()
        {
            var x = new MathVariable("x");
            var y = new MathVariable("y");

            var tensor = MetricTensor.Identity(new MathVariable[] { x, y });
            Console.WriteLine(tensor);

            var r = new MathVariable("r");
            var theta = new MathVariable("θ");

            var transformation = new VariablesChangeTransformation(new MathVariable[] { r, theta }, (x, r * COS(theta)), (y, r * SIN(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();
        }

        public static void TensorTestPolar3d()
        {
            var x = new MathVariable("x");
            var y = new MathVariable("y");
            var z = new MathVariable("z");

            var tensor = MetricTensor.Identity(new MathVariable[] { x, y, z });
            Console.WriteLine(tensor);

            var r = new MathVariable("r");
            var theta = new MathVariable("θ");
            var phi = new MathVariable("φ");

            var transformation = new VariablesChangeTransformation(new MathVariable[] { r, theta, phi }, 
                (x, r * COS(phi) * SIN(theta)), (y, r * SIN(phi) * SIN(theta)), (z, r * COS(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();
        }

        public void TaylorTest()
        {
            var x = new MathVariable("x");

            var f = new ExpandableMathFunctionDef("f",
            //SIN(-x + 1).Pow(2) * SIN(x + 1)
            //4 * ARCTAN(-x)
            //E.Pow(x)
            //1/(1-I)
            //1/(1-x/4)
            //(-1+2*I).Pow(3-5*I)
            //COS(PI / 2)
            //(2 * (x - 3) + 6) / (x + 1)
            //(SQRT(2) / 2 + I * SQRT(2) / 2).Pow(2)
            //E.Pow(I * x) / (COS(x) + I * SIN(x))
            //SIN(2 * x) / 2 * SIN(x) * COS(x)
            (27 + x).Pow(ONE / 3)
            );

            var base_input = 1;
            var eval_at = 2;
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

            var taylor = TaylorExpansionUtil.Expand(f, taylor_derivatives, x, base_input);
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
        }
    }
}
