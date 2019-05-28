using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    struct MultTerm
    {
        public MultTerm(MathExpr expr, double coefficient) => (Expr, Coefficient) = (expr, coefficient);

        public MathExpr Expr { get; }
        public double Coefficient { get; }

        public MathExpr ToMultExpr()
        {
            if (Coefficient == 1)
            {
                return Expr;
            }

            if (MathEvalUtil.IsOne(Expr))
            {
                return Coefficient;
            }

            if (Expr is MultMathExpr mult)
            {
                return MultMathExpr.Create(mult.Exprs.Prepend(Coefficient));
            }

            return Coefficient * Expr;
        }

        public static MultTerm operator *(MultTerm term, double mult_coefficient) => new MultTerm(term.Expr, term.Coefficient * mult_coefficient);

        public override string ToString()
        {
            var sign = Coefficient >= 0 ? "+" : "-";
            var reduced_coefficient = Math.Abs(Coefficient);

            if (Math.Abs(Coefficient) == 1)
            {
                return $"{sign} {Expr.ToMultScopedString()}";
            }

            if (MathEvalUtil.IsOne(Expr))
            {
                return $"{sign} {reduced_coefficient}";
            }

            return $"{sign} {reduced_coefficient}*{Expr.ToMultScopedString()}";
        }
    }

}
