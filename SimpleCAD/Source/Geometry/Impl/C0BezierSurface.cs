using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Geometry
{
    public class C0BezierSurface : Surface
    {
        public override int PatchSize => 4;
        public override int PatchOffset => 3;

        public C0BezierSurface(int patchesU, int patchesV) : base(patchesU, patchesV)
        {
            
        }
    }
}
