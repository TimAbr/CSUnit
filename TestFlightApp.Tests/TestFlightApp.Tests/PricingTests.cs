using CSUnit.Assertions;
using CSUnit.Attributes;
using FlightSystem.Services;

[DisplayName("Модуль тарификации")]
public class PricingTests
{
    private PricingEngine _engine = new();

    [Test]
    [DisplayName("Расчет бизнес-класса")]
    public void Test_BusinessClass_Pricing()
    {
        var flight = new Flight("AF-200", 50);
        decimal price = _engine.GetPrice(flight, isBusinessClass: true);

        Assertions.assertEquals(300m, price);
    }

    [Test]
    [DisplayName("Применение рабочего промокода")]
    public void Test_PromoCode_ChangesPrice()
    {
        decimal initial = 100m;
        decimal discounted = _engine.ApplyPromoCode(initial, "FLY2026");

        Assertions.assertNotEquals(initial, discounted);
        Assertions.assertEquals(90m, discounted);
    }

    [Test]
    [DisplayName("Ошибка при неверном промокоде")]
    public void Test_InvalidPromo_Throws()
    {

        Assertions.assertThrows<ArgumentException>(() => _engine.ApplyPromoCode(100m, "WRONG"));
    }
}