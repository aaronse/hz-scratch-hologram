using FileParser;
using Primitives;
using ScratchUtility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System;

namespace ScratchTest
{
    public partial class ScratchTest : Form
    {
        public ScratchTest()
        {
            InitializeComponent();

            //halt drawing until we're all set up
            //mView.DrawingEnabled = false;
            mView.ArcSweepAngle = mArcSweepAngleTrack.Value;
            mView.ViewAngle = mViewAngleTrack.Value;
            mView.ShowArcs = mArcCheckBox.Checked;
            mView.ShowGCode = mGcodeCheckBox.Checked;
            mView.RotateCanvas = mDebugCheckBox.Checked;
            SetPointsPerUnitLength();
            ViewSupport.DrawOptions.QuickMode = quickModeCheckBox.Checked;
            SetVisibilityMode();
            mView.SwitchLeftRight = mSwitchCheckBox.Checked;
            TrySetPointWidth();
            SetUpFileList(@".\..\..\..\Data"); // C:\Projects(NAS)\HoloZens\scratchhologram\Data");
                                               //           SetUpFileList(@"C:\Program Files\Blender Foundation\Blender");
                                               //begin drawing
                                               //mView.DrawingEnabled = true;

            mView.Paint += MView_Paint;

            ApplyTheme();
        }

 
        public void SetVisibilityMode()
        {
            mView.VisibilityMode = mHiddenLineCheckBox.Checked ? ViewSupport.VisibilityMode.HiddenLine : ViewSupport.VisibilityMode.Transparent;
        }



        private void mArcSweepAngleTrack_Scroll(object sender, EventArgs e)
        {
            if(!DesignMode)
            {
                mView.ArcSweepAngle = mArcSweepAngleTrack.Value;
                UpdateOutputSummary();
            } 
        }

        private void mViewAngleTrack_Scroll(object sender, EventArgs e)
        {
        }
        private void mViewAngleTrack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
                mViewAngleTrack.Value = 0;
        }

