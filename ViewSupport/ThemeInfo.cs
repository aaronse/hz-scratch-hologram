using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ViewSupport
{
    public class ThemeInfo
    {
        static ThemeInfo()
        {
        }

        public static ThemeInfo LightTheme = new ThemeInfo()
        {
            WindowBackGroundColor = System.Drawing.SystemColors.Control,
            BackgroundColor = Color.White,
            ArcPen = new Pen(Color.Gray),
            ArcPenHighlight = new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.DarkGray,
            VectorPen = new Pen(Color.Blue),
            PointBrush = Brushes.Blue,
            RedBlueModePointBrush = Brushes.Black,  // Can't be Blue
        };

        public static ThemeInfo DarkTheme = new ThemeInfo()
        {
            WindowBackGroundColor = Color.FromArgb(96, 96, 96), // Color.FromArgb(45, 45, 48),
            BackgroundColor = Color.FromArgb(30, 30, 30),
            ArcPen = new Pen(Color.DarkGray),
            ArcPenHighlight = new Pen(Color.FromArgb(96, 128, 255)),  // new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.LightGray,
            VectorPen = new Pen(Color.LightBlue),
            PointBrush = Brushes.LightGray,
            RedBlueModePointBrush = Brushes.White
        };

        public static ThemeInfo Current = DarkTheme;

        public Color WindowBackGroundColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Pen ArcPen { get; set; }
        public Pen ArcPenHighlight { get; set; }
        public Pen VectorPen { get; set; }

        public Brush PointBrush { get; set; }

        public Brush RedBlueModePointBrush { get; set; }

        public Brush ArcTextBrush { get; set; }
    }
}
