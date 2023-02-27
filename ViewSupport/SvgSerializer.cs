using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ViewSupport
{
    // TODO:P1 Currently, Negative Coords are emitted as-is.  Should intentionally clip or offset/shift negative xy coords before conversion.
    public class SvgSerializer
    {
        public SvgSerializer()
        {

        }

        public List<EdgePainter.ArcSegInfo> DeSerializeJson(string filePath)
        {
            string arcSegsJsonText = File.ReadAllText(filePath);
            object parsedObj = JsonConvert.DeserializeObject(arcSegsJsonText, typeof(List<EdgePainter.ArcSegInfo>));
            List<EdgePainter.ArcSegInfo> arcSegs = parsedObj as List<EdgePainter.ArcSegInfo>;

            return arcSegs;
        }


        public StringBuilder Serialize(List<EdgePainter.ArcSegInfo> arcSegs)
        {
            // SVG Format:
            // - https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Paths
            // - Absolute Uppercase, Relative Lowercase
            //   L x y  Line to x y coord
            //   H x    Horizontal Line
            //   V y    Vertical Line
            //   M x1 y1    Move to...
            //   A rx ry ang large-arc-flag[0|1] sweep-flag[0|1] X2 Y2

            float maxX = 1;
            float maxY = 1;

            StringBuilder sb = new StringBuilder();

            if (arcSegs != null)
            {
                float pi = (float)Math.PI;

                arcSegs.Sort((as1, as2) =>
                    {
                        string as1Hash =
                            $"{as1.EdgeId:000000}:{Math.Round(as1.ZeroCoord.X, 2):000000}:{Math.Round(as1.ZeroCoord.Y, 2):000000}:{as1.StartAngle}:{as1.SweepAngle}";
                        string as2Hash =
                            $"{as2.EdgeId:000000}:{Math.Round(as2.ZeroCoord.X, 2):000000}:{Math.Round(as2.ZeroCoord.Y, 2):000000}:{as2.StartAngle}:{as2.SweepAngle}";

                        return as1Hash.CompareTo(as2Hash);
                    });

                foreach (var arcSeg in arcSegs)
                {
                    // TODO:P1 Should 0 and/or small angle sweep be skipped or merged with close by sweeps?  Maybe 0 or small angle gaps are logic/rounding BUG needing to be fixed?

                    // Massage sweep to minimum of 1 degree.
                    Debug.Assert(arcSeg.SweepAngle >= 0);
                    int sweepAngle = Math.Max(1, arcSeg.SweepAngle);

                    int sweepFlag = 1;
                    float startRad = (arcSeg.StartAngle * pi) / 180;
                    float endRad = ((arcSeg.StartAngle + sweepAngle) * pi) / 180;
                    float r = arcSeg.ArcRect.Width / 2;

                    float cx = arcSeg.ArcRect.X + arcSeg.ArcRect.Width / 2;
                    float cy = arcSeg.ArcRect.Y + arcSeg.ArcRect.Height / 2;
                    float x1 = cx + (float)(r * Math.Cos(startRad));
                    float y1 = cy + (float)(r * Math.Sin(startRad));
                    float x2 = cx + (float)(r * Math.Cos(endRad));
                    float y2 = cy + (float)(r * Math.Sin(endRad));

                    if (x1 > maxX) maxX = x1;
                    if (x2 > maxX) maxX = x2;
                    if (y1 > maxY) maxY = y1;
                    if (y2 > maxY) maxY = y2;

                    sb.AppendLine($"M {x1} {y1}");
                    sb.AppendLine($"A {r} {r} 0 0 {sweepFlag} {x2} {y2}");
                }
            }

            int width = (int)maxX;
            int height = (int)maxY;

            StringBuilder sbDoc = new StringBuilder();
            sbDoc.AppendLine($"<svg width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">");
            sbDoc.AppendLine("<path stroke=\"black\" stroke-width=\"1\" d=\"");

            sbDoc.AppendLine(sb.ToString());

            sbDoc.AppendLine("\"/></svg>");

            return sbDoc;
        }
    }
}
