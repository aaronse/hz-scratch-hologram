using FileParser;
using Primitives;
using ScratchUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ViewSupport;
using System.Linq;

namespace ScratchTest
{
    public partial class ScratchTest : Form
    {
        private bool _handleEvents = true;

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
            mView.ProfileMode = mProfileCheckbox.Checked;
            SetPointsPerUnitLength();
            DrawOptions.QuickMode = quickModeCheckBox.Checked;
            SetVisibilityMode();
            mView.SwitchLeftRight = mSwitchCheckBox.Checked;
            DrawOptions.SelectedItemExpr = txtSelectedItem.Text;
            TrySetPointWidth();
            SetUpFileList(@".\..\..\..\Data"); // C:\Projects(NAS)\HoloZens\scratchhologram\Data");
                                               //           SetUpFileList(@"C:\Program Files\Blender Foundation\Blender");
                                               //begin drawing
                                               //mView.DrawingEnabled = true;
            mView.Paint += MView_Paint;
            mView.ViewOptionsChanged += MView_ViewOptionsChanged;
            timer1.Interval = 500;
            timer1.Enabled = true;
            timer1.Tick += Timer1_Tick;
            ApplyTheme();
        }

        public void SetVisibilityMode()
        {
            mView.VisibilityMode = mHiddenLineCheckBox.Checked ? VisibilityMode.HiddenLine : VisibilityMode.Transparent;
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
            if (!DesignMode && _handleEvents)
            {
                mView.ViewAngle = mViewAngleTrack.Value;

                _handleEvents = false;

                txtViewAngle.Text = mViewAngleTrack.Value.ToString();
                UpdateOutputSummary();

                _handleEvents = true;
            }
        }

        private void mLineResolutionTrack_Scroll(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                _handleEvents = false; 

                SetPointsPerUnitLength();
                txtLineResolution.Text = (mLineResolutionTrack.Value / 100.0).ToString();
                UpdateOutputSummary();

                _handleEvents = true;
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
                    mView.ViewMode = ViewMode.Print;
                }
                else
                {
                    if (mRedBlueRadioButton.Checked)
                        mView.ViewMode = ViewMode.RedBlue;
                    else if (mStereoscopicRadioButton.Checked)
                        mView.ViewMode = ViewMode.Stereoscopic;
                    else if (mDarkRadioButton.Checked)
                    {
                        ThemeInfo.Current = ThemeInfo.DarkTheme;
                        mView.ViewMode = ViewMode.Dark;
                        ApplyTheme();
                    }
                    else
                    {
                        ThemeInfo.Current = ThemeInfo.LightTheme;
                        mView.ViewMode = ViewMode.Normal;
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


        private void AttachModelToUi(List<IndexedFaceSet> ifs)
        {
            mView.ClearShapes();
            mView.AddShapes(ifs);
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

        class ModelFile
        { 
            public string Name { get; set; }
            public string FilePath { get; set; }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private void SetUpFileList(string directory, string selectFileName)
        {
            
            var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".stl", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".x3d", StringComparison.OrdinalIgnoreCase))
                .Select(f => new ModelFile() { FilePath = f, Name = Path.GetFileName(f) } );

            mFilesComboBox.Items.Clear();
            foreach (var file in files)
            {
                //X3DFile f = new X3DFile(fileName);
                mFilesComboBox.Items.Add(file);
                //if (fileName == selectFileName)
                if (string.Equals(file.Name, Path.GetFileName(selectFileName), StringComparison.OrdinalIgnoreCase))
                    mFilesComboBox.SelectedIndex = mFilesComboBox.Items.Count - 1;
            }
        }

        private void SetUpFileList(string directory)
        {
            SetUpFileList(directory, null);
        }

        private void mFilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool autoCenter = true;

            var modelFile = mFilesComboBox.SelectedItem as ModelFile;
            if (".x3d".Equals(Path.GetExtension(modelFile.FilePath), StringComparison.OrdinalIgnoreCase))
            {
                X3DFile x3dFile = new X3DFile(modelFile.FilePath);
                x3dFile.Parse(scale: 1.0, autoCenter);

                AttachModelToUi(x3dFile.IndexedFaceSets);
            }
            else if (".stl".Equals(Path.GetExtension(modelFile.FilePath), StringComparison.OrdinalIgnoreCase))
            {
                var scene = new List<IndexedFaceSet>();
                var model = StlSerializer.Deserialize(modelFile.FilePath, autoCenter);
                scene.Add(model);

                AttachModelToUi(scene);
            }
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
            if (!DesignMode)
            {
                DrawOptions.QuickMode = quickModeCheckBox.Checked;
            }
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
                DrawOptions.MergeFaces = mMergeFacesCheckBox.Checked;
            }
        }


