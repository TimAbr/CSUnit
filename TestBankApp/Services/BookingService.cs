using FlightSystem.Core;

namespace FlightSystem.Services
{
    public class BookingService
    {
        private readonly AirportRegistry _registry;

        public BookingService(AirportRegistry registry) => _registry = registry;

        public async Task<string> BookTicketAsync(Passenger p, string flightNumber)
        {
            await Task.Delay(100);

            if (_registry.IsAirportClosed)
                throw new InvalidOperationException("Airport is closed due to weather.");

            var flight = _registry.GetFlight(flightNumber);
            if (flight == null) return null;

            flight.BoardPassenger(p);
            return $"TKT-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}