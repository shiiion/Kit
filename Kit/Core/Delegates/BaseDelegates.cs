

namespace Kit.Core.Delegates
{
    public delegate void VoidDelegate();
    public delegate void StringDelegate(string text);
    public delegate void MouseStateDelegate(Graphics.Types.Vector2 pos, MouseState flags);
    public delegate void KeyStateEventDelegate(System.Windows.Input.Key key, KeyState state);
}