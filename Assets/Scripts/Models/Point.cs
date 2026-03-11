using System;

public readonly struct Point : IEquatable<Point>
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Point other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        // Checks if obj is Point and then cast it to Point (if it is) and call Equals(Point other)
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked // magic number can be any prime number, 23 is commonly used
        {
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }
}