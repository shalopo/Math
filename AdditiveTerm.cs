﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    struct AdditiveTerm
    {
        public AdditiveTerm(MathExpr expr, NumericalConstMathExpr coefficient) => (Expr, Coefficient) = (expr, coefficient);

        public MathExpr Expr { get; }

        //TODO: should coefficients should be fixed to include additional consts which are not considered numerical,
        // but actually are? e.g. (8*pi^2*e + 2/3) should be a compound const and as such a valid coefficient
        public NumericalConstMathExpr Coefficient { get; }

        public MathExpr GetFullExpr()
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
                return MultMathExpr.Create(mult.Terms.Prepend(Coefficient));
            }

            return Coefficient * Expr;
        }

        public static AdditiveTerm operator -(AdditiveTerm term) => new AdditiveTerm(term.Expr, term.Coefficient.Negate());

        private string ToStringInner(bool added)
        {
            //TODO: this does too much. All it should do is deide whether to add +- and a space

            bool isNegative = !Coefficient.IsPositive && !MathEvalUtil.IsZero(Coefficient);

            NumericalConstMathExpr positive_coefficient = isNegative ? Coefficient.Negate() : Coefficient;
            var coefficient_sign = isNegative ? "-" : (added ? "+" : "");
            var space = added ? " " : "";

            if (MathEvalUtil.IsZero(Expr))
            {
                return "0";
            }
            else if (MathEvalUtil.IsOne(Expr))
            {
                return $"{coefficient_sign}{space}{positive_coefficient}";
            }

            if (positive_coefficient is ConstFractionMathExpr fraction_coefficient)
            {
                if (fraction_coefficient.Top == 1)
                {
                    return $"{coefficient_sign}{space}{Expr.ToMultScopedString()}/{fraction_coefficient.BottomAsString}";
                }

                return $"{coefficient_sign}{space}{fraction_coefficient.TopAsString}*" +
                       $"{Expr.ToMultScopedString()}/{fraction_coefficient.BottomAsString}";
            }
            else
            {
                if (MathEvalUtil.IsOne(positive_coefficient))
                {
                    return $"{coefficient_sign}{space}{Expr.ToMultScopedString()}";
                }
                
                return $"{coefficient_sign}{space}{positive_coefficient}*{Expr.ToMultScopedString()}";
            }
        }

        public string ToAddedString()
        {
            return ToStringInner(true);
        }

        public override string ToString()
        {
            return ToStringInner(false);
        }
    }

}
