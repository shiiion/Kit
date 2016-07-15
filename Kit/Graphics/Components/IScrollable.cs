using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Graphics.Types;

namespace Kit.Graphics.Components
{
    interface IScrollable
    {
        bool ContentLargerThanArea();
        Vector2 ContentDimensions();
        void SetContentLocation(Vector2 location);

        void SetScrollbar(KitScrollbar scrollBar);
    }
}
