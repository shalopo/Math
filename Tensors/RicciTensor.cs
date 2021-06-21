using MathUtil.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class RicciTensor
    {
        protected RicciTensor(MetricTensor metric)
        {
            Metric = metric;
            Variables = Metric.Variables;
            Matrix = SymmetricMatrix.CreateUninitialized(Variables.Length);
        }

        public MetricTensor Metric { get; }
        public VariableList Variables { get; }
        public SymmetricMatrix Matrix { get; }

        public MathExpr this[MathVariable rowVar, MathVariable colVar] => Matrix[Variables[rowVar], Variables[colVar]];

        public static RicciTensor Create(MetricTensor metric)
        {
            var variables = metric.Variables;
            var christoffels = ChristoffelSymbols.Create(metric);

            var ricci = new RicciTensor(metric);

            for (int rowIndex = 0; rowIndex < variables.Length; rowIndex++)
            {
                var rowCovar = variables[rowIndex];

                // symmetric - calculate diagonal and below
                for (int colIndex = 0; colIndex <= rowIndex; colIndex++)
                {
                    var colCovar = variables[colIndex];

                    var terms = new List<MathExpr>();

                    foreach (var alpha in variables)
                    {
                        terms.Add(christoffels[alpha, rowCovar, colCovar].Derive(alpha));
                        terms.Add(christoffels[alpha, rowCovar, alpha].Derive(colCovar));

                        foreach (var beta in variables)
                        {
                            terms.Add(christoffels[alpha, rowCovar, colCovar] * christoffels[beta, alpha, beta]);
                            terms.Add(christoffels[beta, rowCovar, alpha] * christoffels[alpha, colCovar, beta]);
                        }
                    }

                    ricci.Matrix[rowIndex, colIndex] = AddMathExpr.Create(terms).Reduce(ReduceOptions.DEFAULT);
                }

            }

            return ricci;
        }

        public MathExpr Scalar()
        {
            var inverseMetric = Metric.Inverse();

            var terms = new List<MathExpr>();

            foreach (var var1 in Variables)
            {
                foreach (var var2 in Variables)
                {
                    terms.Add(inverseMetric[var1, var2] * this[var1, var2]);
                }
            }

            return AddMathExpr.Create(terms).Reduce(ReduceOptions.DEFAULT);
        }

        public override string ToString()
        {
            return Matrix.ToString();
        }
    }
}
