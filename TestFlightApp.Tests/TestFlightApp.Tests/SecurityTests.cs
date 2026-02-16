using CSUnit.Assertions;
using CSUnit.Attributes;
using FlightSystem.Core;
using FlightSystem.Services;

[DisplayName("Служба безопасности аэропорта")]
public class SecurityTests
{
    [Test]
    [DisplayName("Отказ в посадке без паспорта")]
    public void Test_MissingPassport_ThrowsException()
    {
        var passenger = new Passenger("P1", "Ivan", false);
        var flight = new Flight("SU-100", 100);

        Assertions.assertThrows<SecurityException>(() => flight.BoardPassenger(passenger));
    }

    [Test]
    [DisplayName("Валидация данных пассажира")]
    public void Test_PassengerData_NotNull()
    {
        var passenger = new Passenger("P2", "Maria", true);

        Assertions.assertNotNull(passenger.Id);
        Assertions.assertTrue(passenger.HasPassport);
    }

    [Test]
    [DisplayName("Проверка черного списка (пустой результат)")]
    public void Test_Blacklist_ReturnsNull()
    {
        var registry = new AirportRegistry();
        var result = registry.GetFlight("FAKE-999");

        Assertions.assertNull(result);
    }
}