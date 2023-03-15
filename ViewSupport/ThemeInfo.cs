using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ViewSupport
{
    public class ThemeInfo
    {
        public static ThemeInfo LightTheme = new ThemeInfo(new ThemeInfo()
        {
            WindowBackColor = System.Drawing.SystemColors.Control,
            TextColor = Color.FromArgb(0, 0, 0),
            BackgroundColor = Color.FromArgb(0, 255, 255, 255),
            BackgroundColor2 = Color.White,
            BorderColor = System.Drawing.SystemColors.InactiveBorder,
            ArcPen = new Pen(Color.Gray),
            ArcPenHighlight = new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.DarkGray,
            VectorPen = new Pen(Color.Blue),
            ExportVectorColor = "Black",
            PointBrush = Brushes.Blue,
            RedBlueModePointBrush = Brushes.Black,  // Can't be Blue
            SelectedColor = Color.Red,
            SelectedPen = new Pen(Color.FromArgb(242, 45, 20), 2.5f),
            SelectedPen2 = new Pen(Color.FromArgb(32, 242, 45, 20), 3f)
        });

        public static ThemeInfo DarkTheme = new ThemeInfo(new ThemeInfo()
        {
            WindowBackColor = Color.FromArgb(45, 45, 48), //Color.FromArgb(96, 96, 96), // Goal is Color.FromArgb(45, 45, 48) after UI fully updated
            TextColor = Color.FromArgb(241, 241, 241),
            BackgroundColor = Color.FromArgb(0, 30, 30, 30),
            BackgroundColor2 = Color.FromArgb(255, 30, 30, 30),
            BorderColor = Color.FromArgb(67, 67, 70),
            ArcPen = new Pen(Color.DarkGray),
            ArcPenHighlight = new Pen(Color.FromArgb(96, 128, 255)),  // new Pen(Color.LimeGreen, 1f),
            ArcTextBrush = Brushes.LightGray,
            VectorPen = new Pen(Color.LightBlue),
            ExportVectorColor = "Black",
            PointBrush = Brushes.LightGray,
            RedBlueModePointBrush = Brushes.White,
            SelectedColor = Color.MediumPurple,
            SelectedPen = new Pen(Color.FromArgb(242, 45, 20), 2.5f),
            SelectedPen2 = new Pen(Color.FromArgb(32, 242, 45, 20), 3f)
        });

        public static ThemeInfo Current = DarkTheme;

        public Color WindowBackColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public Color BorderColor { get; set; }
        public Pen ArcPen { get; set; }
        public Pen ArcPenHighlight { get; set; }
        public Pen VectorPen { get; set; }
        public string ExportVectorColor { get; set; }
        public Brush PointBrush { get; set; }
        public Brush RedBlueModePointBrush { get; set; }
        public Brush ArcTextBrush { get; set; }
        public Color SelectedColor { get; set; }
        public Pen SelectedPen { get; set; }
        public Pen SelectedPen2 { get; set; }

        public Pen[] SelectedPens { get; set; }

        ThemeInfo()
        {
        }

        ThemeInfo(ThemeInfo template)
        {
            this.WindowBackColor = template.WindowBackColor;
            this.TextColor = template.TextColor;
            this.BackgroundColor = template.BackgroundColor;
            this.BackgroundColor2 = template.BackgroundColor2;
            this.BorderColor = template.BorderColor;
            this.ArcPen = template.ArcPen;
            this.ArcPenHighlight = template.ArcPenHighlight;
            this.ArcTextBrush = template.ArcTextBrush;
            this.VectorPen = template.VectorPen;
            this.ExportVectorColor = template.ExportVectorColor;
            this.PointBrush = template.PointBrush;
            this.RedBlueModePointBrush = template.RedBlueModePointBrush;
            this.SelectedColor = template.SelectedColor;
            this.SelectedPen = template.SelectedPen;
            this.SelectedPen2 = template.SelectedPen2;

            SelectedPens = new Pen[256];

            int a = this.SelectedPen.Color.A;
            int r = this.SelectedPen.Color.R;
            int g = this.SelectedPen.Color.G;
            int b = this.SelectedPen.Color.B;
            float width = this.SelectedPen.Width;

            for (int i = 0; i < 256; i++)
            {
                a = i;
                SelectedPens[i] = new Pen(Color.FromArgb(a, r, b, g), width);
            }
        }
    }
}
