using System.Windows.Input;
using System.Collections;
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

        public string GetHighlightedText(string text)
        {
            int begin = GetFirstIndex();
            int end = GetLastIndex();
            if(begin == -1)
            {
                return "";
            }
            if(end == -1)
            {
                end = text.Length;
            }
            return text.Substring(begin, end - begin);
        }
    }

    public struct FormatterState
    {
        public FormatterState(string text, int loc)
        {
            CurText = text;
            CursorLoc = loc;
        }

        public string CurText;
        public int CursorLoc;
    }

    public class TextIOFormatter
    {
        public int CursorLoc { get; set; }

        public bool ShiftDown { get; set; }

        public bool CtrlDown { get; set; }

        public readonly int CURSOR_END = -1;

        public KitText FormatComponent { get; set; }

        protected Highlight textHighlight;

        protected Stack undoStack;
        protected Stack redoStack;

        public TextIOFormatter(KitText component)
        {
            ShiftDown = false;
            CtrlDown = false;
            FormatComponent = component;
            textHighlight = new Highlight();
            EndHighlight();
            undoStack = new Stack();
            redoStack = new Stack();
            undoStack.Push(new FormatterState("", 0));
        }

        protected virtual bool moveCursor(Key direction, string traverseString)
        {
            if (traverseString.Length == 0)
            {
                return direction == Key.Left || direction == Key.Right;
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
            else if (textHighlight.Enabled)
            {
                textHighlight.End = CursorLoc;
            }
            return true;
        }

        private void relocateCursor(int len)
        {
            if (CursorLoc != CURSOR_END)
            {
                CursorLoc += len;
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

        protected virtual void deletePrevChar()
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

        protected virtual void deleteNextChar()
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

        protected virtual double getTextOffset(string text, int loc)
        {
            string sub;
            if (loc == CURSOR_END)
            {
                sub = text;
            }
            else
            {
                sub = text.Substring(0, loc);
            }
            Vector2 bounds = KitBrush.GetTextBounds(sub, FormatComponent.Font);

            return bounds.X;
        }

        protected virtual int getIndexAtLocation(string text, Vector2 location)
        {
            int ret;
            Vector2 fullDims = KitBrush.GetTextBounds(text, FormatComponent.Font);

            if (location.X >= fullDims.X)
            {
                ret = CURSOR_END;
            }
            else if (location.X <= 0)
            {
                ret = 0;
            }
            else
            {
                int i = 0;
                for (; i < text.Length; i++)
                {
                    Vector2 textDims = KitBrush.GetTextBounds(text.Substring(0, i), FormatComponent.Font);
                    Vector2 nextTextDims = KitBrush.GetTextBounds(text.Substring(0, i + 1), FormatComponent.Font);
                    if (location.X >= textDims.X && location.X <= nextTextDims.X)
                    {
                        if (location.X - textDims.X > nextTextDims.X - location.X)
                        {
                            i++;
                        }
                        break;
                    }
                }
                ret = i;
            }
            return ret;
        }

        protected virtual bool shouldPushUndo(Key keyPress)
        {
            return (keyPress == Key.Delete && CursorLoc != CURSOR_END) ||
                (keyPress == Key.Back && FormatComponent.Text.Length != 0 && CursorLoc != 0) ||
                ((keyPress == Key.Delete || keyPress == Key.Back) && Highlighting()) ||
                (undoStack.Count == 0) ||
                (CtrlDown && keyPress == Key.V);
        }

        protected virtual bool OnCtrlKey(Key key)
        {
            if (key == Key.A)
            {
                BeginHighlight();
                textHighlight.Start = 0;
                textHighlight.End = CURSOR_END;
                CursorLoc = CURSOR_END;
                return true;
            }
            else if (key == Key.Z)
            {
                EndHighlight();
                Undo();
                return true;
            }
            else if (key == Key.Y)
            {
                EndHighlight();
                Redo();
                return true;
            }
            else if (key == Key.V)
            {
                if (System.Windows.Forms.Clipboard.ContainsText())
                {
                    InsertText(System.Windows.Forms.Clipboard.GetText());
                }
                return true;
            }
            else if (key == Key.C)
            {
                if (Highlighting())
                {
                    System.Windows.Forms.Clipboard.SetText(textHighlight.GetHighlightedText(FormatComponent.Text));
                }
            }
            return false;
        }

        private void pushUndo(Key curKey)
        {
            if (shouldPushUndo(curKey))
            {
                undoStack.Push(new FormatterState(FormatComponent.Text, CursorLoc));
                redoStack.Clear();
            }
        }

        public virtual void InsertText(string text)
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

            relocateCursor(text.Length);
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
            pushUndo(key);
            if (CtrlDown)
            {
                return OnCtrlKey(key);
            }
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

        public Box GetHighlightRect(string text)
        {
            Box ret = new Box();

            double max = getTextOffset(text, textHighlight.GetLastIndex());
            double min = getTextOffset(text, textHighlight.GetFirstIndex());

            ret.Size = new Vector2(max - min, FormatComponent.Size.Y);
            ret.Pos = new Vector2(min);

            return ret;
        }

        public Vector2 GetVisibleOffset(string text, double visibleWidth, int loc)
        {
            Vector2 ret = new Vector2(0, 0);
            double endOffset = getTextOffset(text, CURSOR_END);
            if (endOffset < visibleWidth)
            {
                return ret;
            }

            double cursorOffset = getTextOffset(text, loc);


            if ((endOffset - cursorOffset) < (visibleWidth / 2))
            {
                ret.X = endOffset - visibleWidth + 2;
            }
            else if (cursorOffset > (visibleWidth / 2))
            {
                ret.X = cursorOffset - (visibleWidth / 2);
            }
            ret.X = -ret.X;
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
            return getTextOffset(FormatComponent.Text, CursorLoc);
        }

        public bool InsertHighlightEndAt(Vector2 relativeLocation)
        {
            if (textHighlight.Enabled)
            {
                int end = textHighlight.End;
                textHighlight.End = getIndexAtLocation(FormatComponent.Text, relativeLocation);
                CursorLoc = textHighlight.End;
                return end != textHighlight.End;
            }
            return false;
        }

        public void InsertCursorAt(Vector2 relativeLocation)
        {
            CursorLoc = getIndexAtLocation(FormatComponent.Text, relativeLocation);
        }

        public virtual void Undo()
        {
            if (undoStack.Count > 0)
            {
                FormatterState top = (FormatterState)undoStack.Pop();
                redoStack.Push(new FormatterState(FormatComponent.Text, CursorLoc));
                CursorLoc = top.CursorLoc;
                FormatComponent.Text = top.CurText;
            }
        }

        public virtual void Redo()
        {
            if (redoStack.Count > 0)
            {
                FormatterState top = (FormatterState)redoStack.Pop();
                undoStack.Push(new FormatterState(FormatComponent.Text, CursorLoc));
                CursorLoc = top.CursorLoc;
                FormatComponent.Text = top.CurText;
            }
        }
    }
}
