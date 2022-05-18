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
            using (AppWindow app = new AppWindow(1280, 720, "SimpleCAD"))
            {
                app.Run(60.0);
            }
        }
    }
}
