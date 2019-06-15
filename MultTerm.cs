﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    struct MultTerm
    {
        public MultTerm(MathExpr expr, NumericalConstMathExpr coefficient) => (Expr, Coefficient) = (expr, coefficient);

        public MathExpr Expr { get; }
        public NumericalConstMathExpr Coefficient { get; }

        public MathExpr ToMultExpr()
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

        //public static MultTerm operator *(MultTerm term, NumericalConstMathExpr mult_coefficient) => 
        //    new MultTerm(term.Expr, term.Coefficient.Mult(mult_coefficient));

        public static MultTerm operator -(MultTerm term) => new MultTerm(term.Expr, term.Coefficient.Negate());

        private string ToStringInner(bool added)
        {
            NumericalConstMathExpr positive_coefficient = Coefficient.IsPositive ? Coefficient : Coefficient.Negate();
            var coefficient_sign = Coefficient.IsPositive ? (added ? "+" : "") : "-";
            var space = added ? " " : "";

            if (MathEvalUtil.IsOne(Expr))
            {
                return $"{coefficient_sign}{space}{positive_coefficient}";
            }

            if (positive_coefficient is ConstFractionMathExpr fraction_coefficient)
            {
                if (fraction_coefficient.Top == 1)
                {
                    return $"{coefficient_sign}{space}{Expr.ToMultScopedString()}/{fraction_coefficient.Bottom}";
                }

                return $"{coefficient_sign}{space}{fraction_coefficient.Top}*{Expr.ToMultScopedString()}/{fraction_coefficient.Bottom}";
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