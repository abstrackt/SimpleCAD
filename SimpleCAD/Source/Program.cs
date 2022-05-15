using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source
{
    class Program
    {
        static void Main(string[] args)
        {
            using (AppWindow app = new AppWindow(800, 600, "SimpleCAD"))
            {
                app.Run(60.0);
            }
        }
    }
}
