using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    class AddReducer
    {
        public static MathExpr Reduce(IEnumerable<MathExpr> exprs, bool allow_reduce_to_const_complex = true)
        {
            exprs = (from expr in exprs
                     let expr_reduced = expr.Reduce()
                     where !MathEvalUtil.IsZero(expr_reduced)
                     select expr_reduced is AddMathExpr add_expr ? add_expr.Exprs : new MathExpr[] { expr_reduced }
            ).SelectMany(s => s);

            var multiples = new Dictionary<MathExpr, List<MathExpr>>();

            foreach (var expr in exprs)
            {
                if (!(expr is NumericalConstMathExpr))
                {
                    var term = expr.AsMultTerm();

                    if (multiples.ContainsKey(term.Expr))
                    {
                        multiples[term.Expr].Add(term.Coefficient);
                    }
                    else
                    {
                        multiples.Add(term.Expr, new List<MathExpr> { term.Coefficient });
                    }
                }
            }

            var @const = NumericalConstMathExpr.Add(exprs.OfType<NumericalConstMathExpr>());

            exprs = (from item in multiples
                     let expr = item.Key
                     let multiple = AddMathExpr.Create(item.Value).Reduce()
                     where !MathEvalUtil.IsZero(expr) && !MathEvalUtil.IsZero(multiple)
                     select MathEvalUtil.IsOne(expr) ? multiple :
                            MathEvalUtil.IsOne(multiple) ? expr : (multiple * expr).Reduce());

            if (allow_reduce_to_const_complex && exprs.Count() == 1)
            {
                var expr = exprs.First();
                if (expr.IsConst && (!(expr is ConstMathExpr) || expr.Equals(ImaginaryMathExpr.Instance)))
                {
                    var complex = expr.ComplexEval();
                    var real_part = (@const + complex.Real).RealEval();
                    return complex.HasImagPart ? ConstComplexMathExpr.Create(real_part, complex.Imag) : (MathExpr)real_part;
                }
            }

            if (!MathEvalUtil.IsZero(@const))
            {
                exprs = exprs.Append(@const);
            }

            return AddMathExpr.Create(exprs);
        }

    }
}
