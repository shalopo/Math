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
            AllowFullCoverageFactor = false,
            AllowDistributeTerms = false,
        };

        public static ReduceOptions DEFAULT = new()
        {
            AllowSearchIdentities = true,
            AllowCommonFactorSearch = true,
            AllowFullCoverageFactor = true,
            AllowDistributeTerms = true,
        };

        public ReduceOptions With(bool? allowSearchIdentities = null, bool? allowCommonFactorSearch = null,
            bool? allowFullCoverageFactor = null, bool? allowDistributeTerms = null)
        {
            return new ReduceOptions
            {
                AllowSearchIdentities = allowSearchIdentities ?? AllowSearchIdentities,
                AllowCommonFactorSearch = allowCommonFactorSearch ?? AllowCommonFactorSearch,
                AllowFullCoverageFactor = allowFullCoverageFactor ?? AllowFullCoverageFactor,
                AllowDistributeTerms = allowDistributeTerms ?? AllowDistributeTerms,
            };
        }

        public bool AllowSearchIdentities { get; private set; }
        public bool AllowCommonFactorSearch { get; private set; }
        public bool AllowFullCoverageFactor { get; private set; }
        public bool AllowDistributeTerms { get; private set; }
    }
}
