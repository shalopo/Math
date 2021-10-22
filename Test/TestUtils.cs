using MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Test
{
    public static class TestUtils
    {
        public static void AssertEqual(MathExpr expected, MathExpr actual)
        {
            if (!expected.Equals(actual))
            {
                throw new XunitException($"expected: {expected}{Environment.NewLine}actual: {actual}");
            }
        }

    }
}
