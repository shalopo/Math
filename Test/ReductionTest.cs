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
        [Theory]
        [MemberData(nameof(NumbersTestData))]
        [MemberData(nameof(CompoundTestData))]
        [MemberData(nameof(CollectAddTestData))]
        [MemberData(nameof(CollectMultTestData))]
        [MemberData(nameof(DistributionAddTestData))]
        [MemberData(nameof(MinusDistributionTestData))]
        [MemberData(nameof(PowersTestData))]
        [MemberData(nameof(TrigKnownValuesTestData))]
        [MemberData(nameof(FractionsTestData))]
        [MemberData(nameof(DistributeMultTestData))]
        [MemberData(nameof(TrigIdentitiesTestData))]
        [MemberData(nameof(RepetitiveReductionTest))]
        public void AssertReducedEqual(string expected_str, string actual_str)
        {
            MathParseContext context = new();

            var actual = MathEvalUtil.Reduce(MathParser.Parse(actual_str, context));
            var expected = MathEvalUtil.Reduce(MathParser.Parse(expected_str, context));

            TestUtils.AssertEqual(expected, actual);
        }

        private static IEnumerable<object[]> GetTheoryData((string, string)[] testData) =>
            testData.Select(input => new object[] { input.Item1, input.Item2});

        public static IEnumerable<object[]> NumbersTestData() => GetTheoryData(new[] {
            ("0", "0"),
            ("1", "1"),
            ("-1", "-1"),
            ("2", "2"),
            ("-2", "-2"),
            ("6", "2+3-4+5"),
            ("30", "5*3/2*4"),
            ("256", "2^2^3"),
            ("-24", "-3*8"),
            ("-24", "-3*8"),
            ("-24", "3*(-8)"),
            ("24", "(-3)*(-8)"),
            ("1", "0.1/0.1"),
            ("1", "(-0.1)/(-0.1)"),
            ("-1", "-(0.1)^2000000/(-0.1)^2000000"),
        });

        public static IEnumerable<object[]> CompoundTestData() => GetTheoryData(new[] {
            ("xy", "yx"),
            ("x+y", "y+x"),
            ("x+xy+zx", "xz+x+xy"),
            ("2+x+y+z", "2+(x+(y+z))"),
            ("2xyz/w/t", "((2x)*(yz))/(wt)"),
        });

        public static IEnumerable<object[]> CollectAddTestData() => GetTheoryData(new[] {
            ("3x", "x+2x"),
            ("0", "-x+3x-2x"),
            ("x+2y+3z", "3z+2x+y+y-x+5t-3t-2t"),
            ("2(x+1)^2", "3(x+1)^2 - (1+x)^2"),
            ("4sin(x)", "2sin(x) + 3sin(x) - sin(x)"),
            ("2x^2", "x^2+x^2"),
            ("x^2(1+2x+x^2)", "2x^2-x^2+x^4+x^3+x^3"),

            //TODO: e^x as common factor
            //("2e^x", "e^x+e^x"),
            //("e^x*(y+1)", "e^x + y*e^x"),
            //("e^x*(e+1)", "e^x+e^(x+1)"),
            //("3e^pi", "e^pi+2e^pi"),
            //("xe^x", "xe^x+2xe^x"),
        });

        public static IEnumerable<object[]> CollectMultTestData() => GetTheoryData(new[] {
            ("x^(-2)", "1/(x*x)"),
            ("x*y^2*z^3", "z^3*x^2*y*y/x*t^5/t^3*t^(-2)"),
            ("2(x+1)^2", "3(x+1)^2 - (1+x)^2"),
            ("ln(x)^4*cos(x)^2", "cos(x) * ln(x)^2 * ln(x)^3 / ln(x) * cos x"),
        });

        public static IEnumerable<object[]> DistributionAddTestData() => GetTheoryData(new[] {
            ("x+2", "2(x+1)-x"),
            ("2x", "2(x+1)-2"),
            ("x", "x(x + 1) - x^2"),
            ("3x^2 + 6x - 1", "(x - 1)(x + 3) + (x + 1)(x + 3) + (x + 1)(x - 1)"),
        });

        public static IEnumerable<object[]> MinusDistributionTestData() => GetTheoryData(new[] {
            ("x/y", "(-x)/(-y)"),
            ("-x/y", "x/(-y)"),
            ("-x/(2y)", "x/(-2y)"),
        });

        public static IEnumerable<object[]> PowersTestData() => GetTheoryData(new[] {
            ("1", "1234987132234^0"),
            ("0", "0^1234987132234"),

            ("i^2", "-1"),
            ("-i", "1/i"),
            //TODO
        });

        public static IEnumerable<object[]> TrigKnownValuesTestData() => GetTheoryData(new[] {
            ("2", "2sin(pi/4) sqrt(2)"),
            //TODO
        });

        public static IEnumerable<object[]> FractionsTestData() => GetTheoryData(new[] {
            ("2/3", "4/6"),
            ("1/2", "2/4"),
            ("5/6", "1/2+1/3"),
            ("-1", "1/2-3/2"),
            ("1", "(1/(-2))-((-3)/2)"),
            ("2/x", "1/(x/2)"),
            ("y/x", "1/(x/y)"),
            ("1", "x/(x + y) + y/(x + y)"),
            ("2/x", "1/(x - x/2)"),
            ("2", "2x/(x + y) + 2y/(x + y)"),
            ("1", "-4cos(x)sin(x)/(-4cos(x)sin(x))"),
            ("x^2/y", "x^3/(xy)"),
        });

        //TODO: DistributeMultTestData
        public static IEnumerable<object[]> DistributeMultTestData() => GetTheoryData(new[] {
            ("0", "(sqrt(x)/x)^(-2) - sqr(x)/x"),
            //("sin(x) - cos(x)", "-(cos(x) - sin(x))")
            //("x^2+y^2", "y^2*(1 + x^2/y^2)"),
            //("x^2+y^2", "y^2*(1 + (x/y)^2)"),
            //("1/y^2", "(1 + x^2/y^2)/(x^2 + y^2)"),
            //("2", "(4x+6)/(2x+3)",) //TODO: requires breaking numbers into primes
        });

        //TODO: test trig identities
        public static IEnumerable<object[]> TrigIdentitiesTestData() => GetTheoryData(new[] {
            ("1", "sin(x)^2 + cos(x)^2"),

            //("sin(x)cos(x)", "sin(x) - sin(x)cos(x)"),
            //("cos(2x)", "1 - 2sin(x)^2"),
            //("cos(2x)", "2cos(x)^2 - 1"),
            //("2cos(2x)", "4cos(x)^2 - 2"),  //TODO: this requires changing how common factors are mapped and checked
            //("sin(x)", "tan(x)cos(x)"),
            //("cost(x)", "cot(x)sin(x)"),
            //("1", "tan(x)cot(x)"),
            //("0", "1/cot(x) - sin(x)/cos(x)"),
            //("sin(2*x)", "2sin(x)cos(x)"),
            //("2cos(x)^2", "1 + cos(2*x)"),
            //("2sin(x)^2", "1 - cos(2*x)"),
        });

        //TODO: RepetitiveReductionTest
        public static IEnumerable<object[]> RepetitiveReductionTest() => GetTheoryData(new[] {
            ("2(x + 3)(x + 5)", "(1 - x)(x + 3)(x + 5) + (x + 3)(x + 5)(x + 1)"),
            ("x^2", "x^2*sin(y)^2 + x^2*cos(y)^2"),
            ("x^2", "x^2sin(y)^2sin(z)^2 + x^2sin(y)^2cos(z)^2 + x^2cos(y)^2"),
            ("x^2", "x * (1 - 1 + x*sin(x)^2 + x*cos(x)^2)"),
            ("2", "2*(1 - x/2) + x*sin(x)^2 + x*cos(x)^2"),

            // Identity matching multiple times:
            ("2", "sin(y)^2 + cos(y)^2 + sin(z)^2 + cos(z)^2"),
            
            // Numerics should be collected during repetitive identity matching:
            ("-cos(x)^2", "sin(z)^2 + cos(z)^2 - 2sin(y)^2 - 2cos(y)^2  + sin(x)^2"),

            // Identity matching should be reconsidered after a first match is done:
            //("cos(2x)", "cos(2x)"),   ("2cos(x)^2", "sin(y)^2 + cos(y)^2 + cos(2x)"),      

            //TODO: Identity matching and finding common factors should be executed alternately
            //  ("2cos(x)^2", "sin(y)^2 + cos(y)^2 + sin(z)^2 + cos(z)^2 - 2sin(x)^2"),
            //  ("x", "(1-x^2)*(1-sin(x)^2) + (2x^2-1)*cos(x)^2 + x*sin(x)^2")

            //("1", "cos(2x)^2/(cot(2x) * sin(2x))^2"),
            //("1/cos(x)^2", "1 + sin(x)^2/cos(x)^2"),
            //("2cos(x) + 4", "3cos(x/2)^2 - sin(x/2)^2 + 2sin(2y)^2 + 2cos(2y)^2 + sin(z)^3/sin(z) + cos(z)cos(z)"),
            //("e^(2x)", "(e^x - 2(e^(x/2)*sin(x))^2)^2 + e^x*(e^x*tan(2x)*cot(2x) - e^x*cos(2x)^2)"),
        });
    }
}
