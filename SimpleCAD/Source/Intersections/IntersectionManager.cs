using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Intersections
{
    public class IntersectionManager
    {
        private static IntersectionManager instance = null;
        public static IntersectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IntersectionManager();
                }
                return instance;
            }
        }
    }
}
