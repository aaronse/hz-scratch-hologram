namespace ScratchTest
{
    partial class ScratchTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mViewAngleTrack = new System.Windows.Forms.TrackBar();
            this.mArcSweepAngleTrack = new System.Windows.Forms.TrackBar();
            this.mArcCheckBox = new System.Windows.Forms.CheckBox();
            this.mPointSizeTextBox = new System.Windows.Forms.TextBox();
            this.mLineResolutionTrack = new System.Windows.Forms.TrackBar();
            this.mNormalRadioButton = new System.Windows.Forms.RadioButton();
            this.mRedBlueRadioButton = new System.Windows.Forms.RadioButton();
            this.mStereoscopicRadioButton = new System.Windows.Forms.RadioButton();
            this.mPrintRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.mDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.zf = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.zfTrackBar = new System.Windows.Forms.TrackBar();
            this.mSwitchCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.mEyesTrackBar = new System.Windows.Forms.TrackBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mDarkRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.mMergeFacesCheckBox = new System.Windows.Forms.CheckBox();
            this.quickModeCheckBox = new System.Windows.Forms.CheckBox();
            this.mHiddenLineCheckBox = new System.Windows.Forms.CheckBox();
            this.mVectorsCheckBox = new System.Windows.Forms.CheckBox();
            this.mSwitchBackFrontCheckBox = new System.Windows.Forms.CheckBox();
            this.mOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mOpenButton = new System.Windows.Forms.Button();
            this.mFilesComboBox = new System.Windows.Forms.ComboBox();
            this.mGenerateButton = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.mGcodeCheckBox = new System.Windows.Forms.CheckBox();
            this.txtOutputSummary = new System.Windows.Forms.TextBox();
            this.mView = new ScratchView.View();
            ((System.ComponentModel.ISupportInitialize)(this.mViewAngleTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mArcSweepAngleTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mLineResolutionTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zfTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mEyesTrackBar)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mViewAngleTrack
            // 
            this.mViewAngleTrack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mViewAngleTrack.Location = new System.Drawing.Point(17, 15);
            this.mViewAngleTrack.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mViewAngleTrack.Maximum = 90;
            this.mViewAngleTrack.Minimum = -90;
            this.mViewAngleTrack.Name = "mViewAngleTrack";
            this.mViewAngleTrack.Size = new System.Drawing.Size(1000, 56);
            this.mViewAngleTrack.TabIndex = 1;
            this.mViewAngleTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.mViewAngleTrack.Scroll += new System.EventHandler(this.mViewAngleTrack_Scroll);
            this.mViewAngleTrack.ValueChanged += new System.EventHandler(this.mViewAngleTrack_ValueChanged);
            this.mViewAngleTrack.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mViewAngleTrack_KeyDown);
            // 
            // mArcSweepAngleTrack
            // 
            this.mArcSweepAngleTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mArcSweepAngleTrack.Location = new System.Drawing.Point(1028, 97);
            this.mArcSweepAngleTrack.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mArcSweepAngleTrack.Maximum = 180;
            this.mArcSweepAngleTrack.Name = "mArcSweepAngleTrack";
            this.mArcSweepAngleTrack.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mArcSweepAngleTrack.Size = new System.Drawing.Size(56, 199);
            this.mArcSweepAngleTrack.TabIndex = 2;
            this.mArcSweepAngleTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.mArcSweepAngleTrack.Value = 180;
            this.mArcSweepAngleTrack.Scroll += new System.EventHandler(this.mArcSweepAngleTrack_Scroll);
            // 
            // mArcCheckBox
            // 
            this.mArcCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mArcCheckBox.AutoSize = true;
            this.mArcCheckBox.Location = new System.Drawing.Point(1030, 15);
            this.mArcCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mArcCheckBox.Name = "mArcCheckBox";
            this.mArcCheckBox.Size = new System.Drawing.Size(51, 21);
            this.mArcCheckBox.TabIndex = 3;
            this.mArcCheckBox.Text = "Arc";
            this.mArcCheckBox.UseVisualStyleBackColor = true;
            this.mArcCheckBox.CheckedChanged += new System.EventHandler(this.mArcCheckBox_CheckedChanged);
            // 
            // mPointSizeTextBox
            // 
            this.mPointSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mPointSizeTextBox.Location = new System.Drawing.Point(1044, 729);
            this.mPointSizeTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mPointSizeTextBox.Name = "mPointSizeTextBox";
            this.mPointSizeTextBox.Size = new System.Drawing.Size(36, 22);
            this.mPointSizeTextBox.TabIndex = 5;
            this.mPointSizeTextBox.Text = "5";
            this.mPointSizeTextBox.TextChanged += new System.EventHandler(this.mPointSizeTextBox_TextChanged);
            // 
            // mLineResolutionTrack
            // 
            this.mLineResolutionTrack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mLineResolutionTrack.Location = new System.Drawing.Point(17, 615);
            this.mLineResolutionTrack.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mLineResolutionTrack.Maximum = 1000;
            this.mLineResolutionTrack.Minimum = 1;
            this.mLineResolutionTrack.Name = "mLineResolutionTrack";
            this.mLineResolutionTrack.Size = new System.Drawing.Size(644, 56);
            this.mLineResolutionTrack.TabIndex = 6;
            this.mLineResolutionTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.mLineResolutionTrack.Value = 100;
            this.mLineResolutionTrack.Scroll += new System.EventHandler(this.mLineResolutionTrack_Scroll);
            // 
            // mNormalRadioButton
            // 
            this.mNormalRadioButton.AutoSize = true;
            this.mNormalRadioButton.Location = new System.Drawing.Point(12, 52);
            this.mNormalRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mNormalRadioButton.Name = "mNormalRadioButton";
            this.mNormalRadioButton.Size = new System.Drawing.Size(74, 21);
            this.mNormalRadioButton.TabIndex = 8;
            this.mNormalRadioButton.TabStop = true;
            this.mNormalRadioButton.Text = "Normal";
            this.mNormalRadioButton.UseVisualStyleBackColor = true;
            this.mNormalRadioButton.CheckedChanged += new System.EventHandler(this.mViewModeRadioButton_CheckedChanged);
            // 
            // mRedBlueRadioButton
            // 
            this.mRedBlueRadioButton.AutoSize = true;
            this.mRedBlueRadioButton.Location = new System.Drawing.Point(12, 80);
            this.mRedBlueRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mRedBlueRadioButton.Name = "mRedBlueRadioButton";
            this.mRedBlueRadioButton.Size = new System.Drawing.Size(87, 21);
            this.mRedBlueRadioButton.TabIndex = 9;
            this.mRedBlueRadioButton.Text = "Red/Blue";
            this.mRedBlueRadioButton.UseVisualStyleBackColor = true;
            this.mRedBlueRadioButton.CheckedChanged += new System.EventHandler(this.mViewModeRadioButton_CheckedChanged);
            // 
            // mStereoscopicRadioButton
            // 
            this.mStereoscopicRadioButton.AutoSize = true;
            this.mStereoscopicRadioButton.Location = new System.Drawing.Point(12, 108);
            this.mStereoscopicRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mStereoscopicRadioButton.Name = "mStereoscopicRadioButton";
            this.mStereoscopicRadioButton.Size = new System.Drawing.Size(111, 21);
            this.mStereoscopicRadioButton.TabIndex = 10;
            this.mStereoscopicRadioButton.Text = "Stereoscopic";
            this.mStereoscopicRadioButton.UseVisualStyleBackColor = true;
            this.mStereoscopicRadioButton.CheckedChanged += new System.EventHandler(this.mViewModeRadioButton_CheckedChanged);
            // 
            // mPrintRadioButton
            // 
            this.mPrintRadioButton.AutoSize = true;
            this.mPrintRadioButton.Location = new System.Drawing.Point(12, 135);
            this.mPrintRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mPrintRadioButton.Name = "mPrintRadioButton";
            this.mPrintRadioButton.Size = new System.Drawing.Size(58, 21);
            this.mPrintRadioButton.TabIndex = 11;
            this.mPrintRadioButton.TabStop = true;
            this.mPrintRadioButton.Text = "Print";
            this.mPrintRadioButton.UseVisualStyleBackColor = true;
            this.mPrintRadioButton.CheckedChanged += new System.EventHandler(this.mViewModeRadioButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(939, 734);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Point dia :";
            // 
            // mDebugCheckBox
            // 
            this.mDebugCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mDebugCheckBox.AutoSize = true;
            this.mDebugCheckBox.Location = new System.Drawing.Point(1031, 43);
            this.mDebugCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mDebugCheckBox.Name = "mDebugCheckBox";
            this.mDebugCheckBox.Size = new System.Drawing.Size(72, 21);
            this.mDebugCheckBox.TabIndex = 15;
            this.mDebugCheckBox.Text = "Debug";
            this.mDebugCheckBox.UseVisualStyleBackColor = true;
            this.mDebugCheckBox.CheckedChanged += new System.EventHandler(this.mDebugCheckBox_CheckedChanged);
            // 
            // zf
            // 
            this.zf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.zf.Location = new System.Drawing.Point(1032, 454);
            this.zf.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zf.Name = "zf";
            this.zf.Size = new System.Drawing.Size(55, 22);
            this.zf.TabIndex = 21;
            this.zf.Text = "25";
            this.zf.TextChanged += new System.EventHandler(this.zf_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1028, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 17);
            this.label2.TabIndex = 23;
            this.label2.Text = "Arcs:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1025, 305);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 17);
            this.label3.TabIndex = 24;
            this.label3.Text = "Zf:";
            // 
            // zfTrackBar
            // 
            this.zfTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zfTrackBar.Location = new System.Drawing.Point(1032, 325);
            this.zfTrackBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zfTrackBar.Maximum = 10000;
            this.zfTrackBar.Name = "zfTrackBar";
            this.zfTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.zfTrackBar.Size = new System.Drawing.Size(56, 122);
            this.zfTrackBar.TabIndex = 25;
            this.zfTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.zfTrackBar.Value = 2500;
            this.zfTrackBar.Scroll += new System.EventHandler(this.zfTrackBar_Scroll);
            // 
            // mSwitchCheckBox
            // 
            this.mSwitchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mSwitchCheckBox.AutoSize = true;
            this.mSwitchCheckBox.Location = new System.Drawing.Point(953, 674);
            this.mSwitchCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mSwitchCheckBox.Name = "mSwitchCheckBox";
            this.mSwitchCheckBox.Size = new System.Drawing.Size(135, 21);
            this.mSwitchCheckBox.TabIndex = 26;
            this.mSwitchCheckBox.Text = "Switch Left/Right";
            this.mSwitchCheckBox.UseVisualStyleBackColor = true;
            this.mSwitchCheckBox.CheckedChanged += new System.EventHandler(this.mSwitchCheckBox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1029, 487);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 17);
            this.label4.TabIndex = 27;
            this.label4.Text = "Eyes:";
            // 
            // mEyesTrackBar
            // 
            this.mEyesTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mEyesTrackBar.Location = new System.Drawing.Point(1032, 510);
            this.mEyesTrackBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mEyesTrackBar.Maximum = 40;
            this.mEyesTrackBar.Name = "mEyesTrackBar";
            this.mEyesTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mEyesTrackBar.Size = new System.Drawing.Size(56, 158);
            this.mEyesTrackBar.TabIndex = 28;
            this.mEyesTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.mEyesTrackBar.Value = 10;
            this.mEyesTrackBar.Scroll += new System.EventHandler(this.mEyesTrackBar_Scroll);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.mDarkRadioButton);
            this.groupBox1.Controls.Add(this.mNormalRadioButton);
            this.groupBox1.Controls.Add(this.mRedBlueRadioButton);
            this.groupBox1.Controls.Add(this.mStereoscopicRadioButton);
            this.groupBox1.Controls.Add(this.mPrintRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(668, 623);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(129, 171);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // mDarkRadioButton
            // 
            this.mDarkRadioButton.AutoSize = true;
            this.mDarkRadioButton.Checked = true;
            this.mDarkRadioButton.Location = new System.Drawing.Point(12, 23);
            this.mDarkRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mDarkRadioButton.Name = "mDarkRadioButton";
            this.mDarkRadioButton.Size = new System.Drawing.Size(59, 21);
            this.mDarkRadioButton.TabIndex = 7;
            this.mDarkRadioButton.TabStop = true;
            this.mDarkRadioButton.Text = "Dark";
            this.mDarkRadioButton.UseVisualStyleBackColor = true;
            this.mDarkRadioButton.CheckedChanged += new System.EventHandler(this.mViewModeRadioButton_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.mMergeFacesCheckBox);
            this.groupBox2.Controls.Add(this.quickModeCheckBox);
            this.groupBox2.Controls.Add(this.mHiddenLineCheckBox);
            this.groupBox2.Controls.Add(this.mVectorsCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(807, 655);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(124, 139);
            this.groupBox2.TabIndex = 31;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "View";
            // 
            // mMergeFacesCheckBox
            // 
            this.mMergeFacesCheckBox.AutoSize = true;
            this.mMergeFacesCheckBox.Checked = true;
            this.mMergeFacesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mMergeFacesCheckBox.Location = new System.Drawing.Point(9, 71);
            this.mMergeFacesCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mMergeFacesCheckBox.Name = "mMergeFacesCheckBox";
            this.mMergeFacesCheckBox.Size = new System.Drawing.Size(112, 21);
            this.mMergeFacesCheckBox.TabIndex = 39;
            this.mMergeFacesCheckBox.Text = "Merge Faces";
            this.mMergeFacesCheckBox.UseVisualStyleBackColor = true;
            this.mMergeFacesCheckBox.CheckedChanged += new System.EventHandler(this.mMergeFacesCheckBox_CheckedChanged);
            // 
            // quickModeCheckBox
            // 
            this.quickModeCheckBox.AutoSize = true;
            this.quickModeCheckBox.Checked = true;
            this.quickModeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.quickModeCheckBox.Location = new System.Drawing.Point(9, 97);
            this.quickModeCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.quickModeCheckBox.Name = "quickModeCheckBox";
            this.quickModeCheckBox.Size = new System.Drawing.Size(105, 21);
            this.quickModeCheckBox.TabIndex = 23;
            this.quickModeCheckBox.Text = "Quick Mode";
            this.quickModeCheckBox.UseVisualStyleBackColor = true;
            this.quickModeCheckBox.CheckedChanged += new System.EventHandler(this.quickModeCheckBox_CheckedChanged);
            // 
            // mHiddenLineCheckBox
            // 
            this.mHiddenLineCheckBox.AutoSize = true;
            this.mHiddenLineCheckBox.Checked = true;
            this.mHiddenLineCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mHiddenLineCheckBox.Location = new System.Drawing.Point(9, 20);
            this.mHiddenLineCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mHiddenLineCheckBox.Name = "mHiddenLineCheckBox";
            this.mHiddenLineCheckBox.Size = new System.Drawing.Size(106, 21);
            this.mHiddenLineCheckBox.TabIndex = 3;
            this.mHiddenLineCheckBox.Text = "Hidden Line";
            this.mHiddenLineCheckBox.UseVisualStyleBackColor = true;
            this.mHiddenLineCheckBox.CheckedChanged += new System.EventHandler(this.mHiddenLineCheckBox_CheckedChanged);
            // 
            // mVectorsCheckBox
            // 
            this.mVectorsCheckBox.AutoSize = true;
            this.mVectorsCheckBox.Checked = true;
            this.mVectorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mVectorsCheckBox.Location = new System.Drawing.Point(9, 46);
            this.mVectorsCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mVectorsCheckBox.Name = "mVectorsCheckBox";
            this.mVectorsCheckBox.Size = new System.Drawing.Size(78, 21);
            this.mVectorsCheckBox.TabIndex = 2;
            this.mVectorsCheckBox.Text = "Vectors";
            this.mVectorsCheckBox.UseVisualStyleBackColor = true;
            this.mVectorsCheckBox.CheckedChanged += new System.EventHandler(this.mVectorsCheckBox_CheckedChanged);
            // 
            // mSwitchBackFrontCheckBox
            // 
            this.mSwitchBackFrontCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mSwitchBackFrontCheckBox.AutoSize = true;
            this.mSwitchBackFrontCheckBox.Location = new System.Drawing.Point(953, 702);
            this.mSwitchBackFrontCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mSwitchBackFrontCheckBox.Name = "mSwitchBackFrontCheckBox";
            this.mSwitchBackFrontCheckBox.Size = new System.Drawing.Size(135, 21);
            this.mSwitchBackFrontCheckBox.TabIndex = 32;
            this.mSwitchBackFrontCheckBox.Text = "Swith Back/Front";
            this.mSwitchBackFrontCheckBox.UseVisualStyleBackColor = true;
            this.mSwitchBackFrontCheckBox.CheckedChanged += new System.EventHandler(this.mSwitchBackFrontCheckBox_CheckedChanged);
            // 
            // mOpenFileDialog
            // 
            this.mOpenFileDialog.Filter = "(*.x3d)|*.x3d|All files (*.*)|*.*\"";
            this.mOpenFileDialog.InitialDirectory = "c:\\Program Files\\Blender Foundation\\Blender";
            this.mOpenFileDialog.Title = "Select an x3d file";
            this.mOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.mOpenFileDialog_FileOk);
            // 
            // mOpenButton
            // 
            this.mOpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mOpenButton.Location = new System.Drawing.Point(17, 644);
            this.mOpenButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mOpenButton.Name = "mOpenButton";
            this.mOpenButton.Size = new System.Drawing.Size(100, 28);
            this.mOpenButton.TabIndex = 33;
            this.mOpenButton.Text = "Open";
            this.mOpenButton.UseVisualStyleBackColor = true;
            this.mOpenButton.Click += new System.EventHandler(this.mOpenButton_Click);
            // 
            // mFilesComboBox
            // 
            this.mFilesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mFilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mFilesComboBox.FormattingEnabled = true;
            this.mFilesComboBox.Location = new System.Drawing.Point(128, 644);
            this.mFilesComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mFilesComboBox.Name = "mFilesComboBox";
            this.mFilesComboBox.Size = new System.Drawing.Size(204, 24);
            this.mFilesComboBox.TabIndex = 34;
            this.mFilesComboBox.SelectedIndexChanged += new System.EventHandler(this.mFilesComboBox_SelectedIndexChanged);
            // 
            // mGenerateButton
            // 
            this.mGenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mGenerateButton.Location = new System.Drawing.Point(17, 679);
            this.mGenerateButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mGenerateButton.Name = "mGenerateButton";
            this.mGenerateButton.Size = new System.Drawing.Size(156, 28);
            this.mGenerateButton.TabIndex = 35;
            this.mGenerateButton.Text = "Generate GCODE";
            this.mGenerateButton.UseVisualStyleBackColor = true;
            this.mGenerateButton.Click += new System.EventHandler(this.mGenerateButton_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.Location = new System.Drawing.Point(341, 677);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(308, 127);
            this.txtOutput.TabIndex = 36;
            // 
            // mGcodeCheckBox
            // 
            this.mGcodeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mGcodeCheckBox.AutoSize = true;
            this.mGcodeCheckBox.Checked = true;
            this.mGcodeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mGcodeCheckBox.Location = new System.Drawing.Point(181, 685);
            this.mGcodeCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mGcodeCheckBox.Name = "mGcodeCheckBox";
            this.mGcodeCheckBox.Size = new System.Drawing.Size(127, 21);
            this.mGcodeCheckBox.TabIndex = 37;
            this.mGcodeCheckBox.Text = "compute gcode";
            this.mGcodeCheckBox.UseVisualStyleBackColor = true;
            this.mGcodeCheckBox.Visible = false;
            this.mGcodeCheckBox.CheckedChanged += new System.EventHandler(this.mGcodeCheckBox_CheckedChanged);
            // 
            // txtOutputSummary
            // 
            this.txtOutputSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputSummary.Enabled = false;
            this.txtOutputSummary.Location = new System.Drawing.Point(341, 647);
            this.txtOutputSummary.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtOutputSummary.Name = "txtOutputSummary";
            this.txtOutputSummary.ReadOnly = true;
            this.txtOutputSummary.Size = new System.Drawing.Size(308, 22);
            this.txtOutputSummary.TabIndex = 38;
            // 
            // mView
            // 
            this.mView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mView.ArcAngleResolution = 10F;
            this.mView.ArcSweepAngle = 90F;
            this.mView.BackColor = System.Drawing.Color.White;
            this.mView.Location = new System.Drawing.Point(17, 78);
            this.mView.Margin = new System.Windows.Forms.Padding(5);
            this.mView.Name = "mView";
            this.mView.PaddingPercent = 0.1D;
            this.mView.PointWidth = 5D;
            this.mView.RotateCanvas = false;
            this.mView.ShowArcs = true;
            this.mView.ShowGCode = false;
            this.mView.Size = new System.Drawing.Size(1000, 530);
            this.mView.StereoscopicDisparityAngle = 8D;
            this.mView.StereoscopicMode = ScratchUtility.StereoscopicMode.NonStereoscopic;
            this.mView.SwitchBackFront = false;
            this.mView.SwitchLeftRight = false;
            this.mView.TabIndex = 29;
            this.mView.VectorMode = true;
            this.mView.ViewAngle = 0D;
            this.mView.ViewMode = ViewSupport.ViewMode.Dark;
            this.mView.ViewPointsPerUnitLength = 0D;
            this.mView.VisibilityMode = ViewSupport.VisibilityMode.Transparent;
            // 
            // ScratchTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 809);
            this.Controls.Add(this.txtOutputSummary);
            this.Controls.Add(this.mGcodeCheckBox);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.mGenerateButton);
            this.Controls.Add(this.mFilesComboBox);
            this.Controls.Add(this.mOpenButton);
            this.Controls.Add(this.mSwitchBackFrontCheckBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mView);
            this.Controls.Add(this.mEyesTrackBar);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.mSwitchCheckBox);
            this.Controls.Add(this.zfTrackBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.zf);
            this.Controls.Add(this.mDebugCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mLineResolutionTrack);
            this.Controls.Add(this.mPointSizeTextBox);
            this.Controls.Add(this.mArcCheckBox);
            this.Controls.Add(this.mArcSweepAngleTrack);
            this.Controls.Add(this.mViewAngleTrack);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ScratchTest";
            this.Text = "Scratch Test";
            ((System.ComponentModel.ISupportInitialize)(this.mViewAngleTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mArcSweepAngleTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mLineResolutionTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zfTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mEyesTrackBar)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar mViewAngleTrack;
        private System.Windows.Forms.TrackBar mArcSweepAngleTrack;
        private System.Windows.Forms.CheckBox mArcCheckBox;
        private System.Windows.Forms.TextBox mPointSizeTextBox;
        private System.Windows.Forms.TrackBar mLineResolutionTrack;
        private System.Windows.Forms.RadioButton mNormalRadioButton;
        private System.Windows.Forms.RadioButton mRedBlueRadioButton;
        private System.Windows.Forms.RadioButton mStereoscopicRadioButton;
        private System.Windows.Forms.RadioButton mPrintRadioButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox mDebugCheckBox;
        private System.Windows.Forms.TextBox zf;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar zfTrackBar;
        private System.Windows.Forms.CheckBox mSwitchCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar mEyesTrackBar;
        private ScratchView.View mView;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox mSwitchBackFrontCheckBox;
        private System.Windows.Forms.OpenFileDialog mOpenFileDialog;
        private System.Windows.Forms.Button mOpenButton;
        private System.Windows.Forms.ComboBox mFilesComboBox;
        private System.Windows.Forms.CheckBox mVectorsCheckBox;
        private System.Windows.Forms.CheckBox mHiddenLineCheckBox;
        private System.Windows.Forms.CheckBox quickModeCheckBox;
        private System.Windows.Forms.Button mGenerateButton;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.CheckBox mGcodeCheckBox;
        private System.Windows.Forms.TextBox txtOutputSummary;
        private System.Windows.Forms.RadioButton mDarkRadioButton;
        private System.Windows.Forms.CheckBox mMergeFacesCheckBox;
    }
}

