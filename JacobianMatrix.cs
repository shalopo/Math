using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class JacobianMatrix : Matrix
    {
        private JacobianMatrix(int numRows, int numCols) : base(numRows, numCols)
        {
        }

        public static JacobianMatrix Create(VariablesChangeTransformation transformation)
        {
            JacobianMatrix matrix = new JacobianMatrix(transformation.Sources.Count, transformation.Targets.Count);

            for (int i = 0; i < transformation.Sources.Count; i++)
            {
                MathVariable source = transformation.Sources[i];

                for (int j = 0; j < transformation.Targets.Count; j++)
                {
                    MathVariable target = transformation.Targets[j];
                    matrix[i, j] = transformation[source].Derive(target);
                }
            }

            return matrix;
        }

    }
}
