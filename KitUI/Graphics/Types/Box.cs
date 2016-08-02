
namespace Kit.Graphics.Types
{
    /// <summary>
    /// Container used to store data representing an axis aligned box
    /// </summary>
    public struct Box
    {
        /// <summary>
        /// Position represented by a vector
        /// </summary>
        public Vector2 Pos { get; set; }

        /// <summary>
        /// Dimensions represented by a vector
        /// </summary>
        public Vector2 Size { get; set; }

        public Box(Vector2 Pos = default(Vector2), Vector2 Size = default(Vector2))
        {
            this.Pos = Pos;
            this.Size = Size;
        }

        public Box(Vector2 Pos = default(Vector2), double W = 0, double H = 0)
        {
            this.Pos = Pos;
            Size = new Vector2(W, H);
        }

        public Box(double X = 0, double Y = 0, Vector2 Size = default(Vector2))
        {
            Pos = new Vector2(X, Y);
            this.Size = Size;
        }

        public Box(double X = 0, double Y = 0, double W = 0, double H = 0)
        {
            Pos = new Vector2(X, Y);
            Size = new Vector2(W, H);
        }

        /// <summary>
        /// Returns whether or not a point is contained within this box
        /// </summary>
        /// <param name="point">the point to be checked</param>
        /// <param name="includeEdges">whether or not the point is allowed to be along the edge of the box</param>
        /// <returns>whether or not the point is contained within this box</returns>
        public bool Contains(Vector2 point, bool includeEdges = true)
        {
            if (includeEdges)
            {
                return (point.X >= Pos.X) && (point.Y >= Pos.Y) &&
                    (point.X <= Pos.X + Size.X) && (point.Y <= Pos.Y + Size.Y);
            }
            else
            {
                return (point.X > Pos.X) && (point.Y > Pos.Y) &&
                    (point.X < Pos.X + Size.X) && (point.Y < Pos.Y + Size.Y);
            }
        }
    }
}