        private void UpdateOutputSummary()
        {
            var arcSegs = EdgePainter.ArcSegments;
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

        private void MView_ViewOptionsChanged(object sender, EventArgs e)
        {
            if (DrawOptions.ShowArcSegments != mArcSegmentsCheckbox.Checked)
            {
                mArcSegmentsCheckbox.Checked = DrawOptions.ShowArcSegments;
            }

            if (DrawOptions.ShowArcs != mArcCheckBox.Checked)
            {
                mArcCheckBox.Checked = DrawOptions.ShowArcs;
            }
        }



        private void mExportSvgDialog_FileOk(object sender, CancelEventArgs e)
        {
            throw new NotImplementedException("TODO: Export to " + mExportSvgDialog.FileName);
        }

        private void btnExportSvg_Click(object sender, EventArgs  e)
        {
            SvgSerializer svgSerializer = new SvgSerializer();
            var svgText = svgSerializer.Serialize(
                (!mArcSegmentsCheckbox.Checked) ? null : EdgePainter.ArcSegments,
                EdgePainter.Shapes);

            // TODO: Remove after verifying JSON serializer exists for target platform...
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

            // TODO:P0 User should select target file location
            //if (mExportSvgDialog.ShowDialog() == DialogResult.OK)
            {
                string svgFilePath = "c:\\Users\\aaron\\Documents\\test.svg";
                //string svgFilePath = mExportSvgDialog.FileName;

                File.WriteAllText(svgFilePath, svgText.ToString());

                //string arcsFilePath = Path.ChangeExtension(svgFilePath, "alt.json");
                //File.WriteAllText(arcsFilePath, sbArcsJson.ToString());

                var shapes = new List<VectorShape>();
                if (EdgePainter.ArcSegments?.Count > 0) shapes.AddRange(EdgePainter.ArcSegments);
                if (EdgePainter.Shapes?.Count > 0) shapes.AddRange(EdgePainter.Shapes);

                string arcsFilePath = Path.ChangeExtension(svgFilePath, "json");
                string arcSegsJson = Newtonsoft.Json.JsonConvert.SerializeObject(shapes);
                File.WriteAllText(arcsFilePath, arcSegsJson.Replace(",{", ",\r\n{"));
            }
        }


        private void ApplyTheme()
        {
            foreach(Control control in this.Controls)
            {
                control.ForeColor = ThemeInfo.Current.TextColor;
                control.BackColor = ThemeInfo.Current.WindowBackColor;
            }

            this.ForeColor = ThemeInfo.Current.TextColor;
            this.BackColor = ThemeInfo.Current.WindowBackColor;
        }

        private void txtSelectedItem_TextChanged(object sender, EventArgs e)
        {
            DrawOptions.SelectedItemExpr = txtSelectedItem.Text;
        }

        private void txtViewAngle_TextChanged(object sender, EventArgs e)
        {
            if (!DesignMode && _handleEvents)
            {
                int viewAngle = 0;
                if (int.TryParse(txtViewAngle.Text, out viewAngle))
                {
                    _handleEvents = false;

                    int newVal = Math.Max(-90, Math.Min(90, viewAngle));

                    mView.ViewAngle = newVal;
                    mViewAngleTrack.Value = newVal;

                    UpdateOutputSummary();

                    _handleEvents = true;
                }
            }
        }

        private void ScratchTest_Load(object sender, EventArgs e)
        {

        }

        private void mProfileCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ProfileMode = mProfileCheckbox.Checked;
                UpdateOutputSummary();
            }
        }

        private void mGlowCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                mView.ShowGlow = mGlowCheckBox.Checked;
                UpdateOutputSummary();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("tick");
        }
    }
}
