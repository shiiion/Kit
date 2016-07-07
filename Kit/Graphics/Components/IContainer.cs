using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    public interface IContainer<T>
    {
        void SetParent(T parent);
        void AddChild(T child);
        Vector2 GetLocation();
        Vector2 GetAbsoluteLocation();
    }

    public enum KitAnchoring
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        LeftCenter = 3,
        Center = 4,
        RightCenter = 5,
        BottomLeft = 6,
        BottomCenter = 7,
        BottomRight = 8,
        /// <summary>
        /// The anchoring is based off of Vector2 CustomAnchor location
        /// OR
        /// The origin is based off of Vector2 CustomOrigin location
        /// </summary>
        CustomAnchor = 9
    };
}
