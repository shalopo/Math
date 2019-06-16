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
            if (b == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }

            return a == 0 ? b : a;
        }


        public static (long, long) ReduceFraction(long a, long b)
        {
            long gcd = Convert.ToInt64(GCD((ulong)Math.Abs(a), (ulong)Math.Abs(b)));
            return (a / gcd, b / gcd);
        }
    }
}
