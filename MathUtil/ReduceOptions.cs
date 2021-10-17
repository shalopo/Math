using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathUtil
{
    public struct ReduceOptions
    {
        public static ReduceOptions LIGHT = new()
        {
            AllowSearchIdentities = false,
            AllowCommonFactorSearch = false,
        };

        public static ReduceOptions DEFAULT = new()
        {
            AllowSearchIdentities = true,
            AllowCommonFactorSearch = true,
        };

        public ReduceOptions With(bool? allowSearchIdentities = null, bool? allowCommonFactorSearch = null)
        {
            ReduceOptions newOptions = this;

            if (allowSearchIdentities.HasValue)
            {
                newOptions.AllowSearchIdentities = allowSearchIdentities.Value;
            }

            if (allowCommonFactorSearch.HasValue)
            {
                newOptions.AllowCommonFactorSearch = allowCommonFactorSearch.Value;
            }

            return newOptions;
        }

        public bool AllowSearchIdentities { get; private set; }
        public bool AllowCommonFactorSearch { get; private set; }
    }
}
