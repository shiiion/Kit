using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Types;
using Kit.Graphics.Components;

namespace Kit.Graphics.Components
{
    interface IScrollable
    {
        bool ContentLargerThanArea();
        Vector2 ContentDimensions();
        void SetScrollbar(KitScrollbar scrollBar);

        bool ContainsCursor(Vector2 cursorLoc);
    }
}
