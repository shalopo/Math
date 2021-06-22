using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Matrices
{
    public class SquareMatrix : ISquareMatrix
    {
        protected SquareMatrix(int size)
        {
            Size = size;
            cells = new MathExpr[Size, Size];
        }

        public int Size { get; }
        private readonly MathExpr[,] cells;


        public static SquareMatrix CreateUninitialized(int size)
        {
            return new SquareMatrix(size);
        }

        public static SquareMatrix CreateDefault(int size)
        {
            var matrix = new SquareMatrix(size);

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    matrix[row, col] = row == col ? GlobalMathDefs.ONE : GlobalMathDefs.ZERO;
                }
            }

            return matrix;
        }

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
            return matrix * scalar.Pow(GlobalMathDefs.MINUS_ONE);
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
                    adjugate[col, row] = (sign * this.MinorDeterminant(row, col)).Reduce(ReduceOptions.DEFAULT);

                    sign = -sign;
                }

                rowSign = -rowSign;
            }

            return adjugate;
        }

        public SquareMatrix Inverse()
        {
            return Adjugate() / this.Determinant();
        }

        public override string ToString()
        {
            return ISquareMatrixExtensions.ToString(this);
        }

    }
}
