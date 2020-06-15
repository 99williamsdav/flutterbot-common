using System;

namespace Common.Utils
{
    public static class EloCalculator
    {
        private const double F = 1000;

        private const double K = 32;

        public static double GetProbability(double playerElo, double opponentElo)
        {
            return 1 / (1 + Math.Pow(10, (opponentElo - playerElo) / F));
        }

        public static double GetEloChange(double probability)
        {
            return K * (1 - probability);
        }
    }
}
