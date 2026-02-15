namespace FlightSystem.Core
{
    public class FlightException : Exception { public FlightException(string msg) : base(msg) { } }
    public class NoSeatsException : FlightException { public NoSeatsException() : base("No available seats.") { } }
    public class SecurityException : FlightException { public SecurityException(string msg) : base(msg) { } }

    public record Passenger(string Id, string Name, bool HasPassport);
    public enum FlightStatus { Scheduled, Delayed, Departed, Canceled }
}