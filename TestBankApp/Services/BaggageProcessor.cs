using FlightSystem.Core;

namespace FlightSystem.Services
{
    public class BaggageProcessor
    {
        public decimal CalculateFee(decimal weight)
        {
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));
            if (weight <= 20) return 0;
            return (weight - 20) * 10; 
        }

        public string GenerateTag(Passenger p, string flightNum)
        {
            return $"TAG-{p.Id}-{flightNum}";
        }
    }
}