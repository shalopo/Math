﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class MultReducer
    {
        public static MathExpr Reduce(IEnumerable<MathExpr> exprs)
        {
            exprs = (from expr in exprs select expr.Reduce());

            exprs = (from expr in exprs select expr is MultMathExpr mult_expr ? mult_expr.Exprs : new MathExpr[] { expr }
                ).SelectMany(s => s).ToList();

            var powers = new Dictionary<MathExpr, List<MathExpr>>();
            foreach (var expr in exprs)
            {
                var pow = expr.AsPowerExpr();
                if (powers.ContainsKey(pow.Base))
                {
                    //TODO: input range validity: x/x is 1 for x!=0
                    powers[pow.Base].Add(pow.Exponent);
                }
                else
                {
                    powers.Add(pow.Base, new List<MathExpr> { pow.Exponent });
                }
            }

            exprs = (from item in powers
                     let @base = item.Key
                     let exponent = AddMathExpr.Create(item.Value).Reduce()
                     select (exponent is NumericalConstMathExpr exponent_numerical && !exponent_numerical.IsPositive) ?
                        ReciprocalMathExpr.Create(PowerMathExpr.Create(@base, exponent_numerical.Negate())).Reduce() :
                        PowerMathExpr.Create(@base, exponent).Reduce());

            var coefficient = NumericalConstMathExpr.Mult(exprs.OfType<NumericalConstMathExpr>());

            if (MathEvalUtil.IsZero(coefficient))
            {
                return GlobalMathDefs.ZERO;
            }

            exprs = exprs.Where(expr => !(expr is NumericalConstMathExpr));

            //slupu: const * i => ConstComplex

            if (!coefficient.Equals(GlobalMathDefs.ONE))
            {
                exprs = exprs.Prepend(coefficient);
            }

            return MultMathExpr.Create(exprs);
        }

    }
}
