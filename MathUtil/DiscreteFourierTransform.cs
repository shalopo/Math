using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtil.GlobalMathDefs;

namespace MathUtil
{
    public struct Epicycle
    {
        public Epicycle(double freq, double real, double imag)
        {
            Freq = freq;
            Real = real;
            Imag = imag;
            Size = Math.Sqrt(Real * Real + Imag * Imag);
            Phase = Math.Atan2(Imag, Real);
        }

        public double Size { get; }
        public double Freq { get; }
        public double Phase { get; }
        public double Real { get; }
        public double Imag { get; }
    }

    public class DiscreteFourierTransform
    {
        public static Epicycle[] DFT(double[] x, int maxEpicycles)
        {
            var N = x.Length;
            var two_pi_over_N = -2 * Math.PI / N;

            return Enumerable.Range(0, maxEpicycles).Select(k =>
            {
                double reals = 0;
                double imags = 0;

                double two_pi_k_over_N = two_pi_over_N * k;
                double phase = 0;

                for (int n = 0; n < N; n++)
                {
                    reals += x[n] * Math.Cos(phase);
                    imags += x[n] * Math.Sin(phase);

                    phase += two_pi_k_over_N;
                }

                return new Epicycle(k, reals / N, imags / N);
            })
            .OrderByDescending(epicycle => epicycle.Size)
            .ToArray();
        }
    }
}
