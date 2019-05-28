using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    struct MathTerm
    {
        public MathTerm(MathExpr expr, MathExpr coefficient) => (Expr, Coefficient) = (expr, coefficient);

        public MathExpr Expr { get; }
        public MathExpr Coefficient { get; }

        public MathExpr ToMult()
        {
            if (MathEvalUtil.IsOne(Coefficient))
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

        public static MathTerm operator -(MathTerm term) =>
            new MathTerm(term.Expr, (-term.Coefficient).Reduce());

        public static MathTerm operator *(MathTerm term, double mult_coefficient) => 
            new MathTerm(term.Expr, (term.Coefficient * mult_coefficient).Reduce());

        public static MathTerm operator +(MathTerm term, double added_coefficient) =>
            new MathTerm(term.Expr, (term.Coefficient + added_coefficient).Reduce());
    }
}
