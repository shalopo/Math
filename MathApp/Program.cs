using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathUtil;
using MathUtil.Matrices;
using MathUtil.Tensors;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathTest
{
    class Program
    {
        public static void TestReduction(MathExpr expr)
        {
            Console.WriteLine(expr.ToString());
            Console.WriteLine("=>");
            Console.WriteLine(expr.Reduce(ReduceOptions.DEFAULT));
            Console.WriteLine();
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var x = new MathVariable("x");
            var y = new MathVariable("y");
            var z = new MathVariable("z");

            //TestReduction(2 * (x + 1) - x);

            //TestReduction(y.Pow(2) * (1 + x.Pow(2) / y.Pow(2)));
            //TestReduction((1 + x.Pow(2) / y.Pow(2)) / (x.Pow(2) + y.Pow(2)));
            //TestReduction(y.Pow(2) * (1 + (x / y).Pow(2)));

            //TestReduction(x.Pow(2) * SIN(y).Pow(2) + x.Pow(2) * COS(y).Pow(2));
            //TestReduction(x.Pow(2) * SIN(y).Pow(2) * SIN(z).Pow(2) + x.Pow(2) * SIN(y).Pow(2) * COS(z).Pow(2) + x.Pow(2) * COS(y).Pow(2));

            //TestReduction(3 * COS(x / 2).Pow(2) - SIN(x / 2).Pow(2) + 
            //              2 * SIN(2 * y).Pow(2) + 2 * COS(2 * y).Pow(2) +
            //              SIN(z).Pow(3) / SIN(z) + COS(z) * COS(z));

            //TensorTestIdentity();
            //TensorTestPolar2d();
            //TensorTestPolar3d();
            //TensorTestPolar_nd();
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

            var tensor = new MetricTensor(new[] { t, x, y, z });
            tensor[t, t] = MINUS_ONE;
            tensor[x, x] = ONE;
            tensor[y, y] = ONE;
            tensor[z, z] = ONE;
            Console.WriteLine(tensor);

            var transformation = new VariablesChangeTransformation(new[] { t, x, y, z }, (t, t), (x, x), (y, y), (z, z));

            var triviallyTransformedTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(triviallyTransformedTensor);
            Console.WriteLine();
        }

        public static void TensorTestPolar2d()
        {
            var x = new MathVariable("x");
            var y = new MathVariable("y");

            var tensor = MetricTensor.Identity(new[] { x, y });
            Console.WriteLine(tensor);

            var r = new MathVariable("r");
            var theta = new MathVariable("θ");

            var transformation = new VariablesChangeTransformation(new[] { r, theta }, (x, r * COS(theta)), (y, r * SIN(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();

            var inverseTransformation = new VariablesChangeTransformation(new[] { x, y }, (r, (x * x + y * y).Pow(HALF)), 
                (theta, ARCTAN(y / x)));
            var originalTensor = polarTensor.ChangeCoordinates(inverseTransformation);

            Console.WriteLine(originalTensor);
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

            var transformation = new VariablesChangeTransformation(new[] { r, theta, phi }, 
                (x, r * COS(phi) * SIN(theta)), (y, r * SIN(phi) * SIN(theta)), (z, r * COS(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();
        }

        public static void TensorTestPolar_nd()
        {
            int n = 4;
            var variables = new MathVariable[n];
            var primedVariables = new MathVariable[n];

            for (int i = 0; i < n; i++)
            {
                variables[i] = new MathVariable(((char)('p' + i)).ToString());
            }

            var r = new MathVariable("r");
            primedVariables[0] = r;

            for (int i = 1; i < n; i++)
            {
                primedVariables[i] = new MathVariable($"φ{i}");
            }
             
            var tensor = MetricTensor.Identity(variables);
            Console.WriteLine(tensor);

            var mappings = new List<(MathVariable v, MathExpr transformed)>();

            MathExpr sinMultR = r;

            for (int i = 0; i < n - 1; i++)
            {
                mappings.Add((variables[i], sinMultR * COS(primedVariables[i + 1])));

                sinMultR *= SIN(primedVariables[i + 1]);
            }

            mappings.Add((variables[n - 1], sinMultR));

            var transformation = new VariablesChangeTransformation(primedVariables, mappings.ToArray());

            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();
        }

        public static void TaylorTest()
        {
            var x = new MathVariable("x");

            var f = new ExpandableMathFunctionDef("f",
            //SIN(-x + 1).Pow(2) * SIN(x + 1)
            //4 * ARCTAN(-x)
            //E.Pow(x)
            E.Pow(2 * x * I)
            //SIN(x).Pow(2)
            //1/(1-I)
            //1/(1-x/4)
            //(-1+2*I).Pow(3-5*I)
            //COS(PI / 2)
            //(2 * (x - 3) + 6) / (x + 1)
            //(SQRT(2) / 2 + I * SQRT(2) / 2).Pow(2)
            //E.Pow(I * x) / (COS(x) + I * SIN(x))
            //SIN(2 * x) / 2 * SIN(x) * COS(x)
            //(27 + x).Pow(ONE / 3)
            );

            var base_input = 0;
            var eval_at = PI / 8;
            int taylor_derivatives = 20;

            f = f.Reduce(ReduceOptions.DEFAULT.With(allowSearchIdentities: false));

            Console.WriteLine();
            Console.WriteLine($"f(x) = {f}");
            Console.WriteLine();

            var complex_eval = ComplexEvalWith(f.Definition, (x, base_input));
            Console.WriteLine($"f(0) = {complex_eval}");

            Console.WriteLine();

            //int derivative_number = 1;
            //var derived = DerivativeUtil.Derive(f.Definition, x, derivative_number);
            //Console.WriteLine($"d^{derivative_number} f / dx^{derivative_number}  = {derived}");
            //Console.WriteLine();
            //Console.WriteLine($"derived(0) = {EvalReduce(derived, (x, base_input))}");
            //Console.WriteLine();

            var taylor = TaylorExpansionUtil.Expand(f, taylor_derivatives, x, base_input);
            Console.WriteLine();
            Console.WriteLine($"taylor series of f(x) around {base_input} = {taylor}");
            Console.WriteLine();

            var taylor_evaled = NumericalEvalWith(taylor, (x, eval_at));
            var taylor_exact_evaled = ComplexEval(taylor_evaled);
            Console.WriteLine($"f({eval_at}) via taylor series ~= {taylor_evaled} = {taylor_exact_evaled}");

            var direct_exact_eval = ComplexEvalWith(f.Definition, (x, eval_at));
            Console.WriteLine();
            Console.WriteLine($"f({eval_at}) via direct = {direct_exact_eval}");

            var err = ComplexEval(taylor_exact_evaled - direct_exact_eval).Size;
            Console.WriteLine();
            Console.WriteLine($"err = {err}");
        }
    }
}
