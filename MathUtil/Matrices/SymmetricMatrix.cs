using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Matrices
{
    public class SymmetricMatrix : ISquareMatrix
    {
        protected SymmetricMatrix(int size)
        {
            Size = size;
            Cells = new MathExpr[Size * (Size + 1) / 2];
        }

        public int Size { get; }

        public static SymmetricMatrix CreateUninitialized(int size)
        {
            return new SymmetricMatrix(size);
        }
         
        public static SymmetricMatrix CreateDefault(int size)
        {
            var matrix = new SymmetricMatrix(size);

            for (int i = 0; i < matrix.Cells.Length; i++)
            {
                matrix.Cells[i] = GlobalMathDefs.ZERO;
            }

            for (int row = 0; row < size; row++)
            {
                matrix[row, row] = GlobalMathDefs.ONE;
            }

            return matrix;
        }

        // diagonal and below
        private MathExpr[] Cells { get; set; }

        private int GetArrayIndex(int row, int col)
        {
            var storageRow = Math.Max(row, col);
            var storageCol = Math.Min(row, col);

            var index = storageRow * (storageRow + 1) / 2 + storageCol;

            return index;
        }

        public MathExpr this[int row, int col]
        {
            get => Cells[GetArrayIndex(row, col)];
            set => Cells[GetArrayIndex(row, col)] = value;
        }

        public static SymmetricMatrix operator *(SymmetricMatrix matrix, MathExpr scalar)
        {
            scalar = scalar.Reduce(ReduceOptions.DEFAULT);

            var scaledMatrix = new SymmetricMatrix(matrix.Size);

            for (int i = 0; i < scaledMatrix.Cells.Length; i++)
            {
                 scaledMatrix.Cells[i] = (scalar * matrix.Cells[i]).Reduce(ReduceOptions.DEFAULT);
            }

            return scaledMatrix;
        }

        public static SymmetricMatrix operator *(MathExpr scalar, SymmetricMatrix matrix)
        {
            return matrix * scalar;
        }

        public static SymmetricMatrix operator /(SymmetricMatrix matrix, MathExpr scalar)
        {
            return matrix * scalar.Pow(GlobalMathDefs.MINUS_ONE);
        }

        public SymmetricMatrix Adjugate()
        {
            var adjugate = new SymmetricMatrix(Size);

            int rowSign = 1;

            for (int row = 0; row < Size; row++)
            {
                int sign = rowSign;

                for (int col = 0; col <= row; col++)
                {
                    //transposed
                    adjugate[col, row] = (sign * this.MinorDeterminant(row, col)).Reduce(ReduceOptions.DEFAULT);

                    sign = -sign;
                }

                rowSign = -rowSign;
            }

            return adjugate;
        }

        public SymmetricMatrix Inverse()
        {
            return Adjugate() / this.Determinant();
        }

        public override string ToString()
        {
            return ISquareMatrixExtensions.ToString(this);
        }

    }
}
