using static CSUnit.Assertions.Assertions;
using CSUnit.Attributes;
using FlightSystem.Core;
using FlightSystem.Services;

[DisplayName("Асинхронные сервисы")]
public class AsyncOperationsTests
{
    private readonly BookingService _booking = new(new AirportRegistry());
    private readonly NotificationHub _hub = new();

    [Test]
    [DisplayName("Бронирование с ограничением по времени")]
    public async Task Test_Booking_Timeout()
    {
        var p = new Passenger("U1", "John", true);

        AssertTimeout(TimeSpan.FromMilliseconds(500), () =>
        {
            _booking.BookTicketAsync(p, "ANY-FLIGHT").Wait();
        });
    }

    [Test]
    [DisplayName("Проверка истории уведомлений")]
    public async Task Test_NotificationHistory_State()
    {
        await _hub.SendAsync("P1", "Hello");

        AssertNotNull(_hub.History);
        AssertFalse(_hub.History.Count == 0);
    }

    [Test]
    [DisplayName("Сравнение объектов пассажиров")]
    public void Test_Passenger_Equality()
    {
        var p1 = new Passenger("ID-1", "Ivan", true);
        var p2 = new Passenger("ID-1", "Ivan", true);

        AssertNotSame(p1, p2);
    }
}