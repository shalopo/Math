using MathUtil.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class MetricTensor
    {
        protected MetricTensor(VariableList variables, bool initializeDefaults)
        {
            Variables = variables;

            Matrix = initializeDefaults ? SymmetricMatrix.CreateDefault(NumVariables) :
                SymmetricMatrix.CreateUninitialized(NumVariables);

            InverseMetric = new Lazy<MetricTensor>(() => Create(Variables, Matrix.Inverse()));
        }

        public static MetricTensor CreateDefault(VariableList variables)
        {
            return new MetricTensor(variables, initializeDefaults: true);
        }

        public static MetricTensor CreateUninitialized(VariableList variables)
        {
            return new MetricTensor(variables, initializeDefaults: false);
        }

        public static MetricTensor Create(VariableList variables, SymmetricMatrix matrix)
        {
            var tensor = CreateUninitialized(variables);

            for (int row = 0; row < matrix.Size; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    tensor[row, col] = matrix[row, col];
                }
            }

            return tensor;
        }

        public VariableList Variables { get; }
        private SymmetricMatrix Matrix { get; }
        private Lazy<MetricTensor> InverseMetric { get; }

        int NumVariables => Variables.Length;

        public MathExpr this[MathVariable row, MathVariable col]
        {
            get => Matrix[Variables[row], Variables[col]];
            set => Matrix[Variables[row], Variables[col]] = value;
        }

        public MathExpr this[int row, int col]
        {
            get => Matrix[row, col];
            set => Matrix[row, col] = value;
        }

        public MetricTensor ChangeCoordinates(VariablesChangeTransformation transformation)
        {
            var newTensor = CreateUninitialized(transformation.Targets.ToArray());

            var jacobian = JacobianMatrix.Create(transformation);

            // diagonal and below
            for (var row = 0; row < NumVariables; row++)
            {
                for (var col = 0; col <= row; col++)
                {
                    var transformedEntry = CalculateTransformation(row, col, jacobian);
                    newTensor[row, col] = transformedEntry;
                }
            }

            return newTensor;
        }

        private MathExpr CalculateTransformation(int targetRow, int targetCol, JacobianMatrix jacobian)
        {
            List<MathExpr> terms = new List<MathExpr>();

            for (var sourceRow = 0; sourceRow < NumVariables; sourceRow++)
            {
                for (var sourceCol = 0; sourceCol < NumVariables; sourceCol++)
                {
                    var sourceMetricTensorEntry = this[sourceRow, sourceCol].Visit(jacobian.Transformation);

                    if (!MathEvalUtil.IsZero(sourceMetricTensorEntry))
                    {
                        var term = sourceMetricTensorEntry * jacobian[sourceRow, targetRow] * jacobian[sourceCol, targetCol];
                        terms.Add(term);
                    }
                }
            }

            return AddMathExpr.Create(terms).Reduce(ReduceOptions.DEFAULT);
        }

        public override string ToString()
        {
            // This should be an add expression
            var sb = new StringBuilder("ds^2 = ");

            List<MathExpr> terms = new List<MathExpr>();

            for (int row = 0; row < NumVariables; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    var metricTensorEntry = this[row, col];

                    if (!MathEvalUtil.IsZero(metricTensorEntry))
                    {
                        var term = metricTensorEntry * (Variables[row].Delta * Variables[col].Delta);
                        term = term.Reduce(ReduceOptions.DEFAULT);
                        terms.Add(term);
                    }
                }
            }

            sb.Append(AddMathExpr.Create(terms));

            return sb.ToString();
        }

        public MetricTensor Inverse()
        {
            return InverseMetric.Value;
        }

    }
}
