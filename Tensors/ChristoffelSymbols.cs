using MathUtil.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class ChristoffelSymbols
    {
        private ChristoffelSymbols(VariableList variables)
        {
            Variables = variables;

            Covariants = new Dictionary<MathVariable, SymmetricMatrix>();

            foreach (MathVariable variable in variables)
            {
                Covariants[variable] = SymmetricMatrix.CreateUninitialized(Variables.Length);
            }
        }

        public VariableList Variables { get; }
        private Dictionary<MathVariable, SymmetricMatrix> Covariants { get; }

        public MathExpr this[MathVariable contravar, MathVariable rowCovar, MathVariable colCovar] =>
            Covariants[contravar][Variables[rowCovar], Variables[colCovar]];

        public static ChristoffelSymbols Create(MetricTensor metric)
        {
            var variables = metric.Variables;
            var christoffels = new ChristoffelSymbols(variables);

            var inverseMetric = metric.Inverse();

            for (int rowIndex = 0; rowIndex < variables.Length; rowIndex++)
            {
                var rowCovar = variables[rowIndex];

                // symmetric on covriant indices - calculate diagonal and below
                for (int colIndex = 0; colIndex <= rowIndex; colIndex++)
                {
                    var colCovar = variables[colIndex];

                    var sums = variables.ToDictionary(summedContravar => summedContravar, summedContravar => (
                        metric[summedContravar, rowCovar].Derive(colCovar) +
                        metric[summedContravar, colCovar].Derive(rowCovar) -
                        metric[rowCovar, colCovar].Derive(summedContravar)
                        ).Reduce(ReduceOptions.DEFAULT)
                    );

                    foreach (var targetContravar in variables)
                    {
                        christoffels.Covariants[targetContravar][rowIndex, colIndex] = (GlobalMathDefs.HALF * 
                            AddMathExpr.Create(variables.Select(summedContravar => 
                                               inverseMetric[summedContravar, targetContravar] * sums[summedContravar]))).
                            Reduce(ReduceOptions.DEFAULT);
                    }
                }
            }

            return christoffels;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var contravar in Variables)
            {
                sb.AppendLine($"{contravar} =>").AppendLine(Covariants[contravar].ToString());
            }

            return sb.ToString();
        }
    }
}
