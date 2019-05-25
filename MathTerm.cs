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

        public static MathTerm operator *(MathTerm term, double mult_coefficient) => 
            new MathTerm(term.Expr, term.Coefficient * mult_coefficient);

        public static MathTerm operator +(MathTerm term, double added_coefficient) =>
            new MathTerm(term.Expr, term.Coefficient + added_coefficient);
    }
}
