using MathUtil;
using MathUtil.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using static MathUtil.GlobalMathDefs;
using static MathUtil.MathEvalUtil;

namespace Test
{
    public class ParsingTest
    {
        static readonly MathVariable x = new("x");
        static readonly MathVariable y = new("y");
        static readonly MathVariable z = new("z");

        static void AssertParse(MathExpr expected_expr, string input, VariableCollection variables = null)
        {
            variables = (variables ?? new()).AsReadOnly();
            MathParseContext context = new(variables);
            var parsed_expr = MathParser.Parse(input, context);

            Assert.Equal(expected_expr, parsed_expr, EqualityComparer<MathExpr>.Default);
        }

        static void AssertParseError(string input)
        {
            Assert.Throws<MathParseException>(() => MathParser.Parse(input));
        }

        [Fact]
        public void TestZero()
        {
            AssertParse(ZERO, "0");
            AssertParse(0, "0");
            Assert.True(IsZero(ZERO));
            Assert.True(IsZero(ConstComplexMathExpr.Create(0, 0)));

            Assert.False(IsZero(ONE));
            Assert.False(IsZero(ConstComplexMathExpr.Create(1, 0)));
            Assert.False(IsZero(ConstComplexMathExpr.Create(0, 1)));
        }

        [Fact]
        public void TestOne()
        {
            AssertParse(ONE, "1");
            AssertParse(1, "1");
            Assert.True(IsOne(ONE));
            Assert.True(IsOne(ConstComplexMathExpr.Create(1, 0)));

            Assert.False(IsOne(ZERO));
            Assert.False(IsOne(ConstComplexMathExpr.Create(0, 0)));
            Assert.False(IsOne(ConstComplexMathExpr.Create(1, 1)));
        }

        [Fact]
        public void TestVar()
        {
            AssertParse(x, "x", new() { x });
            AssertParse(x + y + z, "x + y + z", new() { x, y, z });

            {
                VariableCollection v = new() { x };
                var parsed_Expr = MathParser.Parse("x + y + z", new MathParseContext(v));
                
                Assert.True(v.Contains("y"));
                Assert.True(v.Contains("z"));
                Assert.Equal(3, v.Count());

                Assert.Equal(x + v["y"] + v["z"], parsed_Expr);
                Assert.NotEqual(x + y + z, parsed_Expr); // different y and z
            }
        }

        [Fact]
        public void TestNumbers()
        {
            AssertParse(2, "2");
            AssertParse(20, "20");
            AssertParse(20, "020");
            AssertParse(-2, "-2");
            AssertParse(1.23456789, "1.23456789");
            AssertParse(-98765432.1, "-98765432.1");
        }

        [Fact]
        public void TestConsts()
        {
            AssertParse(E, "e");
            AssertParse(E, "E");

            AssertParse(I, "i");
            AssertParse(I, "I");

            AssertParse(PI, "π");
            AssertParse(PI, "pi");
            AssertParse(PI, "PI");
        }

        [Fact]
        public void TestShorthandMultiplication()
        {
            AssertParse(2 * x, "2x", new() { x });
            AssertParse(2 * x, "2(x)", new() { x });
            AssertParse(2 * x * (x + 1), "2x(x+1)", new() { x });
            AssertParse(2 * x * y + x * y * z, "2xy + xyz", new() { x, y, z });
        }

        [Fact]
        public void TestConsecutiveMinusSigns()
        {
            AssertParse(1, "-(-1)");
            AssertParse(-1, "-(-(-1))");
            AssertParse(1 - x, "1 + -x", new() { x });
        }

