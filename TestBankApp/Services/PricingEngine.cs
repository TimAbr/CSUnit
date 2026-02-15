namespace FlightSystem.Services
{
    public class PricingEngine
    {
        public decimal GetPrice(Flight f, bool isBusinessClass)
        {
            decimal basePrice = 100m;
           
            decimal occupancyLoad = (decimal)f.Passengers.Count / f.Capacity;

            decimal finalPrice = isBusinessClass ? basePrice * 3 : basePrice;
            return occupancyLoad > 0.8m ? finalPrice * 1.5m : finalPrice;
        }

        public decimal ApplyPromoCode(decimal price, string code)
        {
            if (code == "FLY2026") return price * 0.9m;
            if (string.IsNullOrEmpty(code)) return price;
            throw new ArgumentException("Invalid promo code");
        }
    }
}