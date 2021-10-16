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
        static readonly MathVariable x = new MathVariable("x");
        static readonly MathVariable y = new MathVariable("y");
        static readonly MathVariable z = new MathVariable("z");

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            TestReductions();
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
                    expr = expr.Reduce(ReduceOptions.DEFAULT);

                    if (variables.Count() == 1)
                    {
                        var v = variables.First();

                        ExpandTaylor(new ExpandableMathFunctionDef("f", expr, v));
                    }
                    else
                    {
                        Console.WriteLine(expr);

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

        public static void TestReductions()
        {
            TestReduction("1/i");

            TestReduction("1 + 2(x + 1)");

            TestReduction("(-x)/(-y)");
            TestReduction("x/(-y)");

            TestReduction("x/(-2y)");
            TestReduction("1/(x/y)");

            TestReduction("x(x + 1) - x^2");

            TestReduction("y^2*(1 + x^2/y^2)");
            TestReduction("(1 + x^2/y^2)/(x^2 + y^2)");
            TestReduction("y^2*(1 + (x/y)^2)");

            TestReduction("x^2*sin(y)^2 + x^2*cos(y)^2");
            TestReduction("x^2sin(y)^2sin(z)^2 + x^2sin(y)^2cos(z)^2 + x^2cos(y)^2");

            TestReduction("3cos(x/2)^2 - sin(x/2)^2 + " +
                          "2sin(2y)^2 + 2cos(2y)^2 + " +
                          "sin(z)^3/sin(z) + cos(z)cos(z)");

            TestReduction("0^198713");

            TestReduction("sin(pi/4)sqrt(2)*2");
            TestReduction("(sqrt(x)/x)^(-2) - sqr(x)/x");
            TestReduction("1/cot(x) - sin(x)/cos(x)"); //TODO: should be zero

            //TODO: fix the identity searching to not rely on adding
            TestReduction("tan(x)cos(x)");
            TestReduction("tan(x)cot(x)");

            //TODO: does not reduce
            TestReduction("sin(2x) - sin(x)cos(x)");

            TestReduction("1/(x - x/2)");

            //TODO: prints funny
            TestReduction("6/x");

            TestReduction("x/(x + y) + y/(x + y)");

            //TODO: does not reduce
            TestReduction("2x/(x + y) + 2y/(x + y)");

            //TODO: needs to reduce to 1/cos(x)^2
            TestReduction("1 + sin(x)^2/cos(x)^2");
        }

        public static void TestReduction(MathExpr expr)
        {
            Console.WriteLine(expr.ToString());
            Console.WriteLine("=>");
            Console.WriteLine(expr.Reduce(ReduceOptions.DEFAULT));
            Console.WriteLine();
        }

        public static void TensorTestIdentity()
        {
            var t = new MathVariable("t");

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
            var tensor = MetricTensor.CreateDefault(new[] { x, y });
            Console.WriteLine(tensor);

            var r = new MathVariable("r");
            var theta = new MathVariable("θ");

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

            //var inverseTransformation = new VariablesChangeTransformation(new[] { x, y }, (r, (x * x + y * y).Pow(HALF)), 
            //    (theta, ARCTAN(y / x)));
            //var originalTensor = polarTensor.ChangeCoordinates(inverseTransformation);

            //Console.WriteLine(originalTensor);
            //Console.WriteLine();

            Console.WriteLine("ricci:");
            var ricci = RicciTensor.Create(metric);
            Console.WriteLine(ricci);

            Console.WriteLine($"ricci scalar: {ricci.Scalar()}");
        }

        public static void TensorTest3Ball()
        {
            var tensor = MetricTensor.CreateDefault(new MathVariable[] { x, y, z });
            Console.WriteLine(tensor);

            var r = new MathVariable("r");
            var theta = new MathVariable("θ");
            var phi = new MathVariable("φ");

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
            var r = new MathVariable("r");

            var theta = new MathVariable("θ");
            var phi = new MathVariable("φ");
            var lambda = new MathVariable("λ");

            var metric = MetricTensor.CreateDefault(new MathVariable[] { theta, phi, lambda });
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
            ExpandTaylor(new ExpandableMathFunctionDef("f", 
            //SIN(-x + 1).Pow(2) * SIN(x + 1)
            //4 * ARCTAN(-x)
            //E.Pow(x)
            //E.Pow(2 * x * I)    
            //SIN(x).Pow(2)
            E.Pow(7 * x) //TODO: some serious error here
            //1/(1-I)
            //1/(1-x/4)
            //(-1+2*I).Pow(3-5*I)
            //COS(PI / 2)
            //(2 * (x - 3) + 6) / (x + 1)
            //(SQRT(2) / 2 + I * SQRT(2) / 2).Pow(2)
            //E.Pow(I * x) / (COS(x) + I * SIN(x))
            //SIN(2 * x) / 2 * SIN(x) * COS(x)
            //(27 + x).Pow(ONE / 3)
            , x));
        }

        private static void ExpandTaylor(ExpandableMathFunctionDef f, MathExpr eval_at = null, MathExpr base_input = null, 
            int max_derivatives = 20, int max_seconds = 2)
        {
            base_input ??= ZERO;
            eval_at ??= ONE;

            var arg = f.Arg;

            f = f.Reduce(ReduceOptions.DEFAULT.With(allowSearchIdentities: false));

            Console.WriteLine();
            Console.WriteLine($"{f.Signature} = {f}");
            Console.WriteLine();

            var taylor = TaylorExpansionUtil.Expand(f, max_derivatives, arg, base_input, max_seconds);
            Console.WriteLine($"{f.Signature} ~= {taylor}");
            Console.WriteLine();

            var taylor_evaled = NumericalEvalWith(taylor, (arg, eval_at));
            var taylor_exact_evaled = ComplexEval(taylor_evaled);
            Console.WriteLine($"{f.Name}({eval_at}) via taylor series ~= {taylor_exact_evaled}");

            var direct_exact_eval = ComplexEvalWith(f.Definition, (arg, eval_at));
            Console.WriteLine($"{f.Name}({eval_at}) via direct = {direct_exact_eval}");

            var err = ComplexEval(taylor_exact_evaled - direct_exact_eval).Size;
            Console.WriteLine($"err = {err}");
        }
    }
}
