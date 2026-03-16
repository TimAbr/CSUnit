using static CSUnit.Assertions.Assertions;
using CSUnit.Attributes;
using FlightSystem.Services;

[DisplayName("Модуль тарификации")]
public class PricingTests
{
    private readonly PricingEngine _engine = new();

    [Test]
    [DisplayName("Расчет бизнес-класса")]
    public void Test_BusinessClass_Pricing()
    {
        var flight = new Flight("AF-200", 50);
        var price = _engine.GetPrice(flight, isBusinessClass: true);

        AssertNotEquals(300m, price);
    }

    [Test]
    [DisplayName("Применение рабочего промокода")]
    public void Test_PromoCode_ChangesPrice()
    {
        var initial = 100m;
        var discounted = _engine.ApplyPromoCode(initial, "FLY2026");

        AssertNotEquals(initial, discounted);
        AssertEquals(90m, discounted);
    }

    [Test]
    [DisplayName("Ошибка при неверном промокоде")]
    public void Test_InvalidPromo_Throws()
    {
        AssertThrows<ArgumentException>(() => _engine.ApplyPromoCode(100m, "WRONG"));
    }
}