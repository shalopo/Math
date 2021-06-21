using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Matrices
{
    public class JacobianMatrix : SquareMatrix
    {
        public VariablesChangeTransformation Transformation { get; }

        private JacobianMatrix(VariablesChangeTransformation transformation) : 
            base(transformation.Sources.Count)
        {
            if (transformation.Sources.Count != transformation.Targets.Count)
            {
                throw new InvalidOperationException("Invalid transformation");
            }

            Transformation = transformation;
        }

        public static JacobianMatrix Create(VariablesChangeTransformation transformation)
        {
            var dimensions = transformation.Sources.Count;
            var matrix = new JacobianMatrix(transformation);

            for (int i = 0; i < dimensions; i++)
            {
                MathVariable source = transformation.Sources[i];

                for (int j = 0; j < dimensions; j++)
                {
                    MathVariable target = transformation.Targets[j];
                    matrix[i, j] = transformation[source].Derive(target);
                }
            }

            return matrix;
        }

    }
}
