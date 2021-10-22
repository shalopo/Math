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
    public class PrintTest
    {
        [Theory]
        [MemberData(nameof(GetDoubleSignTestData))]
        [MemberData(nameof(SimpleTestData))]
        public void AssertCanonicalRepresentation(string input)
        {
            var expr = MathParser.Parse(input);
            var output = expr.ToString();
            Assert.Equal(input, output);
        }

        public static IEnumerable<object[]> GetDoubleSignTestData() => new[]{
            "0", "1", "2",
            "1/2", "2/3",
            "x", "1/x", "2/x", "x/2", "2*x/3",
            "x*y", "2*x*y/3", "x/(y*z)", 
            "x^2", "1/x^2", "2^2", "(-2)^2", "1/(-2)^x", "1/(-x)^3", "3*x/(e^x*y^2)",
            "99,999,999,999,999.9", "0.00000000000001", "0.123456789012345",
            }.
            SelectMany(input => new[] { input, "-" + input }).
            Select(input => new object[] { input });

        public static IEnumerable<object[]> SimpleTestData() => new[]{
            "2*x - y + 3*z",
            "-2*x + y - 3*z",
            }.Select(input => new object[] { input });

    }
}
