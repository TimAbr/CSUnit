using FlightSystem.Core;

namespace FlightSystem.Services
{
    public class Flight
    {
        public string FlightNumber { get; }
        public int Capacity { get; private set; }
        public List<Passenger> Passengers { get; } = new();
        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public Flight(string number, int capacity)
        {
            FlightNumber = number;
            Capacity = capacity;
        }

        public void BoardPassenger(Passenger p)
        {
            if (!p.HasPassport) throw new SecurityException($"Passenger {p.Name} has no documents.");
            if (Passengers.Count >= Capacity) throw new NoSeatsException();

            Passengers.Add(p);
        }

        public void ChangeCapacity(int newCapacity)
        {
            if (newCapacity < Passengers.Count)
                throw new InvalidOperationException("Capacity cannot be less than current passengers.");
            Capacity = newCapacity;
        }
    }
}