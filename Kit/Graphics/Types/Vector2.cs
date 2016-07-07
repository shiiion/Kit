using System;
using System.Windows;

namespace Kit.Graphics.Types
{
    /// <summary>
    /// Container used to store two doubles, generally for position, dimension, and scaling purposes
    /// </summary>
    public struct Vector2
    {
        public static Vector2 Zero { get; }

        public double X;
        public double Y;

        public Vector2(double X = 0, double Y = 0)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vector2 Add(Vector2 other)
        {
            return new Vector2(X + other.X, Y + other.Y);
        }

        public Vector2 Sub(Vector2 other)
        {
            return new Vector2(X - other.X, Y - other.Y);
        }
        
        public Vector2 Mul(double scale)
        {
            return new Vector2(X * scale, Y * scale);
        }

        public Vector2 Mul(double xScale, double yScale)
        {
            return new Vector2(X * xScale, Y * yScale);
        }

        public Vector2 Mul(Vector2 other)
        {
            return new Vector2(X * other.X, Y * other.Y);
        }

        public void AddThis(Vector2 other)
        {
            X += other.X;
            Y += other.Y;
        }

        public void SubThis(Vector2 other)
        {
            X -= other.X;
            Y -= other.Y;
        }

        public void MulThis(double scale)
        {
            X *= scale;
            Y *= scale;
        }

        public void MulThis(double xScale, double yScale)
        {
            X *= xScale;
            Y *= yScale;
        }

        public void MulThis(Vector2 other)
        {
            X *= other.X;
            Y *= other.Y;
        }

        public double length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public static Vector2 operator +(Vector2 t, Vector2 o)
        {
            return t.Add(o);
        }

        public static Vector2 operator -(Vector2 t, Vector2 o)
        {
            return t.Sub(o);
        }

        public static Vector2 operator *(Vector2 v, double s)
        {
            return v.Mul(s);
        }

        public static Vector2 operator *(Vector2 t, Vector2 o)
        {
            return t.Mul(o);
        }

        public static explicit operator Point(Vector2 cast)
        {
            return new Point((int)cast.X, (int)cast.Y);
        }

        public override string ToString()
        {
            return $"{{{X}, {Y}}}";
        }
    }
}
