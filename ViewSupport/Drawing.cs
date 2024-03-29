﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Primitives;
using System.Diagnostics;
using ScratchUtility;

namespace ViewSupport
{
    public delegate void SceneChangedHandler();

    // TODO:P2 Gratuitous Glow, options:
    // - A) Draw to Small bitmap then blit fullsize relying on bicubic interpolation for blur effect https://stackoverflow.com/questions/74232522/outer-glow-effect-for-text-c-sharp

    public static class Drawing
    {
        /// <summary>Specifies whether or not this class should respond to events that would cause 
        /// it to be redrawn. True (should not respond to events) if a drawing operation (onto the
        /// off-screen buffer) is already occuring.</summary>
        public static bool CurrentlyDrawing { get; set; }
        private static bool mDoDraw = false;
        public static RedrawTypeRequired NextRedraw { get; set; }
        internal static ShapeList Shapes { get; private set; }
        internal static Brush PointBrush_BehindCanvas { get; set; }
        internal static Brush PointBrush_InFrontOfCanvas { get; set; }
        public static Point NullPoint { get { return new Point(int.MaxValue, int.MaxValue); } }
        private static Bitmap s_offScreenBitmap;
        private static Bitmap s_offScreenBitmap2;
        private static Graphics s_offScreenGraphics;
        private static Graphics s_offScreenGraphics2;

        public static event SceneChangedHandler SceneChanged;

        static Drawing()
        {
            Shapes = new ShapeList();

            //todo: properly dispose of these at end of program
            
            PointBrush_BehindCanvas = Brushes.DarkViolet;
            PointBrush_InFrontOfCanvas = Brushes.Blue;
            
            s_offScreenBitmap = new Bitmap(10, 10);
            s_offScreenBitmap2 = new Bitmap(10, 10);
            s_offScreenGraphics = Graphics.FromImage(s_offScreenBitmap);
            s_offScreenGraphics2 = Graphics.FromImage(s_offScreenBitmap2);

            ViewContext.ViewChanged += new ViewChangedHandler(ViewContext_ViewChanged);
            DrawOptions.DrawOptionChanged += new DrawOptionChangedHandler(DrawOptions_DrawOptionChanged);
            CurrentlyDrawing = true;
        }

        private static void ViewContext_ViewChanged(RedrawRequiredEventArgs e)
        {
            RespondToRedrawRequired(e.RedrawTypeRequired);
        }
        private static void DrawOptions_DrawOptionChanged(RedrawRequiredEventArgs e)
        {
            RespondToRedrawRequired(e.RedrawTypeRequired);
        }

        private static void RespondToRedrawRequired(RedrawTypeRequired type)
        {
            NextRedraw = type;
            if (!CurrentlyDrawing)
                FireSceneChangedEvent();
        }

        ///<summary>Marks the offscreen buffer as dirty and fires the ScreenChanged event causing it to be redrawn by the host.</summary>
        public static void MarkAsDirty(RedrawTypeRequired type)
        {
            NextRedraw = type;
            FireSceneChangedEvent();
        }

        #region Properties


        /// <summary>Specifies whether or not the screen should be drawn to.</summary>
        public static bool DoDraw
        {
            get
            {
                return mDoDraw;
            }
            set
            {
                if (mDoDraw == value)
                    return;
                mDoDraw = value;
                FireSceneChangedEvent();
            }
        }

        public static Size CanvasSize
        {
            get { return ViewContext.CanvasSize; }
            set
            {
                if (ViewContext.CanvasSize == value)
                    return;
                ViewContext.CanvasSize = value;
                s_offScreenBitmap = new Bitmap(CanvasSize.Width, CanvasSize.Height);
                s_offScreenBitmap2 = new Bitmap(CanvasSize.Width / 5, CanvasSize.Height / 5);
                s_offScreenGraphics = Graphics.FromImage(s_offScreenBitmap);
                s_offScreenGraphics2 = Graphics.FromImage(s_offScreenBitmap2);

                s_offScreenGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                s_offScreenGraphics2.SmoothingMode = SmoothingMode.AntiAlias;
            }
        }

#endregion


