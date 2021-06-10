//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static MathUtil.GlobalMathDefs;

//namespace MathUtil
//{
//    public class MathIdentity
//    {
//    }

//    public static class TrigIdentities
//    {
//        public static List<MathIdentity> Get()
//        {
//            return new List<MathIdentity> {
//                new MathIdentity(x => new MathExpr[]{ SIN(x).Pow(TWO) + COS(x).Pow(TWO), ONE }),
//                new MathIdentity(x => new MathExpr[]{ SIN(TWO * x), TWO * SIN(x) * COS(x) }),

//                new MathIdentity(x => new MathExpr[]{ COS(TWO * x),
//                    TWO * COS(x).Pow(TWO) - ONE,
//                    ONE - TWO * SIN(x).Pow(TWO),
//                    COS(x).Pow(TWO) - SIN(x).Pow(TWO) }),

//                new MathIdentity((x, y) => new MathExpr[]{ SIN(x + y), SIN(x) * COS(y) + COS(x) * SIN(y) }),
//                new MathIdentity(x => new MathExpr[]{ E.Pow(I * x), COS(x) + I * SIN(x) }),
//                new MathIdentity((x, y) => new MathExpr[]{ LN(x * y), LN(x) + LN(y) })
//            };
//        }
//    }

//    public static class MathIdentitiesManager
//    {
//        public static void Register(MathIdentity identity);

//        public static void Register(List<MathIdentity> identities)
//        {
//            foreach (var identity in identities)
//            {
//                Register(identity);
//            }
//        }

//        static MathIdentitiesManager()
//        {
//            Register(TrigIdentities.Get());
//        }
//    }

//}
