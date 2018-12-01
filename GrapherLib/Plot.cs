using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GrapherLib
{
    /// <summary>
    /// Basic plot info
    /// </summary>
    public class Plot
    {
        #region Properies
        public float Step
        {
            get { return step; }
        }
        public float NumberOfPoints
        {
            get { return nOfPoints; }
        }
        public float XMax
        {
            get { return xMax; }
        }
        public float XMin
        {
            get { return xMin; }
        }
        public float YMax
        {
            get { return yMax; }
        }
        public float TMax
        {
            get { return tMax; }
        }
        public float TMin
        {
            get { return tMin; }
        }
        public float YMin
        {
            get { return yMin; }
        }
        public PointF[] RealPoints
        {
            get { return points.ToArray(); }
        }
        public float[] RootPoints
        {
            get { return rootPoints.ToArray(); }
        }
        public float[] NaNPoints
        {
            get { return naNPoints.ToArray(); }
        }
        public PlotFunction Function
        {
            get { return function; }
        }
        public PlotFunction XTFunction
        {
            get { return xfunc; }
        }
        public PlotFunction YTFunction
        {
            get { return yfunc; }
        }
        public bool IsCycloid
        {
            get { return isCycloid; }
        }
        #endregion

        PlotFunction function , xfunc, yfunc;
        float xMax, xMin, yMax, yMin, step, nOfPoints, tMin, tMax;
        bool isCycloid = false;

        List<PointF> points = new List<PointF>();
        List<float> naNPoints = new List<float>();
        List<float> rootPoints = new List<float>();
        List<List<PointF>> multiPoints = new List<List<PointF>>();
        ProgressBar pBar;

        /// <summary>
        /// Creates function plot information
        /// </summary>
        /// <param name="function">Function delegate</param>
        /// <param name="xMin"></param>
        /// <param name="xMax"></param>
        /// <param name="step"></param>
        public Plot(PlotFunction function, float xMin, float xMax, float step)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.step = step;
            this.function = function;
            Build(function);
        }
        public Plot(PlotFunction function, float xMin, float xMax, float step, ProgressBar bar)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.step = step;
            this.function = function;
            pBar = bar;
            Build(function);
        }
        public Plot(PlotFunction XTFunc, PlotFunction YTFunc, float tMin, float tMax, float step)
        {
            isCycloid = true;
            xfunc = XTFunc;
            yfunc = YTFunc;

            this.tMax = tMax;
            this.tMin = tMin;

            Plot tX = new Plot(XTFunc, tMin, tMax, step);
            Plot tY = new Plot(YTFunc, tMin, tMax, step);

            this.step = step;

            List<PointF> points = new List<PointF>();

            List<PointF> tx = new List<PointF>(tX.RealPoints);
            List<PointF> ty = new List<PointF>(tY.RealPoints);

            for (int i = 0; i < tx.Count; i++)
                points.Add(new PointF(tx[i].Y, ty[i].Y));

            this.points = points;
            CountMultipliers();

        }
        public Plot(PlotFunction XTFunc, PlotFunction YTFunc, float tMin, float tMax, float step , ProgressBar bar)
        {
            pBar = bar;
            isCycloid = true;
            xfunc = XTFunc;
            yfunc = YTFunc;

            this.tMax = tMax;
            this.tMin = tMin;

            Plot tX = new Plot(XTFunc, tMin, tMax, step, bar);
            Plot tY = new Plot(YTFunc, tMin, tMax, step, bar);

            this.step = step;

            List<PointF> points = new List<PointF>();

            List<PointF> tx = new List<PointF>(tX.RealPoints);
            List<PointF> ty = new List<PointF>(tY.RealPoints);

            for (int i = 0; i < tx.Count; i++)
                points.Add(new PointF(tx[i].Y, ty[i].Y));

            this.points = points;
            CountMultipliers();

        }

        void CountMultipliers()
        {
            yMax = float.NegativeInfinity;
            yMin = float.PositiveInfinity;
            xMin = float.PositiveInfinity;
            xMax = float.NegativeInfinity;

            foreach (var point in points)
            {
                yMax = point.Y > yMax ? point.Y : yMax;
                yMin = point.Y < yMin ? point.Y : yMin;

                xMax = point.X > xMax ? point.X : xMax;
                xMin = point.X < xMin ? point.X : xMin;
            }
        }
        /// <summary>
        /// Builds function points
        /// </summary>
        /// <param name="func">Function delegate</param>
        protected void Build(PlotFunction func)
        {
            #region building real points
            step = Math.Abs(step);
            nOfPoints = (float)Math.Floor((xMax - xMin) / step);
            if (nOfPoints > 1000000)
                throw new Exception(string.Format("Number of points is too big:{0}", nOfPoints));
            else if (nOfPoints < 2)
                throw new Exception(string.Format("No points in given interval: XMin {0}, XMax {1}", xMin,xMax));
            else if (float.IsNaN(nOfPoints))
                throw new Exception(string.Format("Invalid input: XMin {0}, XMax {1}, step {2}", xMin, xMax, step));

            yMax = float.NegativeInfinity;
            yMin = float.PositiveInfinity;

            if (pBar != null)
            {
                pBar.Value = 0;
                pBar.Step = 1;
                pBar.Maximum = (int)Math.Ceiling(nOfPoints);
                pBar.Minimum = 0;
            }

           
            if (!(xMin <= 0 && xMax > 0))
            {
                for (decimal i = (decimal)xMin; i < (decimal)xMax; i += (decimal)step)
                {
                    float value = func.Invoke((float)i);
                    if (!float.IsNaN(value) && !float.IsInfinity(value))
                    {
                        PointF point = new PointF((float)i, value);
                        points.Add(point);

                        yMax = point.Y > yMax ? point.Y : yMax;
                        yMin = point.Y < yMin ? point.Y : yMin;
                    }
                    else
                        naNPoints.Add((float)i);

                   // if (pBar != null)
                   // {
                      //  pBar.PerformStep();
                    //}
                }
            }
            else
            {
                for (decimal i = 0; i > (decimal)xMin; i -= (decimal)step)
                {
                    float value = func.Invoke((float)i);
                    if (!float.IsNaN(value) && !float.IsInfinity(value))
                    {
                        PointF point = new PointF((float)i, value);
                        points.Add(point);

                        yMax = point.Y > yMax ? point.Y : yMax;
                        yMin = point.Y < yMin ? point.Y : yMin;
                    }
                    else
                    {
                        if(!naNPoints.Contains((float)i))
                        naNPoints.Add((float)i);
                    }
                    //if (pBar != null)
                      //  pBar.PerformStep();
                }
                points.Reverse();
                for (decimal i = 0; i < (decimal)xMax; i += (decimal)step)
                {
                    float value = func.Invoke((float)i);
                    if (!float.IsNaN(value) && !float.IsInfinity(value))
                    {
                        PointF point = new PointF((float)i, value);
                        points.Add(point);

                        yMax = point.Y > yMax ? point.Y : yMax;
                        yMin = point.Y < yMin ? point.Y : yMin;
                    }
                    else
                    {
                        if (!naNPoints.Contains((float)i))
                            naNPoints.Add((float)i);
                    }
                   // if (pBar != null)
                    //    pBar.PerformStep();
                }
            }
            if (points.Count < 2)
                throw new Exception(string.Format("No points in given interval: XMin {0}, XMax {1}", xMin, xMax));

            //if(!isCycloid)
            for (decimal i = (decimal)xMin; i < (decimal)xMax; i += (decimal)step)
            {
                decimal dxMin = i;
                decimal dxMax = i + (decimal)step;
                bool incr = function.Invoke((float)dxMin) < function.Invoke((float)dxMax);

                if (float.IsInfinity(function.Invoke((float)dxMin)) || float.IsInfinity(function.Invoke((float)dxMax)) || float.IsNaN(function.Invoke((float)dxMax)))
                    continue;
                if (function.Invoke((float)dxMin) * function.Invoke((float)dxMax) <= 0)
                {
                            
                        decimal eps = 1E-08M;
                        decimal c = 0;
                        while (Math.Abs(dxMax - dxMin) > eps)
                        {
                            c = (dxMin + dxMax) / 2;
                            float res = function.Invoke((float)c);
                            if (res == 0)
                            break;
                                if (incr)
                                {
                                    if (res > 0)//left side
                                        dxMax = c;
                                    else if (res < 0) //right side
                                        dxMin = c;
                                }
                                else
                                {
                                    if (res > 0)//right side
                                        dxMin = c;
                                    else if (res < 0) //left side
                                        dxMax = c;
                                }
                         }
                    rootPoints.Add((float)c);
                }

            }
            if (pBar != null)
                pBar.Value = 0;
            #endregion
        }
        /// <summary>
        /// Returns function value in X
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float GetY(float x)
        {
            if (!isCycloid)
                return function.Invoke(x);
            else
                return 0;
        }
    }
    public class MultiPlot
    {
        public List<Plot> Plots
        {
            get { return plots; }
        }
        public PlotFunction[] Functions
        {
            get { return functions; }
        }

        List<Plot> plots = new List<Plot>();
        PlotFunction[] functions;

        public MultiPlot (float xMin, float xMax, float step, params PlotFunction[] func)
        {
            foreach (var function in func)
                plots.Add(new Plot(function, xMin, xMax, step));
            functions = new PlotFunction[func.Length];
            func.CopyTo(functions,0);
        }
        public MultiPlot(float xMin, float xMax, float step, ProgressBar bar, params PlotFunction[] func)
        {
            foreach (var function in func)
                plots.Add(new Plot(function, xMin, xMax, step,bar));

            functions = new PlotFunction[func.Length];
            func.CopyTo(functions, 0);
        }
    }

}
