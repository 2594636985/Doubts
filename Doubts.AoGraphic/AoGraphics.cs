using Doubts.AoGraphic.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Doubts.AoGraphic
{
    public class AoGraphics : IGraphics
    {
        private static readonly StringFormat stringFormat;
        private static readonly StringFormat stringFormat2;
        private static readonly CharacterRange[] characterRanges = new CharacterRange[1];

        private readonly Graphics graphics;
        private bool setRtl;

        public SmoothingMode SmoothingMode
        {
            get
            {
                return graphics.SmoothingMode;
            }

            set
            {
                graphics.SmoothingMode = value;
            }
        }


        static AoGraphics()
        {
            stringFormat = new StringFormat(StringFormat.GenericTypographic);
            stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces;

            stringFormat2 = new StringFormat(StringFormat.GenericTypographic);
        }

        public AoGraphics(Graphics graphics)
        {
            this.graphics = graphics;
            this.setRtl = false;
        }


        public void Dispose()
        {
            if (this.setRtl)
                stringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
        }

        public void DrawImage(Image image, RectangleF destRect)
        {
            graphics.DrawImage(image, destRect);
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect)
        {
            graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            graphics.DrawPath(pen, path);
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            graphics.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawString(string str, Font font, Color color, PointF point, SizeF size, bool rtl)
        {
            SetRtlAlign(rtl);
            graphics.DrawString(str, font, RenderUtils.GetSolidBrush(color), (int)Math.Round(point.X) + (rtl ? size.Width : 0), (int)Math.Round(point.Y), stringFormat2);
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
            graphics.FillPath(brush, path);
        }

        public void FillPolygon(Brush brush, PointF[] points)
        {
            graphics.FillPolygon(brush, points);
        }

        public void FillRectangle(Brush getSolidBrush, float left, float top, float width, float height)
        {
            graphics.FillRectangle(getSolidBrush, left, top, width, height);
        }

        public RectangleF GetClip()
        {
            return graphics.ClipBounds;
        }

        public Size MeasureString(string str, Font font)
        {
            characterRanges[0] = new CharacterRange(0, str.Length);
            stringFormat.SetMeasurableCharacterRanges(characterRanges);
            var size = graphics.MeasureCharacterRanges(str, font, RectangleF.Empty, stringFormat)[0].GetBounds(graphics).Size;
            return new Size((int)Math.Round(size.Width), (int)Math.Round(size.Height));
        }

        public Size MeasureString(string str, Font font, float maxWidth, out int charFit, out int charFitWidth)
        {
            charFit = 0;
            charFitWidth = 0;

            var size = MeasureString(str, font);

            for (int i = 1; i <= str.Length; i++)
            {
                charFit = i - 1;
                Size pSize = MeasureString(str.Substring(0, i), font);
                if (pSize.Height <= size.Height && pSize.Width < maxWidth)
                    charFitWidth = pSize.Width;
                else
                    break;
            }

            return size;
        }

        public void SetClip(RectangleF rect, CombineMode combineMode = CombineMode.Replace)
        {
            graphics.SetClip(rect, combineMode);
        }


        private void SetRtlAlign(bool rtl)
        {
            if (setRtl)
            {
                if (!rtl)
                {
                    stringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
                }
            }
            else if (rtl)
            {
                stringFormat2.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            }
            setRtl = rtl;
        }
    }
}
