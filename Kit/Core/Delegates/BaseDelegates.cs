

namespace Kit.Core.Delegates
{
    public delegate void VoidDelegate();
    public delegate void StringDelegate(string text);
    public delegate void MouseStateDelegate(Graphics.Types.Vector2 pos, MouseState flags);
    public delegate void KeyStateDelegate(System.Windows.Input.Key key, KeyState state);
    public delegate void MouseMoveDelegate(MouseState state, Graphics.Types.Vector2 start, Graphics.Types.Vector2 end);
    public delegate void MouseScrollDelegate(Graphics.Types.Vector2 pos, int direction);
}