using BetfairApi.Models;
using Common.Models;
using System.Linq;

namespace BetfairApi
{
    public static class Utils
    {
        public static Odds ConvToOdds(Runner runner)
        {
            return new Odds
            {
                Back = runner.ExchangePrices.AvailableToBack.FirstOrDefault()?.Price,
                BackSize = runner.ExchangePrices.AvailableToBack.FirstOrDefault()?.Size ?? 0,
                Lay = runner.ExchangePrices.AvailableToLay.FirstOrDefault()?.Price,
                LaySize = runner.ExchangePrices.AvailableToLay.FirstOrDefault()?.Size ?? 0
            };
        }
    }
}
