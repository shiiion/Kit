using System;

namespace Kit.Core
{
    /// <summary>
    /// BIT 0: set = down, reset = up
    /// BIT 1: set = left, reset = right
    /// </summary>
    [Flags]
    public enum MouseState
    {
        LeftDown = Down | Left,
        LeftUp = Left,
        RightDown = Down,
        RightUp = 0,

        Down = 0x01,
        Left = 0x02
    }

    public enum KeyState
    {
        Press,
        Hold,
        Release
    }
}