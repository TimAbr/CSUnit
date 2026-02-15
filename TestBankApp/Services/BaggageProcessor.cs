using FlightSystem.Core;

namespace FlightSystem.Services
{
    public class BaggageProcessor
    {
        public decimal CalculateFee(decimal weight)
        {
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));
            if (weight <= 20) return 0;
            return (weight - 20) * 10; // 10$ за каждый кг перевеса
        }

        public string GenerateTag(Passenger p, string flightNum)
        {
            return $"TAG-{p.Id}-{flightNum}";
        }
    }
}