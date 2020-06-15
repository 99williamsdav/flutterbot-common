namespace Common.Models
{
    public class Odds
    {
        public double? Back { get; set; }

        public double? Lay { get; set; }

        public double BackSize { get; set; }

        public double LaySize { get; set; }

        public double? Probability => 1 / Back;

        public double? LayProbability => 1 / Lay;

        public double? Spread => Lay / Back;

        public double? Mid => (Back + Lay) / 2;

        public double? MidProbability => 1 / Mid;

        public double? LastTraded { get; set; }

        public double? LastTradedProbability => 1 / LastTraded;

        //public double? ExpectedProbability { get; set; }

        //public decimal? Expected => 1 / (decimal)ExpectedProbability;

        public override string ToString()
        {
            return $"{Back} - {Lay}";
        }
    }
}
