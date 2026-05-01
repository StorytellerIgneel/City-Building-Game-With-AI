using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GridServiceTests
{
    private GridService gridService;
    private GameGrid gameGrid;
    private GridOccupancyVisual gridOccupancyVisual;

    [SetUp]
    public void Setup()
    {
        gameGrid = new GameGrid(10, 10);

        GameObject visualObject = new GameObject("GridOccupancyVisual_Test");
        gridOccupancyVisual = visualObject.AddComponent<GridOccupancyVisual>();


        gridService = new GridService(gameGrid, gridOccupancyVisual, 1f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridOccupancyVisual.gameObject);
    }

    [Test]
    public void SnapToGrid_RoundsPositionToNearestGridCell()
    {
        Vector3 position = new Vector3(1.4f, 2.6f, 0f);

        Vector3 result = gridService.SnapToGrid(position);

        Assert.AreEqual(new Vector3(1f, 3f, 0f), result);
    }

    [Test]
    public void GridToWorld_ReturnsCorrectWorldPosition()
    {
        Point point = new Point(2, 3);

        Vector3 result = gridService.GridToWorld(point);

        Assert.AreEqual(new Vector3(2f, 2.5f, 0f), result);
    }

    [Test]
    public void WorldToGrid_ReturnsCorrectGridPoint()
    {
        Vector3 worldPosition = new Vector3(2.8f, 3.9f, 0f);

        Point result = gridService.WorldToGrid(worldPosition);

        Assert.AreEqual(2, result.X);
        Assert.AreEqual(3, result.Y);
    }

    [Test]
    public void IsWithinBounds_ReturnsTrueForValidPoint()
    {
        Point point = new Point(5, 5);

        bool result = gridService.IsWithinBounds(point);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsWithinBounds_ReturnsFalseForOutOfBoundsPoint()
    {
        Point point = new Point(10, 10);

        bool result = gridService.IsWithinBounds(point);

        Assert.IsFalse(result);
    }

    [Test]
    public void RegisterPointsOnGrid_WithEmptyPoints_ReturnsTrueAndOccupiesCells()
    {
        List<Point> points = new List<Point>
        {
            new Point(1, 1),
            new Point(1, 2)
        };

        bool result = gridService.RegisterPointsOnGrid(points, PointType.SmallHouse);

        Assert.IsTrue(result);
        Assert.AreEqual(PointType.SmallHouse, gridService.GetGridCell(new Point(1, 1)).Type);
        Assert.AreEqual(PointType.SmallHouse, gridService.GetGridCell(new Point(1, 2)).Type);
    }

    [Test]
    public void CanPlacePoints_WithOccupiedPoint_ReturnsFalse()
    {
        List<Point> points = new List<Point>
        {
            new Point(1, 1)
        };

        gridService.RegisterPointsOnGrid(points, PointType.SmallHouse);

        bool result = gridService.CanPlacePoints(points);

        Assert.IsFalse(result);
    }

    [Test]
    public void UnregisterPointsOnGrid_WithOccupiedPoints_ReturnsTrueAndClearsCells()
    {
        List<Point> points = new List<Point>
        {
            new Point(1, 1),
            new Point(1, 2)
        };

        gridService.RegisterPointsOnGrid(points, PointType.SmallHouse);

        bool result = gridService.UnregisterPointsOnGrid(points);

        Assert.IsTrue(result);
        Assert.AreEqual(PointType.Empty, gridService.GetGridCell(new Point(1, 1)).Type);
        Assert.AreEqual(PointType.Empty, gridService.GetGridCell(new Point(1, 2)).Type);
    }

    [Test]
    public void RegisterPointsOnGrid_WithOutOfBoundsPoint_ReturnsFalse()
    {
        List<Point> points = new List<Point>
        {
            new Point(20, 20)
        };

        bool result = gridService.RegisterPointsOnGrid(points, PointType.SmallHouse);

        Assert.IsFalse(result);
    }

    [Test]
    public void GetPointsAroundRect_ReturnsPointsAroundBuildingArea()
    {
        Point origin = new Point(2, 2);

        List<Point> result = gridService.GetPointsAroundRect(
            origin,
            width: 2,
            height: 2,
            radius: 1
        );

        Assert.IsTrue(result.Contains(new Point(1, 1)));
        Assert.IsTrue(result.Contains(new Point(4, 4)));
        Assert.IsFalse(result.Contains(new Point(2, 2)));
        Assert.IsFalse(result.Contains(new Point(3, 3)));
    }

    [Test]
    public void HasRoadInRadius_WhenRoadNearby_ReturnsTrue()
    {
        List<Point> roadPoints = new List<Point>
        {
            new Point(1, 2)
        };

        gridService.RegisterPointsOnGrid(roadPoints, PointType.Road);

        bool result = gridService.HasRoadInRadius(
            origin: new Point(2, 2),
            sourceWidth: 2,
            sourceHeight: 2,
            radius: 1
        );

        Assert.IsTrue(result);
    }

    [Test]
    public void HasRoadInRadius_WhenNoRoadNearby_ReturnsFalse()
    {
        bool result = gridService.HasRoadInRadius(
            origin: new Point(2, 2),
            sourceWidth: 2,
            sourceHeight: 2,
            radius: 1
        );

        Assert.IsFalse(result);
    }

    [Test]
    public void GetGridCell_WithOutOfBoundsPoint_ReturnsNull()
    {
        // LogAssert.Expect(LogType.Error,
        //     new Regex("Point out of bounds"));

        GridCell cell = gridService.GetGridCell(new Point(99, 99));

        Assert.IsNull(cell);
    }
}