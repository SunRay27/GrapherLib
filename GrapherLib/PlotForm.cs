using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GrapherLib
{
    /// <summary>
    /// Quick PlotDrawer form
    /// </summary>
    public class PlotForm : Form
    {
        public PictureBox PicBox
        {
            get { return picBox; }
        }

        PictureBox handle = new PictureBox();
        protected PictureBox picBox;
        private Label dotCoord;
        private Button openButton;
        private Panel panel1;
        private Button rebuildButton;
        private TextBox stepBox;
        private TextBox xMaxBox;
        private TextBox xMinBox;
        private TextBox infoBox;
        private Panel panel2;
        private TextBox xBox;
        private TextBox yBox;
        private Label paramMin;
        private Label paramMax;
        private Button button1;
        private Button functionsButton;
        private ProgressBar progressBar;
        private Label exceptionLabel;

        string details = "None";
        PlotDrawer drawer;

       
        public PlotForm(string name, Size size, Plot plot, Color main, Color foreColor, Color bg, string details = "None")
        {
            InitializeComponent();
            Text = name;
            Size = size;
            drawer = new PlotDrawer(plot, picBox, main, foreColor, bg);
            CreateFormElements();
            Start();
        }
        public PlotForm(string name, Size size, Plot plot, string details = "None")
        {
            InitializeComponent();
            Text = name;
            Size = size;
            drawer = new PlotDrawer(plot, picBox);
            CreateFormElements();
            Start();
        }
        public PlotForm(string name, Size size, MultiPlot multiPlot, string details = "None")
        {
            InitializeComponent();
            Text = name;
            this.details = details;
            Size = size;
            drawer = new PlotDrawer(picBox, multiPlot);
            CreateFormElements();
            Start();
        }

        void Start()
        {
            if (!drawer.Plot.IsCycloid)
            {
                xMinBox.Text = drawer.Plot.XMin.ToString();
                xMaxBox.Text = drawer.Plot.XMax.ToString();
                stepBox.Text = drawer.Plot.Step.ToString();


            }
            else
            {
                paramMin.Text = "TMin: ";
                paramMax.Text = "TMax: ";
                xMinBox.Text = drawer.Plot.TMin.ToString();
                xMaxBox.Text = drawer.Plot.TMax.ToString();
                stepBox.Text = drawer.Plot.Step.ToString();
            }
            try
            {
                picBox.Image = drawer.Draw();
            }
            catch (Exception ex)
            {
                panel1.Show();
                ShowException(ex);
            }
            HideException();

            UpdateInfo();
            Show();
        }
        void UpdateInfo()
        {
            infoBox.Text = $"XMin:{drawer.Plot.XMin}\r\nXMax:{drawer.Plot.XMax}\r\nYMin:{drawer.Plot.YMin}\r\nYMax:{drawer.Plot.YMax}\r\nPoints: {drawer.Plot.RealPoints.Length}\r\n Details:\n{details}\r\n Function zeros:\r\n";
            int c = 0;
            foreach (var item in drawer.Plot.RootPoints)
            {
                infoBox.Text += $"X{c}:{item}\r\n";
                c++;
            }
        }
        void CreateFormElements()
        {
            // if cycloid plot
            if (drawer.Plot.IsCycloid)
            {
                xBox.Text = "0";
                xBox.Enabled = false;
                yBox.Enabled = false;
            }
            else // if Single/multi plot
            {
                //create dot
                Controls.Add(handle);
                handle.Size = new Size(6, 6);
                handle.BackColor = Color.Transparent;
                handle.Parent = picBox;
                handle.Enabled = false;
                picBox.BackColor = Color.Transparent;

                Bitmap bmp = new Bitmap(handle.Size.Width, handle.Size.Height);
                Graphics g = Graphics.FromImage(bmp);
                MemoryStream stream = new MemoryStream();
                g.FillEllipse(Brushes.Red, 0, 0, handle.Size.Width, handle.Size.Height);
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                handle.Image = Image.FromStream(stream);

                picBox.MouseMove += OnMouseHover;
            }
            
            dotCoord.BackColor = Color.Transparent;
            dotCoord.Parent = picBox;
           // functionsPanel.Hide();
            panel1.Hide();
            HideException();
            ClientSizeChanged += OnSizeChanged;
        }
        //Exceptions
        void ShowException(Exception ex)
        { 
            exceptionLabel.Text = "Exception: " + ex.Message;
            exceptionLabel.ForeColor = Color.DarkRed;
        }
        void HideException()
        {
            exceptionLabel.Text = "Done!";
            exceptionLabel.ForeColor = Color.Blue;
        }
        //Events
        private void RebuildButtonPress(object sender, EventArgs e)
        {
            
            bool invalid = false;
            Plot newPlot;
            MultiPlot newMultiPlot;

            xMinBox.Text = xMinBox.Text.Replace('.', ',');
            xMaxBox.Text = xMaxBox.Text.Replace('.', ',');
            stepBox.Text = stepBox.Text.Replace('.', ',');

            float xMin = 0,xMax = 0,step = 0;
            #region Check input
            try
            {
                xMin = float.Parse(xMinBox.Text);
                xMinBox.BackColor = Color.White;
            }
            catch
            {
                xMinBox.BackColor = Color.IndianRed;
                invalid = true;
            }
            try
            {
                xMax = float.Parse(xMaxBox.Text);
                xMaxBox.BackColor = Color.White;
            }
            catch
            {
                xMaxBox.BackColor = Color.IndianRed;
                invalid = true;
            }
            try
            {
                step = float.Parse(stepBox.Text);
                stepBox.BackColor = Color.White;
            }
            catch
            {
                stepBox.BackColor = Color.IndianRed;
                invalid = true;
            }
            if (invalid)
                return;
            #endregion

            try
            {
                if (xMin != drawer.Plot.XMin || xMax != drawer.Plot.XMax || step != drawer.Plot.Step)
                {
                    if (!drawer.IsMulti && !drawer.Plot.IsCycloid) // ReBuild simple plot
                    {
                        newPlot = new Plot(drawer.Plot.Function, xMin, xMax, step, progressBar);
                        drawer = new PlotDrawer(newPlot, picBox);
                    }
                    else if (!drawer.Plot.IsCycloid) //ReBuild MultiPlot
                    {
                        newMultiPlot = new MultiPlot(xMin, xMax, step, progressBar, drawer.MultiPlot.Functions);
                        drawer = new PlotDrawer(picBox, newMultiPlot);
                    }
                    else // ReBuild cycloid
                    {
                        newPlot = new Plot(drawer.Plot.XTFunction, drawer.Plot.YTFunction, xMin, xMax, step, progressBar);
                        drawer = new PlotDrawer(newPlot, picBox);
                    }
                    Start();
                }
                else
                {
                    HideException();
                    return; //Nothing changed, no need in rebuild
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
                //Color buttons
                if (ex.Message.Contains("Number of points is too big:"))
                    stepBox.BackColor = Color.IndianRed;

                else if(ex.Message.Contains("No points in given interval"))
                {
                    xMaxBox.BackColor = Color.IndianRed;
                    xMinBox.BackColor = Color.IndianRed;
                }
                else
                {
                    stepBox.BackColor = Color.IndianRed;
                    xMaxBox.BackColor = Color.IndianRed;
                    xMinBox.BackColor = Color.IndianRed;
                }
            }
        }
        private void xBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                float x = float.Parse(xBox.Text.Replace('.',','));
                xBox.BackColor = Color.White;
                float y = drawer.Plot.GetY(x);
                if (float.IsNaN(y) || float.IsInfinity(y))
                {
                    yBox.BackColor = Color.IndianRed;
                    yBox.Text = "NaN";
                    return;
                }
                yBox.BackColor = Color.White;
                yBox.Text = y.ToString();

            }
            catch
            {
                xBox.BackColor = Color.IndianRed;
                yBox.BackColor = Color.White;
                yBox.Text = "";
            }
        }
        private void BuildButtonPress(object sender, EventArgs e)
        {
            if (panel1.Visible)
                panel1.Hide();
            else
                panel1.Show();
            openButton.Text = openButton.Text == "<" ? ">" : "<";
        }
        private void OnMouseHover(object sender, EventArgs e)
        {
            PointF localPoint = picBox.PointToClient(MousePosition);
            PointF result = drawer.FromImageX(localPoint);
            handle.Location = new Point((int)drawer.ToImageX(result.X) - handle.Size.Width / 2, (int)drawer.ToImageY(result.Y) - handle.Size.Height / 2);
            dotCoord.Text = $"X: {result.X} Y: {result.Y}";
        }
        private void OnSizeChanged(object sender, EventArgs e)
        {
            PointF localPoint = picBox.PointToClient(MousePosition);
            picBox.Image = drawer.Draw();

            if (!drawer.Plot.IsCycloid)
            {
                PointF result = drawer.FromImageX(localPoint);
                handle.Location = new Point((int)drawer.ToImageX(result.X) - handle.Size.Width / 2, (int)drawer.ToImageY(result.Y) - handle.Size.Height / 2);
            }
        }
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            this.paramMin = new System.Windows.Forms.Label();
            this.paramMax = new System.Windows.Forms.Label();
            this.infoBox = new System.Windows.Forms.TextBox();
            this.picBox = new System.Windows.Forms.PictureBox();
            this.openButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.xBox = new System.Windows.Forms.TextBox();
            this.yBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.stepBox = new System.Windows.Forms.TextBox();
            this.xMaxBox = new System.Windows.Forms.TextBox();
            this.xMinBox = new System.Windows.Forms.TextBox();
            this.rebuildButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.functionsButton = new System.Windows.Forms.Button();
            this.exceptionLabel = new System.Windows.Forms.Label();
            this.dotCoord = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.SystemColors.ButtonFace;
            label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            label2.Location = new System.Drawing.Point(9, 409);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(41, 17);
            label2.TabIndex = 4;
            label2.Text = "Step:";
            // 
            // label5
            // 
            label5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            label5.Location = new System.Drawing.Point(56, 7);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(81, 24);
            label5.TabIndex = 0;
            label5.Text = "Plot info:";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label6.AutoSize = true;
            label6.BackColor = System.Drawing.SystemColors.ButtonFace;
            label6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            label6.Location = new System.Drawing.Point(11, 377);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(21, 17);
            label6.TabIndex = 10;
            label6.Text = "X:";
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label7.AutoSize = true;
            label7.BackColor = System.Drawing.SystemColors.ButtonFace;
            label7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            label7.Location = new System.Drawing.Point(110, 377);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(21, 17);
            label7.TabIndex = 11;
            label7.Text = "Y:";
            // 
            // paramMin
            // 
            this.paramMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.paramMin.AutoSize = true;
            this.paramMin.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.paramMin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.paramMin.Location = new System.Drawing.Point(9, 428);
            this.paramMin.Name = "paramMin";
            this.paramMin.Size = new System.Drawing.Size(47, 17);
            this.paramMin.TabIndex = 5;
            this.paramMin.Text = "X Min:";
            // 
            // paramMax
            // 
            this.paramMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.paramMax.AutoSize = true;
            this.paramMax.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.paramMax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.paramMax.Location = new System.Drawing.Point(9, 447);
            this.paramMax.Name = "paramMax";
            this.paramMax.Size = new System.Drawing.Size(50, 17);
            this.paramMax.TabIndex = 6;
            this.paramMax.Text = "X Max:";
            // 
            // infoBox
            // 
            this.infoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.infoBox.Location = new System.Drawing.Point(13, 33);
            this.infoBox.Multiline = true;
            this.infoBox.Name = "infoBox";
            this.infoBox.ReadOnly = true;
            this.infoBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.infoBox.Size = new System.Drawing.Size(149, 267);
            this.infoBox.TabIndex = 1;
            this.infoBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // picBox
            // 
            this.picBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.picBox.Location = new System.Drawing.Point(0, 0);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(925, 509);
            this.picBox.TabIndex = 0;
            this.picBox.TabStop = false;
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openButton.AutoSize = true;
            this.openButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.openButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.openButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.openButton.Location = new System.Drawing.Point(923, 0);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(28, 62);
            this.openButton.TabIndex = 3;
            this.openButton.Text = "<";
            this.openButton.UseVisualStyleBackColor = false;
            this.openButton.Click += new System.EventHandler(this.BuildButtonPress);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.panel1.Controls.Add(this.progressBar);
            this.panel1.Controls.Add(label7);
            this.panel1.Controls.Add(label6);
            this.panel1.Controls.Add(this.xBox);
            this.panel1.Controls.Add(this.yBox);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.paramMax);
            this.panel1.Controls.Add(this.paramMin);
            this.panel1.Controls.Add(label2);
            this.panel1.Controls.Add(this.stepBox);
            this.panel1.Controls.Add(this.xMaxBox);
            this.panel1.Controls.Add(this.xMinBox);
            this.panel1.Controls.Add(this.rebuildButton);
            this.panel1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.panel1.Location = new System.Drawing.Point(725, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 509);
            this.panel1.TabIndex = 4;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 346);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(176, 23);
            this.progressBar.TabIndex = 12;
            // 
            // xBox
            // 
            this.xBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.xBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xBox.Location = new System.Drawing.Point(34, 375);
            this.xBox.Name = "xBox";
            this.xBox.Size = new System.Drawing.Size(58, 22);
            this.xBox.TabIndex = 9;
            this.xBox.Text = "value";
            this.xBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.xBox.TextChanged += new System.EventHandler(this.xBox_TextChanged);
            // 
            // yBox
            // 
            this.yBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.yBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.yBox.Location = new System.Drawing.Point(130, 375);
            this.yBox.Name = "yBox";
            this.yBox.ReadOnly = true;
            this.yBox.Size = new System.Drawing.Size(58, 22);
            this.yBox.TabIndex = 8;
            this.yBox.Text = "value";
            this.yBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel2.Controls.Add(this.infoBox);
            this.panel2.Controls.Add(label5);
            this.panel2.Location = new System.Drawing.Point(12, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(176, 327);
            this.panel2.TabIndex = 7;
            // 
            // stepBox
            // 
            this.stepBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stepBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stepBox.Location = new System.Drawing.Point(104, 401);
            this.stepBox.Name = "stepBox";
            this.stepBox.Size = new System.Drawing.Size(89, 22);
            this.stepBox.TabIndex = 3;
            this.stepBox.Text = "value";
            this.stepBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // xMaxBox
            // 
            this.xMaxBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.xMaxBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xMaxBox.Location = new System.Drawing.Point(104, 445);
            this.xMaxBox.Name = "xMaxBox";
            this.xMaxBox.Size = new System.Drawing.Size(89, 22);
            this.xMaxBox.TabIndex = 2;
            this.xMaxBox.Text = "value";
            this.xMaxBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // xMinBox
            // 
            this.xMinBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.xMinBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xMinBox.Location = new System.Drawing.Point(104, 423);
            this.xMinBox.Name = "xMinBox";
            this.xMinBox.Size = new System.Drawing.Size(89, 22);
            this.xMinBox.TabIndex = 1;
            this.xMinBox.Text = "value";
            this.xMinBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rebuildButton
            // 
            this.rebuildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rebuildButton.BackColor = System.Drawing.SystemColors.Control;
            this.rebuildButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rebuildButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rebuildButton.Location = new System.Drawing.Point(8, 471);
            this.rebuildButton.Name = "rebuildButton";
            this.rebuildButton.Size = new System.Drawing.Size(185, 23);
            this.rebuildButton.TabIndex = 0;
            this.rebuildButton.Text = "Rebuild";
            this.rebuildButton.UseVisualStyleBackColor = false;
            this.rebuildButton.Click += new System.EventHandler(this.RebuildButtonPress);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.AutoSize = true;
            this.button1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(923, 456);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 53);
            this.button1.TabIndex = 6;
            this.button1.Text = "<";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // functionsButton
            // 
            this.functionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.functionsButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.functionsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.functionsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.functionsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.functionsButton.Location = new System.Drawing.Point(924, 61);
            this.functionsButton.Name = "functionsButton";
            this.functionsButton.Size = new System.Drawing.Size(27, 397);
            this.functionsButton.TabIndex = 7;
            this.functionsButton.Text = "<";
            this.functionsButton.UseVisualStyleBackColor = false;
            this.functionsButton.Click += new System.EventHandler(this.functionsButton_Click);
            // 
            // exceptionLabel
            // 
            this.exceptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exceptionLabel.AutoSize = true;
            this.exceptionLabel.Location = new System.Drawing.Point(0, 510);
            this.exceptionLabel.Name = "exceptionLabel";
            this.exceptionLabel.Size = new System.Drawing.Size(77, 17);
            this.exceptionLabel.TabIndex = 9;
            this.exceptionLabel.Text = "Exception: ";
            // 
            // dotCoord
            // 
            this.dotCoord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dotCoord.AutoSize = true;
            this.dotCoord.Location = new System.Drawing.Point(0, 493);
            this.dotCoord.Name = "dotCoord";
            this.dotCoord.Size = new System.Drawing.Size(66, 17);
            this.dotCoord.TabIndex = 10;
            this.dotCoord.Text = "X: 1 Y:25";
            // 
            // PlotForm
            // 
            this.ClientSize = new System.Drawing.Size(951, 524);
            this.Controls.Add(this.dotCoord);
            this.Controls.Add(this.exceptionLabel);
            this.Controls.Add(this.functionsButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.picBox);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "PlotForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void functionsButton_Click(object sender, EventArgs e)
        {
            functionsButton.Text = functionsButton.Text == "<" ? ">" : "<";
        }
    }
}
