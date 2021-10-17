using MathUtil;
using MathUtil.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class ReductionTest
    {
        private static void AssertReducedEqual(string expected_str, string actual_str)
        {
            var reduceOptions = ReduceOptions.DEFAULT;

            MathParseContext context = new();

            var expected = MathParser.Parse(expected_str, context).Reduce(reduceOptions);
            var actual = MathParser.Parse(actual_str, context).Reduce(reduceOptions);

            if (!expected.Equals(actual))
            {
                if (!(expected - actual).Reduce(ReduceOptions.DEFAULT).Equals(GlobalMathDefs.ZERO))
                {
                    Assert.True(false, $"Expected: {expected}, actual: {actual}");
                }
            }
        }

        [Fact]
        public void TestNumbers()
        {
            AssertReducedEqual("6", "2+3-4+5");
            AssertReducedEqual("30", "5*3/2*4");
            AssertReducedEqual("256", "2^2^3");
            AssertReducedEqual("2/3", "4/6");
            AssertReducedEqual("1/2", "2/4");
            AssertReducedEqual("5/6", "1/2+1/3");
            AssertReducedEqual("-1", "1/2-3/2");
            AssertReducedEqual("1", "(1/(-2))-((-3)/2)");
        }

        [Fact]
        public void TestCompound()
        {
            AssertReducedEqual("xy", "yx");
            AssertReducedEqual("x+y", "y+x");
            AssertReducedEqual("x+xy+zx", "xz+x+xy");
            AssertReducedEqual("2+x+y+z", "2+(x+(y+z))");
            AssertReducedEqual("2xyz/w/t", "((2x)*(yz))/(wt)");
        }

        [Fact]
        public void TestCollectAdd()
        {
            AssertReducedEqual("x+2y+3z", "3z+2x+y+y-x+5t-3t-2t");
            AssertReducedEqual("2(x+1)^2", "3(x+1)^2 - (1+x)^2");
            AssertReducedEqual("4sin(x)", "2sin(x) + 3sin(x) - sin(x)");
        }

        [Fact]
        public void TestCollectMult()
        {
            AssertReducedEqual("x*y^2*z^3", "z^3*x^2*y*y/x*t^5/t^3*t^(-2)");
            AssertReducedEqual("2(x+1)^2", "3(x+1)^2 - (1+x)^2");
            AssertReducedEqual("ln(x)^4*cos(x)^2", "cos(x) * ln(x)^2 * ln(x)^3 / ln(x) * cos x");
        }

        [Fact]
        public void TestDistribution()
        {
            AssertReducedEqual("x+2", "2(x+1)-x");
            AssertReducedEqual("2x", "2(x+1)-2");
            AssertReducedEqual("x", "x(x + 1) - x^2");
        }

        [Fact]
        public void TestMinusDistribution()
        {
            AssertReducedEqual("x/y", "(-x)/(-y)");
            AssertReducedEqual("-x/y", "x/(-y)");
            AssertReducedEqual("-x/(2y)", "x/(-2y)");
        }

        [Fact]
        public void TestPowers()
        {
            AssertReducedEqual("1", "1234987132234^0");
            AssertReducedEqual("0", "0^1234987132234");

            AssertReducedEqual("i^2", "-1");
            AssertReducedEqual("-i", "1/i");
            //TODO
        }

        [Fact]
        public void TestTrigKnownValues()
        {
            AssertReducedEqual("2", "2sin(pi/4) sqrt(2)");
            //TODO
        }

        [Fact]
        public void TestFractions()
        {
            AssertReducedEqual("1", "x/(x + y) + y/(x + y)");
            AssertReducedEqual("y/x", "1/(x/y)");
            AssertReducedEqual("2/x", "1/(x - x/2)");
            AssertReducedEqual("2", "2x/(x + y) + 2y/(x + y)");
        }

        [Fact]
        public void TestDistributePowers()
        {
            AssertReducedEqual("0", "(sqrt(x)/x)^(-2) - sqr(x)/x");
            AssertReducedEqual("x^2+y^2", "y^2*(1 + x^2/y^2)");
            AssertReducedEqual("x^2+y^2", "y^2*(1 + (x/y)^2)");
            AssertReducedEqual("(x^2+y^2)^2/y^2", "(1 + x^2/y^2)/(x^2 + y^2)");
        }

        [Fact]
        public void TestTrigIdentities()
        {
            AssertReducedEqual("x^2", "x^2*sin(y)^2 + x^2*cos(y)^2");
            AssertReducedEqual("x^2", "x^2sin(y)^2sin(z)^2 + x^2sin(y)^2cos(z)^2 + x^2cos(y)^2");
            AssertReducedEqual("2cos(x) + 4", "3cos(x/2)^2 - sin(x/2)^2 + 2sin(2y)^2 + 2cos(2y)^2 + sin(z)^3/sin(z) + cos(z)cos(z)");

            AssertReducedEqual("sin(x)cos(x)", "sin(2x) - sin(x)cos(x)");
            
            AssertReducedEqual("1/cos(x)^2", "1 + sin(x)^2/cos(x)^2");

            //TODO: fix the identity searching to not rely on adding
            AssertReducedEqual("sin(x)", "tan(x)cos(x)");
            AssertReducedEqual("cost(x)", "cot(x)sin(x)");
            AssertReducedEqual("1", "tan(x)cot(x)");
            AssertReducedEqual("0", "1/cot(x) - sin(x)/cos(x)");
        }

    }
}
