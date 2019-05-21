using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public static class FractionUtil
    {
        private static long GCD(long a, long b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }

        private static long MultiGCD(params long [] arr)
        {
            return arr.Aggregate(GCD);
        }
    }
}
