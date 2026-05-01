using NUnit.Framework;

public class ActionPointServiceTest
{
    [Test]
    public void CanAfford_WhenEnoughAP_ReturnsTrue()
    {
        var service = new ActionPointService(3, 3);

        Assert.IsTrue(service.CanAfford(2));
    }

    [Test]
    public void CanAfford_WhenNotEnoughAP_ReturnsFalse()
    {
        var service = new ActionPointService(3, 3);

        Assert.IsFalse(service.CanAfford(5));
    }

    [Test]
    public void TrySpend_WhenValid_ReducesAP()
    {
        var service = new ActionPointService(3, 3);

        bool result = service.TrySpend(2);

        Assert.IsTrue(result);
        Assert.AreEqual(1, service.CurrentAP);
    }

    [Test]
    public void TrySpend_WhenInsufficientAP_ReturnsFalse()
    {
        var service = new ActionPointService(3, 3);

        bool result = service.TrySpend(5);

        Assert.IsFalse(result);
        Assert.AreEqual(3, service.CurrentAP);
    }

    [Test]
    public void RefillToMax_RestoresAP()
    {
        var service = new ActionPointService(3, 3);

        service.TrySpend(2);
        service.RefillToMax();

        Assert.AreEqual(3, service.CurrentAP);
    }

    [Test]
    public void IncreaseMaxAP_UpdatesMaximum()
    {
        var service = new ActionPointService(3, 3);

        service.IncreaseMaxAP(2);

        Assert.AreEqual(5, service.MaxAP);
    }

    [Test]
    public void AddActionPoint_DoesNotExceedMax()
    {
        var service = new ActionPointService(1, 3);

        service.AddActionPoint(5);

        Assert.AreEqual(3, service.CurrentAP);
    }

    [Test]
    public void HasActionPoints_WhenZero_ReturnsFalse()
    {
        var service = new ActionPointService(0, 3);

        Assert.IsFalse(service.HasActionPoints());
    }

    [Test]
    public void AddActionPoint_WhenWithinMax_IncreasesAP()
    {
        var service = new ActionPointService(1, 3);

        bool result = service.AddActionPoint(1);

        Assert.IsTrue(result);
        Assert.AreEqual(2, service.CurrentAP);
    }

    [Test]
    public void UseActionPoint_DecreasesAP()
    {
        var service = new ActionPointService(3, 3);

        service.UseActionPoint(2);

        Assert.AreEqual(1, service.CurrentAP);
    }
}