        /// <summary>Redraws the current ViewPrimitives list onto the off screen buffer. Calling 
        /// Blit() will blit it to the supplied graphics object. Returns true if the redraw 
        /// occurred successfully.</summary>
        public static bool ReDraw(
            bool isRendering,
            bool isRendering2,
            bool isAnimating)
        {
            DrawOptions drawOptions = new DrawOptions()
            {
                IsRendering = isRendering,
                IsRendering2 = isRendering2,
                Graphics = s_offScreenGraphics,
                Graphics2 = s_offScreenGraphics2
            };

            if (NextRedraw != RedrawTypeRequired.None || isAnimating)
            {
                CurrentlyDrawing = true;
                if (NextRedraw == RedrawTypeRequired.RecalculateViewPrimitives)
                {
                    Shapes.Refresh(DrawOptions.SwitchBackFront, false);
                }
                else if (NextRedraw == RedrawTypeRequired.RecalculateAllArcs)
                {
                    Shapes.Refresh(DrawOptions.SwitchBackFront, true);
                }
                else if (NextRedraw == RedrawTypeRequired.RecalculateArcPositionsOnly)
                {
                    Shapes.Refresh(DrawOptions.SwitchBackFront, true);
                }

                switch (DrawOptions.ViewMode)
                {
                    case ViewMode.Normal:
                        drawOptions.Theme = ThemeInfo.LightTheme;
                        DrawNormal(drawOptions, true);
                        break;
                    case ViewMode.Dark:
                        drawOptions.Theme = ThemeInfo.DarkTheme;
                        DrawNormal(drawOptions, true);
                        break;
                    case ViewMode.RedBlue:
                        drawOptions.Theme = ThemeInfo.LightTheme;
                        DrawRedBlue(drawOptions);
                        break;
                    case ViewMode.Stereoscopic:
                        drawOptions.Theme = ThemeInfo.LightTheme;
                        s_offScreenGraphics.Clear(Color.White);
                        s_offScreenGraphics2.Clear(Color.White);
                        DrawStereoscopic(drawOptions);
                        break;
                    case ViewMode.Print:
                        drawOptions.Theme = ThemeInfo.LightTheme;
                        DrawPrint(drawOptions);
                        break;
                }

                NextRedraw = RedrawTypeRequired.None;
                CurrentlyDrawing = false;


                FireSceneChangedEvent();
            }

            return true;
        }

        private static void FireSceneChangedEvent()
        {
            if (SceneChanged != null && DoDraw)
                SceneChanged();
        }

        private static void DrawNormal(DrawOptions options, bool clearGraphics)
        {
            if (clearGraphics)
            {
                options.Graphics.Clear(options.Theme.BackgroundColor);
                options.Graphics2.Clear(options.Theme.BackgroundColor2);
            }

            if (Shapes.Count > 0)
            {
                EdgePainter.ShapeList = Shapes;
                EdgePainter.Draw(options);
            }


            //if (DrawOptions.ShowPoints)
            //{
            //    if (DrawOptions.CanvasCutoffMode != CanvasCutoffMode.ToggleColor)
            //    {
            //        ViewPrimitives.Draw(g);
            //    }
            //    else
            //    {
            //        //if we're supposed to be drawing different colors for in front vs. behind, we need to draw the behind points first, change the color, then draw the in front points
            //        Brush backupBrush = PointBrush;

            //        PointBrush = PointBrush_BehindCanvas;
            //        DrawOptions.CanvasCutoffMode = CanvasCutoffMode.ShowBehindOnly;
            //        ViewPrimitives.Draw(g);

            //        PointBrush = PointBrush_InFrontOfCanvas; ;
            //        DrawOptions.CanvasCutoffMode = CanvasCutoffMode.ShowInFrontOnly;
            //        ViewPrimitives.Draw(g);

            //        PointBrush = backupBrush;
            //        DrawOptions.CanvasCutoffMode = CanvasCutoffMode.ToggleColor;

            //    }
            //}
        }

        private static void DrawRedBlue(DrawOptions options)
        {
            bool buShowArcs = DrawOptions.ShowArcs;
            DrawOptions.ShowArcs = false;


            float transparency = .6f;
            float[][] mtrx = new float[5][] {
            new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
            new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
            new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
            new float[] {0.0f, 0.0f, 0.0f, transparency, 0.0f},
            new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}};


