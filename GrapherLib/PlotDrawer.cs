using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GrapherLib
{
    /// <summary>
    /// Draws plots
    /// </summary>
    public class PlotDrawer
    {
        #region Properties
       
        public bool IsMulti
        {
            get { return isMulti; }
        }
        public PointF[] ImagePoints
        {
            get { return imagePoints; }
        }
        public Plot Plot
        {
            get { return plot; }
        }
        public MultiPlot MultiPlot
        {
            get { return multiPlot; }
        }
        /*  public Color PlotColor
          {
              get { return plotColor; }
          }
          public Color AxisColor
          {
              get { return axisColor; }
          }
          public Color BackgroundColor
          {
              get { return bgColor; }
          }*/
        #endregion
        bool isMulti = false;
        float v, h, v1, h1, kv, kh;
        float vMin, v1Min;
        int margin = 50;
        Plot plot;
        MultiPlot multiPlot;
        public List<Color> plotColor = new List<Color> {Color.Blue,Color.Red,Color.Purple,Color.Green,Color.Orange,Color.Pink,Color.Violet,Color.Indigo, Color.GreenYellow, Color.Gold, Color.Fuchsia };
        Color axisColor = Color.Gray;
        Color bgColor = Color.WhiteSmoke;
        PictureBox picBox;
        PointF[] imagePoints;

        List<PointF[]> multiImagePoints = new List<PointF[]>();

        /// <summary>
        /// Initializes simple PlotDrawer
        /// </summary>
        /// <param name="plot">Plot to draw</param>
        /// <param name="picBox">Output PictureBox</param>
        public PlotDrawer(Plot plot, PictureBox picBox)
        {
            this.picBox = picBox;
            this.plot = plot;
        }
        public PlotDrawer(PictureBox picBox, MultiPlot multiPlot)
        {
            this.picBox = picBox;
            this.multiPlot = multiPlot;
            plot = multiPlot.Plots[0];
            isMulti = true;
            //this.plot = plot;
        }


        /// <summary>
        /// Initializes PlotDrawer
        /// </summary>
        /// <param name="plot">Plot to draw</param>
        /// <param name="picBox">Output PictureBox</param>
        /// <param name="plotColor">Main color - function color</param>
        /// <param name="axisColor">Forecolor - color of axises</param>
        /// <param name="backgroundColor">Backgroud color - color of background</param>
        public PlotDrawer(Plot plot, PictureBox picBox, Color plotColor, Color axisColor, Color backgroundColor)
        {
            this.picBox = picBox;
            this.plot = plot;
            this.plotColor[0] = plotColor;
            this.axisColor = axisColor;
            bgColor = backgroundColor;
        }

        /// <summary>
        /// Builds real function points to image coordinates
        /// </summary>
        void BuildImagePoints()
        {
            imagePoints = new PointF[plot.RealPoints.Length];
            plot.RealPoints.CopyTo(imagePoints, 0);
            #region count multipliers
            float XMAX = plot.XMax;
            float XMIN = plot.XMin;
            float YMAX = plot.YMax;
            float YMIN = plot.YMin;

            float width = picBox.Width;
            float height = picBox.Height;

            h1 = (XMAX + XMIN) / 2;
            v1 = (YMAX + YMIN) / 2;

            h = (width / 2 - margin) / (XMAX - h1);
            v = (height / 2 - margin) / (YMAX - v1);
            h = h == float.PositiveInfinity ? 1 : h;
            v = v == float.PositiveInfinity ? 1 : v;
            for (int i = 0; i < imagePoints.Length; i++)
                imagePoints[i] = (new PointF((imagePoints[i].X - h1) * h + width / 2, (-imagePoints[i].Y + v1) * v + height / 2));

            kh = (width) / 2 - h1 * h ;
            if (kh < 0)
                kh = 0;
            else if (kh > width)
                kh = width;

            kv = (height) / 2 + v1 * v;
            if (kv < 0)
                kv = 0;
            else if (kv > height)
                kv = height;

            #endregion
        }

        void BuildMultiImagePoints()
        {
            //  int c = 0;
            multiImagePoints = new List<PointF[]>();
            vMin = float.PositiveInfinity;
            v1Min = 0;
            float width = picBox.Width;
            float height = picBox.Height;
            foreach (var plot in multiPlot.Plots)
            {
                float YMAX = plot.YMax;
                float YMIN = plot.YMin;
                v1 = (YMAX + YMIN) / 2;

                v = (height / 2 - margin) / (YMAX - v1);
                v = v == float.PositiveInfinity ? 1 : v;

                if(v < vMin)
                {
                    vMin = v;
                    v1Min = v1;

                    kv = (height) / 2 + v1 * v;
                    if (kv < 0)
                        kv = 0;
                    else if (kv > height)
                        kv = height;
                }
                
            }

            //  throw new Exception($"{vMin}");
            foreach (var plot in multiPlot.Plots) //TODO: умножать на наименьший множитель v из всех!
            {
                PointF[] plotImagePoints = new PointF[plot.RealPoints.Length];
                plot.RealPoints.CopyTo(plotImagePoints, 0);
                
                float XMAX = plot.XMax;
                float XMIN = plot.XMin;
                float YMAX = plot.YMax;
                float YMIN = plot.YMin;

                h1 = (XMAX + XMIN) / 2;
                v1 = (YMAX + YMIN) / 2;

                h = (width / 2 - margin) / (XMAX - h1);
                v = (height / 2 - margin) / (YMAX - vMin);
                h = h == float.PositiveInfinity ? 1 : h;
                v = v == float.PositiveInfinity ? 1 : v;
                for (int i = 0; i < plotImagePoints.Length; i++)
                    plotImagePoints[i] = (new PointF((plotImagePoints[i].X - h1) * h + width / 2, (-plotImagePoints[i].Y + v1Min) * vMin + height / 2));

                kh = (width) / 2 - h1 * h;
                if (kh < 0)
                    kh = 0;
                else if (kh > width)
                    kh = width;





                multiImagePoints.Add(plotImagePoints);
            }


            // #endregion
        }


        #region Coordinates functions

        /// <summary>
        /// Converts real X to image X
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float ToImageX(float x)
        {
            return ((x - h1) * h + (float)picBox.Width / 2);
        }

        /// <summary>
        /// Converts real Y to image Y
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public float ToImageY(float y)
        {
            if (!isMulti)
                return ((-y + v1) * v + (float)picBox.Height / 2);
            else
                return ((-y + v1Min) * vMin + (float)picBox.Height / 2);
        }

        /// <summary>
        /// Converts image X to real X
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float FromImageX(float x)
        {

            return (x - (float)picBox.Width / 2) / h + h1;
            //return 1;
        }

        /// <summary>
        /// Converts image X to real XY point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PointF FromImageX(PointF point)
        {
            float x = FromImageX(point.X);
            return new PointF(x, plot.Function.Invoke(x));
            //return new PointD();
        }
        #endregion

        void Setup(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        /// <summary>
        /// Returns plot image
        /// </summary>
        /// <returns></returns>
        public Image Draw()
        {
            if (picBox.Width == 0 || picBox.Height == 0)
                return null;
            if (!isMulti)
                BuildImagePoints();
            else
                BuildMultiImagePoints();

            Bitmap bmp = new Bitmap(picBox.Width, picBox.Height);
            Graphics g = Graphics.FromImage(bmp);
            Setup(g);

            g.Clear(bgColor);
            g.DrawLine(new Pen(axisColor, 2.5f), (float)kh, picBox.Height, (float)kh, 0);//vertical
            g.DrawLine(new Pen(axisColor, 2.5f), 0, (float)kv, picBox.Width, (float)kv);//horizontal

            

            try
            {
                if (!isMulti)
                    g.DrawLines(new Pen(plotColor[0], 2.5F), imagePoints);
                else
                {
                    int c = 0;
                foreach (var item in multiImagePoints)
                {
                        Color col = plotColor[c];
                    g.DrawLines(new Pen(col, 2.5F), item);
                        c++;
                }
            }
            }
            catch(Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

            Color transpRed = Color.FromArgb(255, Color.Red);
            int lastFloor = 0;

                foreach (var item in plot.NaNPoints)
                {
                    int floor = (int)Math.Floor(ToImageX(item));
                    if (floor == lastFloor)
                        continue;
                    g.DrawLine(new Pen(transpRed, 1.7f), floor, 0 + margin, floor, picBox.Height - margin);
                    lastFloor = floor;
                }

             if(isMulti)
            foreach (var plot in multiPlot.Plots)
            {
                    foreach (var item in plot.NaNPoints)
                    {
                        int floor = (int)Math.Floor(ToImageX(item));
                        if (floor == lastFloor)
                            continue;
                        g.DrawLine(new Pen(transpRed, 1.7f), floor, 0 + margin, floor, picBox.Height - margin);
                        lastFloor = floor;
                    }
                }
                g.DrawRectangle(new Pen(axisColor, 2.5f), new Rectangle(new Point(margin, margin), new Size(picBox.Width - margin * 2, picBox.Height - margin * 2)));

            string xmin = plot.XMin.ToString("0.##");
            string xmax = plot.XMax.ToString("0.##");
            string ymin = plot.YMin.ToString("0.##");
            string ymax = plot.YMax.ToString("0.##");

            float n = 0.01f;
            while(g.MeasureString(xmin,new Font("Arial",n,GraphicsUnit.Pixel)).Width < margin*0.7f)
                n += 0.1f;
            float m = 0.01f;
            while (g.MeasureString(xmax, new Font("Arial", m, GraphicsUnit.Pixel)).Width < margin * 0.7f)
                m += 0.1f;
            n = n > m ? m : n;
            Font font = new Font("Arial", n);
            //Font font2 = new Font("Arial", m);


            g.DrawString(xmin,font, Brushes.Gray, new PointF(0, (float)kv));
            g.DrawString(xmax, font, Brushes.Gray, new PointF(picBox.Width - margin, (float)kv));

            g.DrawString(ymax, font, Brushes.Gray, new PointF((float)kh, 0));
            g.DrawString(ymin, font, Brushes.Gray, new PointF((float)kh, picBox.Height - margin));

            foreach (var point in plot.RootPoints)
            {
                float pointX = (float)ToImageX(point);
                float size = 10;
                g.FillEllipse(new SolidBrush(Color.LimeGreen), pointX - size / 2f, (float)kv - size / 2f, size, size);
                g.DrawEllipse(new Pen(axisColor), pointX - (size - 2) / 2f, (float)kv - (size - 2) / 2f, size - 2, size - 2);

            }

            MemoryStream memory = new MemoryStream();
            bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            bmp.Dispose();
            g.Dispose();
            GC.Collect();
            return Image.FromStream(memory);
        }
    }
}
