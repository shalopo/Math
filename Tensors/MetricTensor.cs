using MathUtil.Matrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class MetricTensor : BaseTensor
    {
        public MetricTensor(MathVariable[] variables)
        {
            Variables = variables;
            VariableMapping = new Dictionary<MathVariable, int>(Variables.Length);

            for (int i = 0; i < Variables.Length; i++)
            {
                VariableMapping[Variables[i]] = i;
            }

            // symmetric - we only need the diagonal and below
            Array = new MathExpr[NumVariables * (NumVariables + 1) / 2];

            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = GlobalMathDefs.ZERO;
            }
        }

        public static MetricTensor Identity(MathVariable[] variables)
        {
            var tensor = new MetricTensor(variables);

            for (int i = 0; i < variables.Length; i++)
            {
                tensor[i, i] = GlobalMathDefs.ONE;
            }

            return tensor;
        }

        private MathVariable[] Variables { get; set; }
        private Dictionary<MathVariable, int> VariableMapping { get; set; }
        private MathExpr[] Array { get; set; }

        int NumVariables => Variables.Length;

        private int GetVariableIndex(MathVariable v)
        {
            return VariableMapping[v];
        }

        private int GetArrayIndex(MathVariable var1, MathVariable var2)
        {
            return GetArrayIndex(GetVariableIndex(var1), GetVariableIndex(var2));
        }

        private int GetArrayIndex(int varIndex1, int varIndex2)
        {
            // symmetric matrix - we only store the diagonal and below
            var row = Math.Max(varIndex1, varIndex2);
            var col = Math.Min(varIndex1, varIndex2);

            var index = row * (row + 1) / 2 + col;
            
            return index;
        }

        public MathExpr this[int varIndex1, int varIndex2]
        {
            get => Array[GetArrayIndex(varIndex1, varIndex2)];
            set => Array[GetArrayIndex(varIndex1, varIndex2)] = value;
        }

        public MathExpr this[MathVariable u, MathVariable v]
        {
            get => Array[GetArrayIndex(u, v)];
            set => Array[GetArrayIndex(u, v)] = value;
        }

        //internal override MathExpr Visit(IMathExprTransformer transformer)
        //{
        //    var transformation = new VariablesChangeTransformation();
        //    return ChangeCoordinates(transformation);
        //}

        public MetricTensor ChangeCoordinates(VariablesChangeTransformation transformation)
        {
            var newTensor = new MetricTensor(transformation.Targets.ToArray());

            var jacobian = JacobianMatrix.Create(transformation);

            for (var targetVarIndex1 = 0; targetVarIndex1 < NumVariables; targetVarIndex1++)
            {
                // diagonal and below

                for (var targetVarIndex2 = 0; targetVarIndex2 <= targetVarIndex1; targetVarIndex2++)
                {
                    var transformedEntry = CalculateTransformation(targetVarIndex1, targetVarIndex2, jacobian);
                    newTensor[targetVarIndex1, targetVarIndex2] = transformedEntry;
                }
            }

            return newTensor;
        }

        private MathExpr CalculateTransformation(int targetVarIndex1, int targetVarIndex2, JacobianMatrix jacobian)
        {
            List<MathExpr> terms = new List<MathExpr>();

            for (var sourceVarIndex1 = 0; sourceVarIndex1 < NumVariables; sourceVarIndex1++)
            {
                for (var sourceVarIndex2 = 0; sourceVarIndex2 < NumVariables; sourceVarIndex2++)
                {
                    var sourceMetricTensorEntry = this[sourceVarIndex1, sourceVarIndex2].Visit(jacobian.Transformation);

                    if (!MathEvalUtil.IsZero(sourceMetricTensorEntry))
                    {
                        var term = sourceMetricTensorEntry * jacobian[sourceVarIndex1, targetVarIndex1] * jacobian[sourceVarIndex2, targetVarIndex2];
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

            for (int varIndex1 = 0; varIndex1 < NumVariables; varIndex1++)
            {
                for (int varIndex2 = 0; varIndex2 <= varIndex1; varIndex2++)
                {
                    var metricTensorEntry = this[varIndex1, varIndex2];

                    if (!MathEvalUtil.IsZero(metricTensorEntry))
                    {
                        var term = metricTensorEntry * (Variables[varIndex1].Delta * Variables[varIndex2].Delta);
                        term = term.Reduce(ReduceOptions.DEFAULT);
                        terms.Add(term);
                    }
                }
            }

            sb.Append(AddMathExpr.Create(terms));

            return sb.ToString();
        }

    }
}
