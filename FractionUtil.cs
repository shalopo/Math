using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.MathEvalUtil;

namespace MathUtil
{
    public static class FractionUtil
    {
        public static ulong LCM(ulong a, ulong b)
        {
            return a * b / GCD(a, b);
        }

        public static ulong GCD(ulong a, ulong b)
        {
            if (a == 0 || b == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            while (b != 0)
            {
                (a, b) = (b, a % b);
            }

            return a;
        }


        public static (long, long) ReduceFraction(long a, long b)
        {
            long gcd = Convert.ToInt64(GCD((ulong)Math.Abs(a), (ulong)Math.Abs(b)));
            return (a / gcd, b / gcd);
        }
    }
}
