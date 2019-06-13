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
            long gcd = Convert.ToInt64(GCD((ulong)a, (ulong)b));
            return (a / gcd, b / gcd);
        }

        public static (double, double) ReduceFraction(double a, double b)
        {
            if (IsWholeNumber(a) && IsWholeNumber(b))
            {
                try
                {
                    return ReduceFraction(Convert.ToInt64(Math.Round(a)), Convert.ToInt64(Math.Round(b)));
                }
                catch (OverflowException)
                {
                    return (a, b);
                }
            }

            if (b == 0)
            {
                throw new UndefinedMathBehavior("Division by zero");
            }

            return (a / b, 1);
        }
    }
}
