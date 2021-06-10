using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil.Tensors
{
    public class Tensor : BaseTensor
    {
        public Tensor(MathVariable[] covariants, MathVariable[] contravariants) =>
            (Covariants, Contravariants) = (covariants, contravariants);

        public static MathVariable[] NO_VARIABLES = new MathVariable[] { };

        public MathVariable[] Covariants { get; private set; }
        public MathVariable[] Contravariants { get; private set; }

    }
}
