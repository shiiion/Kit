using System;

namespace Kit.Graphics.Types
{
    public class Line
    {
        public Vector2 Point;
        public Vector2 Endpoint;
        public double Width;

        public Line(Vector2 Point = default(Vector2), Vector2 Endpoint = default(Vector2), double Width = 1)
        {
            this.Point = Point;
            this.Endpoint = Endpoint;
            this.Width = Width;
        }

        /// <summary>
        /// Creates a line which, when drawn, appears as a rotated version of a box
        /// </summary>
        /// <param name="box">box to be rotated</param>
        /// <param name="angle">angle in radians to rotate it (about center counterclockwise)</param>
        public void MakeRotatedBox(Box box, double angle)
        {
            Vector2 centerPoint = new Vector2(box.Pos.X + (box.Size.X / 2.0), box.Pos.Y + (box.Size.Y / 2.0));
            Point = new Vector2(box.Pos.X, centerPoint.Y);
            Endpoint = new Vector2(box.Pos.X + box.Size.X, centerPoint.Y);

            Width = box.Size.Y;
            rotateAbout(centerPoint, angle);
        }

        private void rotateAbout(Vector2 point, double angle)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            Point.X -= point.X;
            Point.Y -= point.Y;
            Endpoint.X -= point.X;
            Endpoint.Y -= point.Y;

            double xRot = Point.X * c - Point.Y * s;
            double yRot = Point.X * s + Point.Y * c;
            Point.X = xRot + point.X;
            Point.Y = yRot + point.Y;

            xRot = Endpoint.X * c - Endpoint.Y * s;
            yRot = Endpoint.X * s + Endpoint.Y * c;
            Endpoint.X = xRot + point.X;
            Endpoint.Y = yRot + point.Y;
        }
    }
}
