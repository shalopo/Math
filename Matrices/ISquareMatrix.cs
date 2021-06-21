using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Matrices
{
    public interface ISquareMatrix
    {
        int Size { get; }
        MathExpr this[int row, int col] { get; set; }
    }


    public static class ISquareMatrixExtensions
    {
        public static MathExpr Determinant(this ISquareMatrix matrix)
        {
            if (matrix.Size == 2)
            {
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            }
            else if (matrix.Size == 1)
            {
                return matrix[0, 0];
            }
            else if (matrix.Size == 0)
            {
                return GlobalMathDefs.ONE;
            }

            var terms = new List<MathExpr>();

            int sign = 1;

            for (int col = 0; col < matrix.Size; col++)
            {
                if (!MathEvalUtil.IsZero(matrix[0, col]))
                {
                    terms.Add(sign * matrix[0, col] * matrix.MinorDeterminant(0, col));
                }

                sign = -sign;
            }

            return AddMathExpr.Create(terms).Reduce(ReduceOptions.DEFAULT);
        }

        public static MathExpr MinorDeterminant(this ISquareMatrix matrix, int removeRow, int removeCol)
        {
            var minor = SquareMatrix.CreateUninitialized(matrix.Size - 1);

            for (int row = 0; row < matrix.Size; row++)
            {
                if (row == removeRow)
                {
                    continue;
                }

                for (int col = 0; col < matrix.Size; col++)
                {
                    if (col == removeCol)
                    {
                        continue;
                    }

                    minor[row < removeRow ? row : row - 1, col < removeCol ? col : col - 1] = matrix[row, col];
                }
            }

            return minor.Determinant();
        }

        public static string ToString(this ISquareMatrix matrix)
        {
            var sb = new StringBuilder();

            for (int row = 0; row < matrix.Size; row++)
            {
                for (int col = 0; col < matrix.Size; col++)
                {
                    sb.Append(matrix[row, col]).Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

    }
}
