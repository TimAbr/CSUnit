using FlightSystem.Core;

namespace FlightSystem.Services
{
    public class AirportRegistry
    {
        private readonly List<Flight> _flights = new();
        public bool IsAirportClosed { get; set; } = false;

        public void RegisterFlight(Flight f) => _flights.Add(f);

        public Flight? GetFlight(string number) => _flights.FirstOrDefault(f => f.FlightNumber == number);

        public List<Flight> GetActiveFlights() => _flights.Where(f => f.Status != FlightStatus.Canceled).ToList();
    }
}