            ColorMatrix colorMatrix = new ColorMatrix(mtrx);
            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                Color leftColor = DrawOptions.SwitchLeftRight ? Color.Red : Color.Cyan;
                Color rightColor = DrawOptions.SwitchLeftRight ? Color.Cyan : Color.Red;

                //The rightBitmap will be blitted onto g after the left image is drawn onto g
                Bitmap rightBitmap = new Bitmap(ViewContext.CanvasSize.Width, ViewContext.CanvasSize.Height);
                Graphics rightGraphics = Graphics.FromImage(rightBitmap);
                DrawOptions rightOptions = options.Clone();
                rightOptions.Graphics = rightGraphics;

                rightGraphics.Clear(rightColor);
                options.Graphics.Clear(leftColor);

                ViewContext.StereoscopicMode = StereoscopicMode.Left; 
                Shapes.Refresh(DrawOptions.SwitchBackFront, true);
                    
                DrawNormal(options, false);

                ViewContext.StereoscopicMode = StereoscopicMode.Right;
                Shapes.Refresh(DrawOptions.SwitchBackFront, true);
                    
                DrawNormal(rightOptions, false);
                ViewContext.StereoscopicMode = StereoscopicMode.NonStereoscopic;

                Shapes.Refresh(DrawOptions.SwitchBackFront, true);
                    
                options.Graphics.DrawImage(rightBitmap, new Rectangle(0, 0, rightBitmap.Width, rightBitmap.Height), 0, 0, rightBitmap.Width, rightBitmap.Height, GraphicsUnit.Pixel, ia);
            }

