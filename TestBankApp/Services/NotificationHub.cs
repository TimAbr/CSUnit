namespace FlightSystem.Services
{
    public class NotificationHub
    {
        public List<string> History { get; } = new();

        public async Task SendAsync(string passengerId, string message)
        {
            await Task.Delay(50);
            History.Add($"[{passengerId}]: {message}");
        }

        public void Clear() => History.Clear();
    }
}