        [Theory]
        [InlineData("")]
        [InlineData("()")]
        [InlineData("1-+2")]
        [InlineData("1**2")]
        [InlineData("1*/2")]
        [InlineData("--2")]
        [InlineData("1 + --x")]
        [InlineData("2(")]
        [InlineData("2(x + 1")]
        [InlineData("2(x + 1) - 3((x + 1) / x + 2")]
        [InlineData(")")]
        [InlineData("2)")]
        [InlineData("2(x + 1))")]
        [InlineData("2((x + 1)/3) + 1) + 2")]
        public void TestErrors(string input)
        {
            AssertParseError(input);
        }

        [Theory]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("*")]
        [InlineData("/")]
        [InlineData("^")]
        public void TestBinaryOpError(string op)
        {
            AssertParseError(op);
            AssertParseError($"2{op}");
            AssertParseError($"x{op}");

            if (op != "-")
            {
                AssertParseError($"2{op}{op}3");
                AssertParseError($"x{op}{op}y");

                AssertParseError($"{op}2");
                AssertParseError($"{op}x");
            }
        }

        [Theory]
        [InlineData(typeof(SinFunctionDef))]
        [InlineData(typeof(CosFunctionDef))]
        [InlineData(typeof(TanFunctionDef))]
        [InlineData(typeof(CotFunctionDef))]
        [InlineData(typeof(ArcTanFunctionDef))]
        [InlineData(typeof(ArcTan2FunctionDef))]
        [InlineData(typeof(ArcSinFunctionDef))]
        [InlineData(typeof(ArcCosFunctionDef))]
        [InlineData(typeof(LnFunctionDef))]
        [InlineData(typeof(SqrtFunctionDef))]
        [InlineData(typeof(SqrFunctionDef))]
        public void TestFunction(Type funcType)
        {
            var func = (MathFunctionDef)funcType.GetConstructor(Type.EmptyTypes).Invoke(null);

            var name = func.Name;

            AssertParse(func.Call(x), $"{name}(x)", new() { x });
            AssertParse(func.Call(x), $"{name}x", new() { x });
            AssertParse(func.Call(x), $"{name} x", new() { x });

            AssertParse(func.Call(x.Pow(2)), $"{name}(x^2)", new() { x });
            AssertParse(func.Call(x).Pow(2), $"{name} x^2", new() { x });

            AssertParse(func.Call(x.Pow(2)) * func.Call(y) * func.Call(z) * func.Call(x) * func.Call(2 * y), 
                $"{name}(x^2){name}y*{name}z {name} x * {name} (2y)", new() { x, y, z });
        }


        [Fact]
        public void TestBasicParseFlow()
        {
            VariableCollection v = new() { x, y, z };
            var expected_expr = x.Pow((x + y - z - 3) * z / 2.5);

            AssertParse(expected_expr, "x^((x+y-z-3)*z/2.5)", v);
            AssertParse(expected_expr, "x ^ (  (x + y - z - 3 ) *  z / 2.5)   ", v);
        }

        [Fact]
        public void TestOrderOfOperations()
        {
            AssertParse(x + 3 * y, "x+3 * y", new() { x, y });
            AssertParse(x + (3 / z) - (4 * z) + 5, "x + 3 / z - 4*z + 5", new() { x, y, z });
            AssertParse((x / y) / z, "x/y/z", new() { x, y, z });
            AssertParse(((1 / x) / y) / z, "1/x/y/z", new() { x, y, z });
            AssertParse((x / y) * z, "x/y*z", new() { x, y, z });
            AssertParse(x / (y * z), "x/(y*z)", new() { x, y, z });

            AssertParse(x.Pow(x.Pow(2)), "x^x^2", new() { x });
            AssertParse(2 * x.Pow(2), "2x^2", new() { x });
            AssertParse(-(TWO.Pow(x)), "-2^x", new() { x });

            AssertParse(2 / x.Pow(2), "2 / x^2", new() { x });
            AssertParse((2 / y) * z * x.Pow(2), "2 / y * z * x^2", new() { x, y, z });
            AssertParse((x / y.Pow(z)) * z.Pow(x), "x / y^z * z^x", new() { x, y, z });
        }

    }
}
