using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class JacobianMatrix : Matrix
    {
        private JacobianMatrix(int dimensions) : base(dimensions, dimensions)
        {
        }

        public static JacobianMatrix Create(VariablesChangeTransformation transformation)
        {
            Debug.Assert(transformation.Sources.Count == transformation.Targets.Count);

            var dimensions = transformation.Sources.Count;
            var matrix = new JacobianMatrix(dimensions);

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
