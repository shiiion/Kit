using System.Windows.Input;
using Kit.Graphics.Types;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;

namespace Kit.Core
{
    public struct Highlight
    {
        public bool Enabled;
        public int Start;
        public int End;

        public int GetLastIndex()
        {
            if (Start == -1 || End == -1)
            {
                return -1;
            }
            return System.Math.Max(Start, End);
        }

        public int GetFirstIndex()
        {
            if (Start == -1)
            {
                return End;
            }
            if (End == -1)
            {
                return Start;
            }
            return System.Math.Min(Start, End);
        }
    }

    public class TextIOFormatter
    {
        public int CursorLoc { get; set; }

        public bool ShiftDown { get; set; }

        public bool CtrlDown { get; set; }

        public readonly int CURSOR_END = -1;

        public KitText FormatComponent { get; set; }

        private Highlight textHighlight;

        public TextIOFormatter(KitText component)
        {
            ShiftDown = false;
            CtrlDown = false;
            FormatComponent = component;
            textHighlight = new Highlight();
            EndHighlight();
        }

        private bool moveCursor(Key direction, string traverseString)
        {
            if (traverseString.Length == 0)
            {
                if (direction == Key.Left || direction == Key.Right)
                {
                    return true;
                }
                return false;
            }
            switch (direction)
            {
                case Key.Left:
                    {
                        if (CursorLoc == CURSOR_END)
                        {
                            CursorLoc = traverseString.Length - 1;
                        }
                        else if (CursorLoc != 0)
                        {
                            CursorLoc--;
                        }
                    }
                    break;
                case Key.Right:
                    {
                        if (CursorLoc == traverseString.Length - 1)
                        {
                            CursorLoc = CURSOR_END;
                        }
                        else if (CursorLoc != CURSOR_END)
                        {
                            CursorLoc++;
                        }
                    }
                    break;
                default:
                    return false;
            }
            if (!ShiftDown && textHighlight.Enabled)
            {
                if (direction == Key.Left)
                {
                    CursorLoc = textHighlight.GetFirstIndex();
                }
                else if (direction == Key.Right)
                {
                    CursorLoc = textHighlight.GetLastIndex();
                }
                EndHighlight();
            }
            if (textHighlight.Enabled)
            {
                textHighlight.End = CursorLoc;
            }
            return true;
        }

        private void relocateCursor()
        {
            if (CursorLoc != CURSOR_END)
            {
                CursorLoc++;
            }
        }

        private void removeHighlight()
        {
            int max = textHighlight.GetLastIndex();
            int min = textHighlight.GetFirstIndex();

            if (max == CURSOR_END)
            {
                max = FormatComponent.Text.Length;
            }

            int length = max - min;

            FormatComponent.Text = FormatComponent.Text.Remove(min, length);

            if (CursorLoc >= max)
            {
                CursorLoc = min;
            }

            if (CursorLoc >= FormatComponent.Text.Length)
            {
                CursorLoc = CURSOR_END;
            }
        }

        private void deletePrevChar()
        {
            if (Highlighting())
            {
                removeHighlight();
                return;
            }
            if (CursorLoc == CURSOR_END)
            {
                FormatComponent.Text = FormatComponent.Text.Remove(FormatComponent.Text.Length - 1, 1);
            }
            else if (CursorLoc != 0)
            {
                FormatComponent.Text = FormatComponent.Text.Remove(CursorLoc - 1, 1);
                CursorLoc--;
            }
        }

        private void deleteNextChar()
        {
            if (Highlighting())
            {
                removeHighlight();
                return;
            }
            if (CursorLoc != CURSOR_END)
            {
                FormatComponent.Text = FormatComponent.Text.Remove(CursorLoc, 1);
                if (CursorLoc == FormatComponent.Text.Length)
                {
                    CursorLoc = CURSOR_END;
                }
            }
        }

        private double getTextOffset(int loc)
        {
            string sub;
            if (loc == 0)
            {
                return 0;
            }
            else if (loc == CURSOR_END)
            {
                sub = FormatComponent.Text;
            }
            else
            {
                sub = FormatComponent.Text.Substring(0, loc);
            }
            Vector2 bounds = Graphics.Drawing.KitBrush.GetTextBounds(sub, FormatComponent.Font);

            return bounds.X;
        }

