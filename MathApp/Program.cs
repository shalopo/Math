using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathUtil;
using MathUtil.Matrices;
using MathUtil.Parsing;
using MathUtil.Tensors;
using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace MathTest
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            //TensorTestIdentity();
            //TensorTest2Ball();
            //TensorTest2Sphere();
            //TensorTest3Ball();
            //TensorTest3Sphere();
            //TensorTestPolar_nd();
            //TaylorTest();
            TestInput();

            Console.WriteLine();
            Console.WriteLine("done.");

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        public static void TestInput()
        {
            while (true)
            {
                Console.WriteLine("Enter expression:");
                string input;

                do
                {
                    input = Console.ReadLine();
                }
                while (string.IsNullOrWhiteSpace(input));

                var (expr, variables) = Parse(input);

                if (expr == null)
                {
                    continue;
                }

                try
                {
                    Console.WriteLine($"Canonical form: {expr} , weight = {GetWeight(expr)}");

                    expr = expr.Reduce(ReduceOptions.DEFAULT);

                    Console.WriteLine($"Reduced form:   {expr} , weight = {GetWeight(expr)}");

                    if (variables.Count() == 1)
                    {
                        var v = variables.First();

                        ExpandTaylor(new ExpandableMathFunctionDef("f", expr, v));
                    }
                    else
                    {
                        if (variables.Any())
                        {
                            Console.WriteLine("Too many variables");
                        }
                        else
                        {
                            var eval = ComplexEval(expr);

                            if (!eval.Equals(expr))
                            {
                                Console.WriteLine(eval);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static (MathExpr, VariableCollection) Parse(string input)
        {
            try
            {
                var variables = new VariableCollection();
                var context = new MathParseContext(variables);

                return (MathParser.Parse(input, context), variables);
            }
            catch (MathParseException ex)
            {
                Console.Write(new string(' ', ex.Offset));
                Console.WriteLine("^");

                Console.WriteLine(ex);
                return (null, null);
            }
        }

        public static void TensorTestIdentity()
        {
            MathVariable x = new("x");
            MathVariable y = new("y");
            MathVariable z = new("z");
            MathVariable t = new("t");

            var tensor = MetricTensor.CreateDefault(new[] { t, x, y, z });
            tensor[t, t] = MINUS_ONE;
            Console.WriteLine(tensor);

            var transformation = new VariablesChangeTransformation(new[] { t, x, y, z }, (t, t), (x, x), (y, y), (z, z));

            var triviallyTransformedTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(triviallyTransformedTensor);
            Console.WriteLine();
        }

        public static void TensorTest2Ball()
        {
            MathVariable x = new("x");
            MathVariable y = new("y");
            MathVariable z = new("z");
            MathVariable r = new("r");
            MathVariable theta = new("θ");

            var tensor = MetricTensor.CreateDefault(new[] { x, y });
            Console.WriteLine(tensor);

            var transformation = new VariablesChangeTransformation(new[] { r, theta }, (x, r * COS(theta)), (y, r * SIN(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();

            Console.WriteLine("Christoffels:");
            Console.WriteLine(ChristoffelSymbols.Create(polarTensor));

            var inverseTransformation = new VariablesChangeTransformation(new[] { x, y }, (r, (x * x + y * y).Pow(HALF)),
                (theta, ARCTAN(y / x)));
            var originalTensor = polarTensor.ChangeCoordinates(inverseTransformation);

            Console.WriteLine(originalTensor);
            Console.WriteLine();
        }

        public static void TensorTest2Sphere()
        {
            // parameter (constant)
            var r = new MathVariable("r");

            var theta = new MathVariable("θ");
            var phi = new MathVariable("φ");

            var metric = MetricTensor.CreateDefault(new[] { theta, phi });
            metric[theta, theta] = r.Pow(2);
            metric[phi, phi] = r.Pow(2) * SIN(theta).Pow(2);

            Console.WriteLine(metric);
            Console.WriteLine();

            Console.WriteLine("Christoffels:");
            Console.WriteLine(ChristoffelSymbols.Create(metric));

            Console.WriteLine("ricci:");
            var ricci = RicciTensor.Create(metric);
            Console.WriteLine(ricci);

            Console.WriteLine($"ricci scalar: {ricci.Scalar()}");
        }

        public static void TensorTest3Ball()
        {
            MathVariable x = new("x");
            MathVariable y = new("y");
            MathVariable z = new("z");
            MathVariable r = new("r");
            MathVariable theta = new("θ");
            MathVariable phi = new("φ");

            var tensor = MetricTensor.CreateDefault(new[] { x, y, z });
            Console.WriteLine(tensor);

            var transformation = new VariablesChangeTransformation(new[] { r, theta, phi }, 
                (x, r * COS(phi) * SIN(theta)), (y, r * SIN(phi) * SIN(theta)), (z, r * COS(theta)));
            var polarTensor = tensor.ChangeCoordinates(transformation);

            Console.WriteLine(polarTensor.ToString());
            Console.WriteLine();

            Console.WriteLine("Christoffels:");
            Console.WriteLine(ChristoffelSymbols.Create(polarTensor));
        }

        public static void TensorTest3Sphere()
        {
            // parameter (constant)
            MathVariable r = new("r");
            MathVariable theta = new("θ");
            MathVariable phi = new("φ");
            MathVariable lambda = new("λ");

            var metric = MetricTensor.CreateDefault(new[] { theta, phi, lambda });
            metric[theta, theta] = r.Pow(2);
            metric[phi, phi] = r.Pow(2) * SIN(theta).Pow(2);
            metric[lambda, lambda] = r.Pow(2) * SIN(theta).Pow(2) * SIN(phi).Pow(2);

            Console.WriteLine(metric);
            Console.WriteLine();

            Console.WriteLine("Christoffels:");
            Console.WriteLine(ChristoffelSymbols.Create(metric));

            Console.WriteLine("ricci:");
            var ricci = RicciTensor.Create(metric);
            Console.WriteLine(ricci);

            Console.WriteLine($"ricci scalar: {ricci.Scalar()}");
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
             
            var tensor = MetricTensor.CreateDefault(variables);
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
            var x = ExpandableMathFunctionDef.x1;

            ExpandTaylor(new ExpandableMathFunctionDef("f",
            (x + 1)*(x - 1)*(x + 3)*(x + 5) / (x - 2)  //TODO: errors and weird reuctions
            //(SQRT(2) / 2 + I * SQRT(2) / 2).Pow(2)   //TODO: Weird printing on complex eval
            , x));
        }

        private static void ExpandTaylor(ExpandableMathFunctionDef f, MathExpr eval_at = null, MathExpr base_input = null, 
            int max_derivatives = 15, int max_seconds = 2)
        {
            base_input ??= ZERO;
            eval_at ??= ONE;

            var arg = f.Arg;

            f = f.Reduce(ReduceOptions.DEFAULT.With(allowSearchIdentities: false));

            Console.WriteLine();
            Console.WriteLine($"{f.Signature} = {f}");
            Console.WriteLine();

            var taylor = TaylorExpansionUtil.Expand(f, max_derivatives, arg, base_input, max_seconds);
            Console.WriteLine();
            Console.WriteLine($"{f.Signature} ~= {taylor}");
            Console.WriteLine();

            var taylor_evaled = NumericalEvalWith(taylor, (arg, eval_at));
            var taylor_exact_evaled = ComplexEval(taylor_evaled);
            Console.WriteLine($"{f.Name}({eval_at}) via taylor series ~= {taylor_exact_evaled}");

            var direct_exact_eval = ComplexEvalWith(f.Definition, (arg, eval_at));
            Console.WriteLine($"{f.Name}({eval_at}) via direct = {direct_exact_eval}");

            var error = ComplexEval(taylor_exact_evaled - direct_exact_eval).Size;

            if (direct_exact_eval.Size == 0)
            {
                Console.WriteLine($"error = {error}");
            }
            else
            {
                var errorRatio = error / direct_exact_eval.Size;
                Console.WriteLine($"error ratio = {errorRatio}");
            }
        }
    }
}
