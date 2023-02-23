using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ViewSupport
{
    public class ThemeInfo
    {
        public static ThemeInfo LightTheme = new ThemeInfo()
        {
            BackgroundColor = Color.White,
            ArcPen = new Pen(Color.Gray),
            VectorPen = new Pen(Color.Blue),
            PointBrush = Brushes.Blue,
            RedBlueModePointBrush = Brushes.Black,  // Can't be Blue
    };

        public static ThemeInfo DarkTheme = new ThemeInfo()
        {
            BackgroundColor = Color.FromArgb(30, 30, 30),
            ArcPen = new Pen(Color.Gray),
            VectorPen = new Pen(Color.LightBlue),
            PointBrush = Brushes.LightGray,
            RedBlueModePointBrush = Brushes.White
        };

        public Color BackgroundColor { get; set; }
        public Pen ArcPen { get; set; }
        public Pen VectorPen { get; set; }

        public Brush PointBrush { get; set; }

        public Brush RedBlueModePointBrush { get; set; }
    }
}
