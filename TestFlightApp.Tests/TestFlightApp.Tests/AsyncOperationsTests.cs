using CSUnit.Assertions;
using CSUnit.Attributes;
using FlightSystem.Core;
using FlightSystem.Services;

[DisplayName("Асинхронные сервисы")]
public class AsyncOperationsTests
{
    private BookingService _booking = new(new AirportRegistry());
    private NotificationHub _hub = new();

    [Test]
    [DisplayName("Бронирование с ограничением по времени")]
    public void Test_Booking_Timeout()
    {
        var p = new Passenger("U1", "John", true);

        Assertions.assertTimeout(TimeSpan.FromMilliseconds(500), () =>
        {
            var task = _booking.BookTicketAsync(p, "ANY-FLIGHT");
            task.Wait();
        });
    }

    [Test]
    [DisplayName("Проверка истории уведомлений")]
    public void Test_NotificationHistory_State()
    {
        var task = _hub.SendAsync("P1", "Hello");
        task.Wait();

        Assertions.assertNotNull(_hub.History);
        Assertions.assertFalse(_hub.History.Count == 0);
    }

    [Test]
    [DisplayName("Сравнение объектов пассажиров")]
    public void Test_Passenger_Equality()
    {
        var p1 = new Passenger("ID-1", "Ivan", true);
        var p2 = new Passenger("ID-1", "Ivan", true);

        Assertions.assertNotSame(p1, p2);
    }
}