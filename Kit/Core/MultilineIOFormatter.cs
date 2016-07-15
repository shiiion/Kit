using System.Windows.Input;
using Kit.Graphics.Types;
using Kit.Graphics.Components;
using Kit.Graphics.Drawing;

namespace Kit.Core
{
    class MultilineIOFormatter : TextIOFormatter
    {
        private string[] formatterLines;

        public MultilineIOFormatter(KitText component)
            : base(component)
        {
            formatterLines = component.Text.Split('\n');
        }

        private int getLine(string[] lines, int loc)
        {
            if (loc == CURSOR_END)
            {
                return lines.Length - 1;
            }
            
            int charIndex = 0;
            int lineIndex = 0;
            while (charIndex <= loc)
            {
                charIndex += lines[lineIndex].Length + 1;
                lineIndex++;
            }
            return lineIndex - 1;
        }

        private int getCol(string[] lines, int line, int loc)
        {
            if (loc == CURSOR_END)
            {
                return lines[line].Length;
            }
            int charIndex = 0;

            for (int a = 0; a < line; a++)
            {
                charIndex += lines[a].Length + 1;
            }

            return loc - charIndex;
        }

        private int indexFromRowCol(string[] lines, int row, int col)
        {
            int ret = 0;
            int charCount = 0;
            for (int a = 0; a < lines.Length; a++)
            {
                if (a < row)
                {
                    ret += lines[a].Length + 1;
                }
                charCount += lines[a].Length + 1;
            }
            charCount--;
            if (lines[row].Length < col)
            {
                ret += lines[row].Length;
            }
            else
            {
                ret += col;
            }
            if (ret >= charCount)
            {
                ret = CURSOR_END;
            }
            return ret;
        }

        private void nextLine(string traverseString)
        {
            
            int line = getLine(formatterLines, CursorLoc);
            int col = getCol(formatterLines, line, CursorLoc);

            if (line == formatterLines.Length - 1)
            {
                CursorLoc = CURSOR_END;
            }
            else
            {
                line++;
                CursorLoc = indexFromRowCol(formatterLines, line, col);
            }
        }

        private void previousLine(string traverseString)
        {
            int line = getLine(formatterLines, CursorLoc);
            int col = getCol(formatterLines, line, CursorLoc);

            if (line == 0)
            {
                CursorLoc = 0;
            }
            else
            {
                line--;
                CursorLoc = indexFromRowCol(formatterLines, line, col);
            }
        }

        protected override bool moveCursor(Key direction, string traverseString)
        {
            if (base.moveCursor(direction, traverseString))
            {
                return true;
            }
            switch (direction)
            {
                case Key.Up:
                    previousLine(traverseString);
                    break;
                case Key.Down:
                    nextLine(traverseString);
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

        protected override void deleteNextChar()
        {
            base.deleteNextChar();
            formatterLines = FormatComponent.Text.Split('\n');
        }

        protected override void deletePrevChar()
        {
            base.deletePrevChar();
            formatterLines = FormatComponent.Text.Split('\n');
        }

        protected override bool shouldPushUndo(Key keyPress)
        {
            return keyPress == Key.Enter | base.shouldPushUndo(keyPress);
        }

        protected override int getIndexAtLocation(string text, Vector2 location)
        {
            int row, col;

            if(formatterLines.Length == 0)
            {
                return CURSOR_END;
            }

            {
                double yLoc = 0;
                int botLine = 0;
                double lineHeight = KitBrush.GetTextBounds("|", FormatComponent.Font).Y;
                while (yLoc < location.Y)
                {
                    yLoc += lineHeight;
                    botLine++;
                }
                row = botLine - 1;
                if(row < 0)
                {
                    row = 0;
                }
                else if(row >= formatterLines.Length)
                {
                    row = formatterLines.Length - 1;
                }
            }
            string line = formatterLines[row];
            col = base.getIndexAtLocation(line, location);

            if(col == CURSOR_END)
            {
                col = line.Length;
            }
            
            return indexFromRowCol(formatterLines, row, col);
        }

        public Box[] GetHighlightRects(string text)
        {

            int firstRow = getLine(formatterLines, textHighlight.GetFirstIndex());
            int lastRow = getLine(formatterLines, textHighlight.GetLastIndex());

            Box[] ret = new Box[lastRow - firstRow + 1];

            Vector2 spaceSize = KitBrush.GetTextBounds(" ", FormatComponent.Font);
            double textHeight = spaceSize.Y;
            

            int firstCol = getCol(formatterLines, firstRow, textHighlight.GetFirstIndex());
            int lastCol = getCol(formatterLines, lastRow, textHighlight.GetLastIndex());

            if (firstRow == lastRow)
            {
                double max = getTextOffset(formatterLines[firstRow], lastCol);
                double min = getTextOffset(formatterLines[firstRow], firstCol);
                ret[0] = new Box(min, textHeight * firstRow, max - min, textHeight);
            }
            else
            {
                double min = getTextOffset(formatterLines[firstRow], firstCol);
                double max = getTextOffset(formatterLines[firstRow], CURSOR_END) + spaceSize.X;
                ret[0] = new Box(min, textHeight * firstRow, max - min, textHeight);

                for (int a = firstRow + 1; a < lastRow; a++)
                {
                    ret[a - firstRow] = new Box(0, textHeight * a, getTextOffset(formatterLines[a], CURSOR_END) + spaceSize.X, textHeight);
                }

                max = getTextOffset(formatterLines[lastRow], lastCol);
                ret[lastRow - firstRow] = new Box(0, textHeight * lastRow, max, textHeight);

            }
            return ret;
        }

        new public Vector2 GetCursorOffset()
        {
            if(formatterLines.Length == 0)
            {
                return new Vector2();
            }
            double lineHeight = KitBrush.GetTextBounds("|", FormatComponent.Font).Y;

            int line = getLine(formatterLines, CursorLoc);
            int col = getCol(formatterLines, line, CursorLoc);

            return new Vector2(getTextOffset(formatterLines[line], col), lineHeight * line);
        }

        private double GetVerticalOffset(double visibleHeight, int loc)
        {
            double ret = 0;
            double textHeight = KitBrush.GetTextBounds(" ", FormatComponent.Font).Y;

            double endOffset = textHeight * formatterLines.Length;
            if(endOffset < visibleHeight)
            {
                return 0;
            }

            int line = getLine(formatterLines, loc);

            double cursorOffset = textHeight * line;

            if ((endOffset - cursorOffset) < (visibleHeight / 2))
            {
                ret = endOffset - visibleHeight + 2;
            }
            else if (cursorOffset > (visibleHeight / 2))
            {
                ret = cursorOffset - (visibleHeight / 2);
            }
            ret = -ret;
            return ret;
        }

        public Vector2 GetVisibleOffset(double visibleWidth, double visibleHeight, int loc)
        {

            int line = getLine(formatterLines, loc);
            int col = getCol(formatterLines, line, loc);

            Vector2 horizontalVector = GetVisibleOffset(formatterLines[line], visibleWidth, col);
            horizontalVector.Y = GetVerticalOffset(visibleHeight, loc);
            return horizontalVector;
        }

        public override void InsertText(string text)
        {
            base.InsertText(text);
            formatterLines = FormatComponent.Text.Split('\n');
        }

        public override void Undo()
        {
            base.Undo();
            formatterLines = FormatComponent.Text.Split('\n');
        }

        public override void Redo()
        {
            base.Redo();
            formatterLines = FormatComponent.Text.Split('\n');
        }
    }
}
