using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public static class TrigIdentities
    {
        public static List<MathIdentity> Get()
        {
            var x = new MathVariable("X");
            var y = new MathVariable("Y");

            return new List<MathIdentity> {
                new MathIdentity(SIN(x).Pow(TWO) + COS(x).Pow(TWO) - ONE),
                new MathIdentity(SIN(x)/COS(x) - TAN(x)),
                new MathIdentity(COS(x)/SIN(x) - COT(x)),
                new MathIdentity(SIN(TWO * x) - TWO * SIN(x) * COS(x)),
                new MathIdentity(COS(TWO * x) + TWO * SIN(x).Pow(TWO) - ONE),
                new MathIdentity(COS(TWO * x) - TWO * COS(x).Pow(TWO) + ONE),
                new MathIdentity(COS(TWO * x) - TWO * COS(x).Pow(TWO) + SIN(x).Pow(2)),

                //    ONE - TWO * SIN(x).Pow(TWO),
                //    COS(x).Pow(TWO) - SIN(x).Pow(TWO) }),

                //new MathIdentity((x, y) => new MathExpr[]{ SIN(x + y), SIN(x) * COS(y) + COS(x) * SIN(y) }),
                //new MathIdentity(x => new MathExpr[]{ E.Pow(I * x + y), E.Pow(y) * (COS(x) + I * SIN(x)) }),
                //new MathIdentity((x, y) => new MathExpr[]{ LN(x * y), LN(x) + LN(y) })
            };
        }
    }

}
