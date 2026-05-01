using NUnit.Framework;
public class GoldServiceTest
{

    [Test]
    public void CanAfford_WhenGoldIsEnough_ReturnsTrue()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.CanAfford(50);

        Assert.IsTrue(result);
    }

    [Test]
    public void CanAfford_WhenGoldIsNotEnough_ReturnsFalse()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.CanAfford(150);

        Assert.IsFalse(result);
    }

    [Test]
    public void TrySpend_WhenGoldIsEnough_ReducesGoldAndReturnsTrue()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.TrySpend(40);

        Assert.IsTrue(result);
        Assert.AreEqual(60, goldService.CurrentGold);
    }

    [Test]
    public void TrySpend_WhenGoldIsNotEnough_DoesNotReduceGoldAndReturnsFalse()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.TrySpend(150);

        Assert.IsFalse(result);
        Assert.AreEqual(100, goldService.CurrentGold);
    }

    [Test]
    public void AddGold_WhenAmountIsPositive_IncreasesGoldAndReturnsTrue()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.AddGold(50);

        Assert.IsTrue(result);
        Assert.AreEqual(150, goldService.CurrentGold);
    }

    [Test]
    public void AddGold_WhenAmountIsNegative_DoesNotChangeGoldAndReturnsFalse()
    {
        var goldService = new GoldService(100, null);

        bool result = goldService.AddGold(-50);

        Assert.IsFalse(result);
        Assert.AreEqual(100, goldService.CurrentGold);
    }

    [Test]
    public void TrySpend_WhenSuccessful_InvokesGoldChangedEvent()
    {
        var goldService = new GoldService(100, null);

        ResourceType changedType = default;
        int changedValue = -1;

        goldService.OnResourceChanged += (type, value) =>
        {
            changedType = type;
            changedValue = value;
        };

        goldService.TrySpend(30);

        Assert.AreEqual(ResourceType.Gold, changedType);
        Assert.AreEqual(70, changedValue);
    }

    [Test]
    public void AddGold_WhenSuccessful_InvokesGoldChangedEvent()
    {
        var goldService = new GoldService(100, null);

        ResourceType changedType = default;
        int changedValue = -1;

        goldService.OnResourceChanged += (type, value) =>
        {
            changedType = type;
            changedValue = value;
        };

        goldService.AddGold(25);

        Assert.AreEqual(ResourceType.Gold, changedType);
        Assert.AreEqual(125, changedValue);
    }
}