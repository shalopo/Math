using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Matrices
{
    public class SquareMatrix
    {
        protected SquareMatrix(int size)
        {
            Size = size;
            cells = new MathExpr[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    cells[row, col] = row == col ? GlobalMathDefs.ONE : GlobalMathDefs.ZERO;
                }
            }
        }

        public int Size { get; }
        private readonly MathExpr[,] cells;

        public static SquareMatrix Create(int size) => new SquareMatrix(size);

        public MathExpr this[int row, int col]
        {
            get => cells[row, col];
            set => cells[row, col] = value;
        }

        public static SquareMatrix operator* (SquareMatrix matrix, MathExpr scalar)
        {
            scalar = scalar.Reduce(ReduceOptions.DEFAULT);

            var scaledMatrix = new SquareMatrix(matrix.Size);

            for (int row = 0; row < matrix.Size; row++)
            {
                for (int col = 0; col < matrix.Size; col++)
                {
                    scaledMatrix[row, col] = (scalar * matrix[row, col]).Reduce(ReduceOptions.DEFAULT);
                }
            }

            return scaledMatrix;
        }

        public static SquareMatrix operator *(MathExpr scalar, SquareMatrix matrix)
        {
            return matrix * scalar;
        }

        public static SquareMatrix operator /(SquareMatrix matrix, MathExpr scalar)
        {
            return matrix * ReciprocalMathExpr.Create(scalar);
        }

        private MathExpr MinorDeterminant(int removeRow, int removeCol)
        {
            var minor = new SquareMatrix(Size - 1);

            for (int row = 0; row < Size; row++)
            {
                if (row == removeRow)
                {
                    continue;
                }

                for (int col = 0; col < Size; col++)
                {
                    if (col == removeCol)
                    {
                        continue;
                    }

                    minor[row < removeRow ? row : row - 1, col < removeCol ? col : col - 1] = cells[row, col];
                }
            }

            return minor.Determinant();
        }

        public MathExpr Determinant()
        {
            if (Size == 2)
            {
                return cells[0, 0] * cells[1, 1] - cells[0, 1] * cells[1, 0];
            }
            else if (Size == 1)
            {
                return cells[0, 0];
            }
            else if (Size == 0)
            {
                return GlobalMathDefs.ONE;
            }

            var terms = new List<MathExpr>();

            int sign = 1;

            for (int col = 0; col < Size; col++)
            {
                terms.Add(sign * cells[0, col] * MinorDeterminant(0, col));

                sign = -sign;
            }

            return AddMathExpr.Create(terms).Reduce(ReduceOptions.DEFAULT);
        }

        public SquareMatrix Adjugate()
        {
            var adjugate = new SquareMatrix(Size);

            int rowSign = 1;

            for (int row = 0; row < Size; row++)
            {
                int sign = rowSign;

                for (int col = 0; col < Size; col++)
                {
                    //transposed
                    adjugate[col, row] = (sign * MinorDeterminant(row, col)).Reduce(ReduceOptions.DEFAULT);

                    sign = -sign;
                }

                rowSign = -rowSign;
            }

            return adjugate;
        }

        public SquareMatrix Inverse()
        {
            var adjugate = Adjugate();
            var determinant = Determinant();

            return adjugate / determinant;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    sb.Append(cells[row, col]).Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

    }
}
