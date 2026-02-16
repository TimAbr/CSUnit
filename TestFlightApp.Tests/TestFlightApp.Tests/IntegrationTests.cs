using CSUnit.Assertions;
using CSUnit.Attributes;
using FlightSystem.Core;
using FlightSystem.Services;

[DisplayName("Интеграционные операции")]
public class IntegrationTests
{
    private static AirportRegistry _sharedRegistry;

    [BeforeAll]
    public static void SetupSharedContext()
    {
        _sharedRegistry = new AirportRegistry();
        _sharedRegistry.RegisterFlight(new Flight("G-1", 2));
    }

    [Test]
    [DisplayName("Проверка ссылочной идентичности в реестре")]
    public void Test_Registry_Identity()
    {
        var flight1 = _sharedRegistry.GetFlight("G-1");
        var flight2 = _sharedRegistry.GetFlight("G-1");

        Assertions.assertSame(flight1, flight2);
    }

    [Test]
    [DisplayName("Переполнение рейса")]
    public void Test_Flight_Overfill()
    {
        var flight = _sharedRegistry.GetFlight("G-1");
        var p1 = new Passenger("ID1", "A", true);
        var p2 = new Passenger("ID2", "B", true);
        var p3 = new Passenger("ID3", "C", true);

        flight.BoardPassenger(p1);
        flight.BoardPassenger(p2);

        Assertions.assertThrows<NoSeatsException>(() => flight.BoardPassenger(p3));
    }
}