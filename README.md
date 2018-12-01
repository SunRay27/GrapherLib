# GrapherLib
Simple library for displaying 2d plots

## Features:
- Parametrized functions
- Function zeros
- Function NAN's
- Runtime customization

## Code sample:
```cs
using GrapherLib;
namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            PlotFunction f = Function;
            Plot plot = new Plot(Function, -6, 6, 0.01f);
            PlotForm a = new PlotForm("Plot window", new System.Drawing.Size(500, 500), plot);
        }
        static float Function(float x)
        {
            return 1 / x;
        }
    }
}
```
