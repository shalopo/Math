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
        public static long GCD(long a, long b)
        {
            (a, b) = (Math.Abs(a), Math.Abs(b));

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

        public static (double, double) ReduceFraction(double a, double b)
        {
            if (IsWholeNumber(a) && IsWholeNumber(b))
            {
                try
                {
                    var gcd = GCD(Convert.ToInt64(Math.Round(a)), Convert.ToInt64(Math.Round(b)));
                    return (a / gcd, b / gcd);
                }
                catch (OverflowException)
                {
                    return (a, b);
                }
            }

            return (a / b, 1);
        }
    }
}
