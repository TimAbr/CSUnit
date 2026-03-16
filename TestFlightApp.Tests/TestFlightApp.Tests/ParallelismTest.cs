using CSUnit.Attributes;
using System.Threading;

[DisplayName("Тестирование параллелизма")]
public class ParallelismTest
{
    [Test]
    [DisplayName("Долгая задача #1 (3 сек)")]
    public void LongTask1()
    {
        Thread.Sleep(3000);
    }

    [Test]
    [DisplayName("Долгая задача #2 (2 сек)")]
    public void LongTask2()
    {
        Thread.Sleep(2000);
        throw new Exception();
    }

    [Test]
    [DisplayName("Долгая задача #3 (4 сек)")]
    public void LongTask3()
    {
        Thread.Sleep(4000);
    }

    [Test]
    [DisplayName("Долгая задача #4 (1.5 сек)")]
    public void LongTask4()
    {
        Thread.Sleep(1500);
    }

    [Test]
    [DisplayName("Мгновенная задача #1")]
    public void QuickTask1()
    {
    }

    [Test]
    [DisplayName("Мгновенная задача #2")]
    public void QuickTask2()
    {
    }

    [Test]
    [Timeout(1000)]
    [DisplayName("Тест с таймаутом (должен упасть)")]
    public void TimeoutFailTask()
    {
        Thread.Sleep(2000);
    }
}
