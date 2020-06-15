using BetfairApi.Models;
using System.Collections.Generic;

namespace BetfairApi
{
    public static class Projections
    {
        public static readonly PriceProjection Price = new PriceProjection
        {
            PriceData = new HashSet<PriceData> { PriceData.EX_BEST_OFFERS }
        };
    }
}