        private int getIndexAtLocation(Vector2 location)
        {
            int ret;
            if (location.X <= 0)
            {
                ret = 0;
            }
            else if (location.X >= FormatComponent.Size.X)
            {
                ret = CURSOR_END;
            }
            else
            {
                int i = 0;
                for (; i < FormatComponent.Text.Length; i++)
                {
                    Vector2 textDims = KitBrush.GetTextBounds(FormatComponent.Text.Substring(0, i), FormatComponent.Font);
                    Vector2 nextTextDims = KitBrush.GetTextBounds(FormatComponent.Text.Substring(0, i + 1), FormatComponent.Font);
                    if (location.X >= textDims.X && location.X <= nextTextDims.X)
                    {
                        if (location.X - textDims.X > nextTextDims.X - location.X)
                        {
                            i++;
                        }
                        break;
                    }
                }
                if (i == FormatComponent.Text.Length)
                {
                    i = CURSOR_END;
                }
                ret = i;
            }
            return ret;
        }

        public void InsertText(string text)
        {
            if (Highlighting())
            {
                removeHighlight();
            }
            EndHighlight();
            if (CursorLoc == CURSOR_END)
            {
                FormatComponent.Text += text;
            }
            else
            {
                FormatComponent.Text = FormatComponent.Text.Insert(CursorLoc, text);
            }

            relocateCursor();
        }

        public void BeginHighlight()
        {
            textHighlight.Start = CursorLoc;
            textHighlight.End = CursorLoc;
            textHighlight.Enabled = true;
        }

        public void EndHighlight()
        {
            textHighlight.Start = 0;
            textHighlight.End = 0;
            textHighlight.Enabled = false;
        }

        public bool HandleKeyPress(Key key)
        {
            bool shouldRedraw = moveCursor(key, FormatComponent.Text);

            if (key == Key.Back)
            {
                if (FormatComponent.Text.Length > 0)
                {
                    deletePrevChar();
                    if (FormatComponent.Text.Length == 0)
                    {
                        CursorLoc = CURSOR_END;
                    }
                }
                EndHighlight();
                return true;
            }
            else if (key == Key.Delete)
            {
                if (FormatComponent.Text.Length > 0)
                {
                    deleteNextChar();
                    if (FormatComponent.Text.Length == 0)
                    {
                        CursorLoc = CURSOR_END;
                    }
                }
                EndHighlight();
                return true;
            }
            else if (key == Key.A)
            {
                if (CtrlDown)
                {
                    BeginHighlight();
                    textHighlight.Start = 0;
                    textHighlight.End = CURSOR_END;
                    CursorLoc = CURSOR_END;
                    return true;
                }
            }

            return shouldRedraw;
        }

        public bool HighlightEnabled()
        {
            return textHighlight.Enabled;
        }

        public bool Highlighting()
        {
            return textHighlight.Enabled && (textHighlight.End != textHighlight.Start);
        }

        public Box GetHighlightRect()
        {
            Box ret = new Box();

            double max = getTextOffset(textHighlight.GetLastIndex());
            double min = getTextOffset(textHighlight.GetFirstIndex());

            ret.Size = new Vector2(max - min, FormatComponent.Size.Y);
            ret.Pos = new Vector2(min, FormatComponent.Location.Y);

            return ret;
        }

        public void OnKey(Key key, KeyState state)
        {
            if (state == KeyState.Press || state == KeyState.Hold)
            {
                if ((key == Key.LeftShift || key == Key.RightShift) && !Highlighting())
                {
                    ShiftDown = true;
                    BeginHighlight();
                }
                else if ((key == Key.LeftShift || key == Key.RightShift) && Highlighting())
                {
                    ShiftDown = true;
                }
                else if (key == Key.LeftCtrl || key == Key.RightCtrl)
                {
                    CtrlDown = true;
                }
            }
            else
            {
                if (key == Key.LeftShift || key == Key.RightShift)
                {
                    ShiftDown = false;
                }
                else if (key == Key.LeftCtrl || key == Key.RightCtrl)
                {
                    CtrlDown = false;
                }
            }
        }

        public double GetCursorOffset()
        {
            return getTextOffset(CursorLoc);
        }

        public bool InsertHighlightEndAt(Vector2 relativeLocation)
        {
            if(textHighlight.Enabled)
            {
                int end = textHighlight.End;
                textHighlight.End = getIndexAtLocation(relativeLocation);
                CursorLoc = textHighlight.End;
                return end != textHighlight.End;
            }
            return false;
        }

        public void InsertCursorAt(Vector2 relativeLocation)
        {
            CursorLoc = getIndexAtLocation(relativeLocation);
        }
    }
}
