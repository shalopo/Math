using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public class Matrix
    {
        public static Matrix Create(int numRows, int numCols) => new Matrix(numRows, numCols);
        protected Matrix(int numRows, int numCols) => cells = new MathExpr[numRows, numCols];

        public int NumRows => cells.GetLength(0);
        public int NumCols => cells.GetLength(1);

        public MathExpr this[int row, int col]
        {
            get => cells[row, col];
            set => cells[row, col] = value;
        }

        public MathExpr Determinant()
        {
            return InnerDeterminant().Reduce();
        }

        private MathExpr InnerDeterminant()
        {
            if (NumRows != NumCols)
            {
                return 0;
            }

            if (NumRows == 1)
            {
                return cells[0, 0];
            }
            else if (NumRows == 2)
            {
                return cells[0, 0] * cells[1, 1] - cells[0, 1] * cells[1, 0];
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private readonly MathExpr[,] cells;
    }
}