            DrawOptions.ShowArcs = buShowArcs;
        }
        private static void DrawStereoscopic(DrawOptions options)
        {
            //because we're shrinking the drawing, we need to increase the point size.
            DrawOptions.PointWidth *= 1.5;
            //shrink and move the drawing to the left by manipulating g's TransformMatrix.

            //offset the first direction (default left)
            if (DrawOptions.SwitchLeftRight)
                TranslateGraphicsRight(options);
            else
                TranslateGraphicsLeft(options);

            ViewContext.StereoscopicMode = StereoscopicMode.Left;
            Shapes.Refresh(DrawOptions.SwitchBackFront, true);
            DrawNormal(options, false);

            //now offset the other direction (default right)
            if (DrawOptions.SwitchLeftRight)
                TranslateGraphicsLeft(options);
            else
                TranslateGraphicsRight(options);

            ViewContext.StereoscopicMode = StereoscopicMode.Right;
            Shapes.Refresh(DrawOptions.SwitchBackFront, true);
            DrawNormal(options, false);

            //reset the ViewAngle and graphics transform
            ViewContext.StereoscopicMode = StereoscopicMode.NonStereoscopic;
            Shapes.Refresh(DrawOptions.SwitchBackFront, true);
            options.Graphics.ResetTransform();
            options.Graphics.ResetClip();
            //set the point size back to where it was.
            DrawOptions.PointWidth /= 1.5;
        }

        private static void TranslateGraphicsLeft(DrawOptions options)
        {
            Graphics g = options.Graphics;
            Size canvasSize = ViewContext.CanvasSize;
            g.ResetTransform();
            g.ResetClip();
            g.SetClip(new Rectangle(0,0,canvasSize.Width / 2, canvasSize.Height));
            ShrinkGraphics(g);
            g.TranslateTransform(-canvasSize.Width / 4, 0, MatrixOrder.Append);
        }
        private static void TranslateGraphicsRight(DrawOptions options)
        {
            Graphics g = options.Graphics;
            Size canvasSize = ViewContext.CanvasSize;
            g.ResetTransform();
            g.ResetClip();
            g.SetClip(new Rectangle(canvasSize.Width / 2, 0, canvasSize.Width / 2, canvasSize.Height));
            ShrinkGraphics(g);
            g.TranslateTransform(canvasSize.Width / 4, 0, MatrixOrder.Append);
        }
        private static void ShrinkGraphics(Graphics g)
        {
            Size canvasSize = ViewContext.CanvasSize;
            //in reverse order, do the following: translate center to 0,0, shrink, translate center back.
            g.TranslateTransform(canvasSize.Width / 2, canvasSize.Height / 2);
            g.ScaleTransform(.5f, .5f);
            g.TranslateTransform(-canvasSize.Width / 2, -canvasSize.Height / 2);
        }


        private static void DrawPrint(DrawOptions options)
        {
            DrawNormal(options, true);

            foreach (IndexedFaceSet ifs in EdgePainter.ShapeList)
            {
                foreach (Edge e in ifs.Edges)
                {
                    Rectangle r = Transformer.GetArcSquare(e.StartVertex.ViewCoord_ZeroAngle);
                    Point center = new Point(r.X + (int)(r.Width / 2), r.Y + (int)(r.Height / 2));
                    if (options.IsRendering)
                    {
                        options.Graphics.FillEllipse(Brushes.Black, new Rectangle(center.X - 1, center.Y - 1, 3, 3));
                    }
                }
            }

            //if (ShowArcs)
            //{
            //    using (Pen pPen = new Pen(Color.LightGray))
            //    {

            //        foreach(ViewPoint vp in viewPrimitives)
            //            if (vp.BaseVoxel.IsEndPoint)
            //                vp.DrawArc(g, pPen);

            //    }
            //}

            //using (Pen pPen = new Pen(pointColor))
            //{
            //    using (Brush pBrush = pPen.Brush)
            //    {

            //        foreach(ViewPoint vp in viewPrimitives)
            //        {

            //            Point center = Point.Round(vp.ArcCenter);
            //            DrawPoint(g, Point.Round(center), pBrush, PointWidth);

            //            if (ShowPoints)
            //            {
            //                if (vp.BaseVoxel.IsEndPoint)
            //                {
            //                    FontStyle fontStyle = FontStyle.Regular;
            //                    g.DrawString(Convert.ToInt32(vp.ArcRadius).ToString(), new Font("Arial", 8f, fontStyle), pBrush, Point.Round(new PointF(center.X + (int)(PointWidth / 2) + 2, center.Y)));
            //                }
            //            }
            //        }
            //    }
            //}
        }




        //private static void DrawPoints(Graphics g, Color pointColor, ViewPrimitiveList viewPrimitives, bool forceSpecifiedColor)
        //{
        //            foreach (ViewPrimitive vp in viewPrimitives)
        //            {
        //                if (vp.BasePrimitive.DrawAsVector || DrawAllAsVectors)
        //                    DrawAsVector(g, vp, pointColor, forceSpecifiedColor);
        //                else
        //                    vp.DrawPoints(g, pBrush);
        //            }
        //        }
        //    }
        //}


        

        ///// <summary>Rotates a ViewPoint 180 degrees about another ViewPoint.</summary>
        //private static void RotatePoint(ViewPoint stationaryPoint, ViewPoint pointToRotate)
        //{
        //    pointToRotate.Location = new ViewCoord(stationaryPoint.Location - (pointToRotate.Location - stationaryPoint.Location), pointToRotate.IsBehindUser);
        //}

        ///// <summary>If one or both of the given points are off the canvas, this method moves the problematic point(s) to the edge of the canvas. The points remain on the line that connects them.</summary>
        ///// <returns>True if there were no problems moving the points to the edges of the canvas.</returns>
        //private static bool EnsureOnScreen(ViewPoint startPoint, ViewPoint endPoint)
        //{
        //    CutOffLineAtXValue(startPoint, endPoint, 0);
        //    CutOffLineAtXValue(endPoint, startPoint, 0);
        //    CutOffLineAtYValue(startPoint, endPoint, 0);
        //    CutOffLineAtYValue(endPoint, startPoint, 0);
        //    CutOffLineAtXValue(startPoint, endPoint, ViewContext.CanvasSize.Width);
        //    CutOffLineAtXValue(endPoint, startPoint, ViewContext.CanvasSize.Width);
        //    CutOffLineAtYValue(startPoint, endPoint, ViewContext.CanvasSize.Height);
        //    CutOffLineAtYValue(endPoint, startPoint, ViewContext.CanvasSize.Height);

        //    return true;
        //}

        //private static void CutOffLineAtXValue(ViewPoint pointToCutOff, ViewPoint anotherPointOnLine, double xValue)
        //{
        //    //...if xValue is between the two points
        //    if (xValue > Math.Min(pointToCutOff.Location.X, pointToCutOff.Location.X) && xValue < Math.Max(pointToCutOff.Location.X, pointToCutOff.Location.X))
        //    {
        //        double lengthOfX = Math.Abs(anotherPointOnLine.Location.X) + Math.Abs(pointToCutOff.Location.X);
        //        double validPercent = (Math.Abs(anotherPointOnLine.Location.X) + xValue) / lengthOfX;

        //        double shiftX = -anotherPointOnLine.Location.X;
        //        double shiftY = -anotherPointOnLine.Location.Y;
        //        double shiftZ = -anotherPointOnLine.Location.Z;

        //        pointToCutOff.Location.X = ((pointToCutOff.Location.X + shiftX) * validPercent - shiftX);
        //        pointToCutOff.Location.Y = ((pointToCutOff.Location.Y + shiftY) * validPercent - shiftY);
        //        pointToCutOff.Location.Z = ((pointToCutOff.Location.Z + shiftZ) * validPercent - shiftZ);
        //    }
        //}
        //private static void CutOffLineAtYValue(ViewPoint pointToCutOff, ViewPoint anotherPointOnLine, double yValue)
        //{
        //    //...if yValue is between the two points
        //    if (yValue > Math.Min(pointToCutOff.Location.Y, pointToCutOff.Location.Y) && yValue < Math.Max(pointToCutOff.Location.Y, pointToCutOff.Location.Y))
        //    {
        //        double lengthOfY = Math.Abs(anotherPointOnLine.Location.Y) + Math.Abs(pointToCutOff.Location.Y);
        //        double validPercent = (Math.Abs(anotherPointOnLine.Location.Y) + yValue) / lengthOfY;

        //        double shiftX = -anotherPointOnLine.Location.X;
        //        double shiftY = -anotherPointOnLine.Location.Y;
        //        double shiftZ = -anotherPointOnLine.Location.Z;

        //        pointToCutOff.Location.X = ((pointToCutOff.Location.X + shiftX) * validPercent - shiftX);
        //        pointToCutOff.Location.Y = ((pointToCutOff.Location.Y + shiftY) * validPercent - shiftY);
        //        pointToCutOff.Location.Z = ((pointToCutOff.Location.Z + shiftZ) * validPercent - shiftZ);
        //    }
        //}









        ///// <summary>Sets VoxelToViewPointMatrix such that the specified Primitives list will fit exactly within the specified Rectangle.</summary>
        //public static void AutoFitWithin(Rectangle fitWithin, PrimitiveList primitives)
        //{
        //    //if (primitives.Count > 0)
        //    //{
        //    //    Rectangle boundingRect = Rectangle.Round(new ViewPointList(primitives).BoundingRect);
        //    //    double currentAspectRatio = fitWithin.Width / (double)fitWithin.Height;
        //    //    double desiredAspectRatio = boundingRect.Width / (double)boundingRect.Height;

        //    //    if (currentAspectRatio > desiredAspectRatio)
        //    //    {
        //    //        //Too wide. We need to adjust the left and right.
        //    //        int desiredWidth = (int)(fitWithin.Height * desiredAspectRatio);
        //    //        int dx = (int)((desiredWidth - fitWithin.Width) / 2);
        //    //        fitWithin.X -= dx;
        //    //        fitWithin.Width = desiredWidth;
        //    //    }
        //    //    else
        //    //    {
        //    //        //Too tall. We need to adjust the top and bottom.
        //    //        int desiredHeight = (int)(fitWithin.Width / desiredAspectRatio);
        //    //        int dy = (int)((desiredHeight - fitWithin.Height) / 2);
        //    //        fitWithin.Y -= dy;
        //    //        fitWithin.Height = desiredHeight;
        //    //    }

        //    //    ViewContext.FitToScreenMatrix2D = new System.Drawing.Drawing2D.Matrix();
        //    //    ViewContext.FitToScreenMatrix2D.Translate(fitWithin.X, fitWithin.Y);
        //    //    ViewContext.FitToScreenMatrix2D.Scale(fitWithin.Width / (float)boundingRect.Width, fitWithin.Height / (float)boundingRect.Height);
        //    //    ViewContext.FitToScreenMatrix2D.Translate(-boundingRect.X, -boundingRect.Y);
        //    //}
        //}



        #region Helper Functions

        //public static PointF TransformPoint(PointF toTransform, System.Drawing.Drawing2D.Matrix transformMatrix)
        //{
        //    PointF[] ps = new PointF[] { toTransform };
        //    transformMatrix.TransformPoints(ps);
        //    return ps[0];
        //}

        //public static RectangleF TransformRectangle(RectangleF toTransform, System.Drawing.Drawing2D.Matrix transformMatrix)
        //{
        //    PointF[] ps = new PointF[2] { toTransform.Location, new PointF(toTransform.Right, toTransform.Bottom) };
        //    transformMatrix.TransformPoints(ps);
        //    return RectangleF.FromLTRB(ps[0].X, ps[0].Y, ps[1].X, ps[1].Y);
        //}

        public static void DrawPoint(DrawOptions options, Point p, Brush b, int pointSize)
        {
            if (options.IsRendering)
            {
                options.Graphics.FillEllipse(b, p.X - (int)(DrawOptions.PointWidth / 2), p.Y - (int)(DrawOptions.PointWidth / 2), pointSize, pointSize);
            }
        }
        #endregion



        /// <summary>
        /// Blits the off-screen buffer onto the supplied Graphics object. This function should be called from the consuming usercontrol's OnPaint function.
        /// </summary>
        /// <param name="g"></param>
        public static void Blit(Graphics g)
        {
            bool isRendering = true;
            bool isRendering2 = DrawOptions.ShowGlow;
            bool isAnimating = DrawOptions.ShowGlow;

            if (DoDraw)
            {
                if (NextRedraw != RedrawTypeRequired.None || isAnimating)
                    ReDraw(isRendering, isRendering2, isAnimating);

                g.ResetTransform();
                if (DrawOptions.RotateCanvas)
                {
                    /*
                     * Rotate 180 degrees about the center of the View. To do this, 
                     * we rotate 180 about the top-left (which puts the rotated image 
                     * above and to the left of the view), then translate back onto the 
                     * screen. Matrix operations are done in reverse.
                     */
                    g.TranslateTransform(CanvasSize.Height, CanvasSize.Width);
                    g.RotateTransform(180);
                }

                var destRect = new Rectangle(
                    0,
                    0,
                    s_offScreenBitmap.Width,
                    s_offScreenBitmap.Height);

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // Optional for wider blur...
                g.CompositingQuality = CompositingQuality.HighQuality;
                //g.SmoothingMode = SmoothingMode.AntiAlias;

                g.DrawImage(
                    s_offScreenBitmap2,
                    destRect,
                    srcX: 0,
                    srcY: 0,
                    srcWidth: s_offScreenBitmap2.Width,
                    srcHeight: s_offScreenBitmap2.Height,
                    srcUnit: GraphicsUnit.Pixel
                    );

                g.CompositingMode = CompositingMode.SourceOver;

                g.DrawImage(
                    s_offScreenBitmap,
                    destRect,
                    srcX: 0,
                    srcY: 0,
                    srcWidth: s_offScreenBitmap.Width,
                    srcHeight: s_offScreenBitmap.Height,
                    srcUnit: GraphicsUnit.Pixel
                    );
            }
        }

        public static void AddShapes(List<IndexedFaceSet> ifs)
        {
            foreach(var indexedFace in ifs)
            {
                Shapes.Add(indexedFace);
            }
            MarkAsDirty(RedrawTypeRequired.RecalculateViewPrimitives);
        }

        public static void ClearShapes()
        {
            Shapes.Clear();
        }

        public static void PreProcessShapes()
        {
            Shapes.PreProcess();
        }
    }
}