        private void mViewAngleTrack_ValueChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ViewAngle = mViewAngleTrack.Value;
                UpdateOutputSummary();
            }
        }

        private void mLineResolutionTrack_Scroll(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                SetPointsPerUnitLength();
                UpdateOutputSummary();
            }
        }

        private void SetPointsPerUnitLength()
        {
            mView.ViewPointsPerUnitLength = mLineResolutionTrack.Value / 100.0;
        }

        private void mArcCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ShowArcs = mArcCheckBox.Checked;
                UpdateOutputSummary();
            }
        }

        private void mGcodeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ShowGCode = mGcodeCheckBox.Checked;
                UpdateOutputSummary();
            }
        }

        private void mDebugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                Global.DebugMode = mDebugCheckBox.Checked;
                UpdateOutputSummary();
            }
        }

        private void mPointSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
                TrySetPointWidth();
        }

        private void TrySetPointWidth()
        {
            int size = 0;
            if (int.TryParse(mPointSizeTextBox.Text, out size) || mPointSizeTextBox.Text.Length == 0)
            {
                mView.PointWidth = size;
            }
            else
            {
                mPointSizeTextBox.Text = mView.PointWidth.ToString();
            }
        }

        private void mViewModeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (mPrintRadioButton.Checked)
                {
                    mView.ViewMode = ViewSupport.ViewMode.Print;
                }
                else
                {
                    if (mRedBlueRadioButton.Checked)
                        mView.ViewMode = ViewSupport.ViewMode.RedBlue;
                    else if (mStereoscopicRadioButton.Checked)
                        mView.ViewMode = ViewSupport.ViewMode.Stereoscopic;
                    else if (mDarkRadioButton.Checked)
                    {
                        ViewSupport.ThemeInfo.Current = ViewSupport.ThemeInfo.DarkTheme;
                        mView.ViewMode = ViewSupport.ViewMode.Dark;
                        ApplyTheme();
                    }
                    else
                    {
                        ViewSupport.ThemeInfo.Current = ViewSupport.ThemeInfo.LightTheme;
                        mView.ViewMode = ViewSupport.ViewMode.Normal;
                        ApplyTheme();
                    }
                }
            }
        }
        private void mVisibilityMode_CheckedChanged(object sender, EventArgs e)
        {
            if(((RadioButton)sender).Checked)
                SetVisibilityMode();
        }


        private string GetFaceString(params int[] coordIndices)
        {
            string ret = "";
            foreach (int i in coordIndices)
            {
                ret += i.ToString() + " ";
            }
            return ret + "-1, ";
        }
        private string GetCoordString(params Coord[] coords)
        {
            string ret = "";
            foreach (Coord c in coords)
            {
                ret += CoordString(c) + " ";
            }
            return ret;
        }

        private string GetCubeCoordString(double xWidth, double yWidth, double zWidth, double xOffset, double yOffset, double zOffset)
        {
            string ret = "";
            ret += CoordString(new Coord(0 + xOffset, yWidth + yOffset, 0 + zOffset)); //topBack
            ret += CoordString(new Coord(xWidth + xOffset, yWidth + yOffset, 0 + zOffset)); //topRight
            ret += CoordString(new Coord(xWidth + xOffset, yWidth + yOffset, zWidth + zOffset)); //topFront
            ret += CoordString(new Coord(0 + xOffset, yWidth + yOffset, zWidth + zOffset)); //topLeft
            ret += CoordString(new Coord(0 + xOffset, 0 + yOffset, 0 + zOffset)); //bottomBack
            ret += CoordString(new Coord(xWidth + xOffset, 0 + yOffset, 0 + zOffset)); //bottomRight
            ret += CoordString(new Coord(xWidth + xOffset, 0 + yOffset, zWidth + zOffset)); //bottomFront
            ret += CoordString(new Coord(0 + xOffset, 0 + yOffset, zWidth + zOffset)); //bottomLeft
            return ret;
        }

        private string CoordString(Coord c)
        {
            return c.X + " " + c.Y + " " + c.Z + ", ";
        }


        private void LoadX3DFile(X3DFile file, double scale)
        {
            file.Parse(scale);
            mView.ClearShapes();
            foreach (IndexedFaceSet ifs in file.IndexedFaces)
            {
                mView.AddShape(ifs);
            }
            mView.PreProcessShapes();
        }


        private void zf_TextChanged(object sender, EventArgs e)
        {
            if(zf.Text == "")
            {
                mView.SetZf(0);
                zfTrackBar.Value = 0;
            }
            else
            {
                mView.SetZf(double.Parse(zf.Text));
                zfTrackBar.Value = (int)(double.Parse(zf.Text) * 100);
            }
        }

        private void zfTrackBar_Scroll(object sender, EventArgs e)
        {
            zf.Text = (zfTrackBar.Value / 100.0).ToString();
        }

        private void mSwitchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            mView.SwitchLeftRight = mSwitchCheckBox.Checked;
        }

        private void mEyesTrackBar_Scroll(object sender, EventArgs e)
        {
            mView.StereoscopicDisparityAngle = mEyesTrackBar.Value / 2.0;
        }

        private void mSwitchBackFrontCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            mView.SwitchBackFront = mSwitchBackFrontCheckBox.Checked;
        }

        private void mOpenButton_Click(object sender, EventArgs e)
        {
            mOpenFileDialog.ShowDialog();
        }



        private void mOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            HandleOpenFile(mOpenFileDialog.FileName);
        }

        private void HandleOpenFile(string fileName)
        {
            SetUpFileList(Path.GetDirectoryName(fileName), fileName);
        }

        private void SetUpFileList(string directory, string selectFileName)
        {
            mFilesComboBox.Items.Clear();
            foreach (string fileName in Directory.GetFiles(directory, "*.x3d"))
            {
                X3DFile f = new X3DFile(fileName);
                mFilesComboBox.Items.Add(f);
                if (fileName == selectFileName)
                    mFilesComboBox.SelectedIndex = mFilesComboBox.Items.Count - 1;
            }
        }
        private void SetUpFileList(string directory)
        {
            SetUpFileList(directory, null);
        }

        private void mFilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadX3DFile((X3DFile)(mFilesComboBox.SelectedItem), 1);
        }

        private void mVectorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.VectorMode = mVectorsCheckBox.Checked;
                UpdateOutputSummary();
            }
        }

        private void mPointsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.PointsMode = mPointsCheckbox.Checked;
                UpdateOutputSummary();
            }
        }

        private void quickModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ViewSupport.DrawOptions.QuickMode = quickModeCheckBox.Checked;
        }
        private void mHiddenLineCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetVisibilityMode();
            UpdateOutputSummary();
        }

        private void mMergeFacesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                ViewSupport.DrawOptions.MergeFaces = mMergeFacesCheckBox.Checked;
            }
        }


        private void UpdateOutputSummary()
        {
            var arcSegs = ViewSupport.EdgePainter.ArcSegments;
            float arcDurSecs = 2;   // TODO: Update based on actual run and/or proper math taking distance/feed into account.
            string summary = "";

            if (arcSegs != null && arcSegs.Count > 0)
            {
                summary = $"Arcs = {arcSegs.Count}, EstHrs = {(arcSegs.Count * arcDurSecs) / 3600.0:F2}";
                // TODO: Implement GCODE Size estimator... Was... Gcode = {gcodeText.Length / 1024}kb...
            }

            summary += $"{((summary.Length > 0) ? ", " :"")}Cam = {ViewContext.Po.ToString(3)}:{ViewContext.Pr.ToString(3)}";

            txtOutputSummary.Text = summary;
        }

        private void mArcSegmentsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ShowArcSegments = mArcSegmentsCheckbox.Checked;
                UpdateOutputSummary();
            }
        }

        private void MView_Paint(object sender, PaintEventArgs e)
        {
            UpdateOutputSummary();
        }


        private void mGenerateButton_Click(object sender, EventArgs e)
        {
            mView.ShowArcs = mArcCheckBox.Checked = true;

            DateTime start = DateTime.Now;

            ViewSupport.Drawing.ReDraw(false);

            Debug.WriteLine("Drawing.ReDraw, durMs=" + (int)DateTime.Now.Subtract(start).TotalMilliseconds + " IndexedFace._count=" + IndexedFace._count);

            ViewSupport.SvgSerializer svgSerializer = new ViewSupport.SvgSerializer();
            var svgText = svgSerializer.Serialize(ViewSupport.EdgePainter.ArcSegments);
            txtOutput.Text = svgText.ToString();

            UpdateOutputSummary();
        }


        private void mExportSvgDialog_FileOk(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Export to " + mExportSvgDialog.FileName);
        }

        private void btnExportSvg_Click(object sender, EventArgs  e)
        {
            ViewSupport.SvgSerializer svgSerializer = new ViewSupport.SvgSerializer();
            var svgText = svgSerializer.Serialize(ViewSupport.EdgePainter.ArcSegments);


            var arcSegs = ViewSupport.EdgePainter.ArcSegments;
            //StringBuilder sbArcsJson = new StringBuilder();
            //if (arcSegs != null)
            //{
            //    sbArcsJson.AppendLine("[");
            //    for (int i = 0; i < arcSegs.Count; i++)
            //    {
            //        var arcSeg = arcSegs[i];
            //        if (i > 0) sbArcsJson.Append(", ");
            //        sbArcsJson.AppendLine(arcSeg.ToString());
            //    }
            //    sbArcsJson.AppendLine("]");
            //}


            //if (mExportSvgDialog.ShowDialog() == DialogResult.OK)
            {
                string svgFilePath = "c:\\Users\\aaron\\Documents\\test.svg";
                //string svgFilePath = mExportSvgDialog.FileName;

                File.WriteAllText(svgFilePath, svgText.ToString());

                //string arcsFilePath = Path.ChangeExtension(svgFilePath, "alt.json");
                //File.WriteAllText(arcsFilePath, sbArcsJson.ToString());

                string arcsFilePath = Path.ChangeExtension(svgFilePath, "json");
                string arcSegsJson = Newtonsoft.Json.JsonConvert.SerializeObject(arcSegs);
                File.WriteAllText(arcsFilePath, arcSegsJson.Replace(",{", ",\r\n{"));
            }
        }


        private void ApplyTheme()
        {
            foreach(Control control in this.Controls)
            {
                control.ForeColor = ViewSupport.ThemeInfo.Current.TextColor;
                control.BackColor = ViewSupport.ThemeInfo.Current.WindowBackColor;
            }

            this.ForeColor = ViewSupport.ThemeInfo.Current.TextColor;
            this.BackColor = ViewSupport.ThemeInfo.Current.WindowBackColor;
        }

        private void btnImportArcs_Click(object sender, EventArgs e)
        {
            ViewSupport.SvgSerializer svgSerializer = new ViewSupport.SvgSerializer();

            string filePath = "C:\\Users\\aaron\\Documents\\test.2.json";
            var arcSegs = svgSerializer.DeSerializeJson(filePath);

            ViewSupport.EdgePainter.ArcSegments = arcSegs;

            UpdateOutputSummary();
        }
    }


}
