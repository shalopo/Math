using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public struct ReduceOptions
    {
        public static ReduceOptions DEFAULT = new ReduceOptions
        {
            AllowSearchIdentities = true,
            AllowReduceToConstComplex = true,
            AllowCommonFactorSearch = true,
        };

        public ReduceOptions With(bool? allowSearchIdentities = null, bool? allowReduceToConstComplex = null,
            bool? allowCommonFactorSearch = null)
        {
            ReduceOptions newOptions = this;

            if (allowSearchIdentities.HasValue)
            {
                newOptions.AllowSearchIdentities = allowSearchIdentities.Value;
            }

            if (allowReduceToConstComplex.HasValue)
            {
                newOptions.AllowReduceToConstComplex = allowReduceToConstComplex.Value;
            }

            if (allowCommonFactorSearch.HasValue)
            {
                newOptions.AllowCommonFactorSearch = allowCommonFactorSearch.Value;
            }

            return newOptions;
        }

        public bool AllowSearchIdentities { get; private set; }
        public bool AllowReduceToConstComplex { get; private set; }
        public bool AllowCommonFactorSearch { get; private set; }
    }
}
