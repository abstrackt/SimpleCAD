using ImGuiNET;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCAD.Source.Geometry
{
    public class C0BezierSurface : Surface, ISceneGUIElement
    {
        public override int PatchSize => 4;
        public override int PatchOffset => 3;
        public override (int u, int v) TesselationLevel => (_tessU, _tessV);

        private int _tessU, _tessV;

        public C0BezierSurface(int patchesU, int patchesV) : base(patchesU, patchesV) 
        {
            _tessU = 4;
            _tessV = 4;
        } 
        
        public override List<Line> GetLines()
        {
            return new List<Line>();
        }

        public void DrawElementGUI()
        {
            ImGui.DragInt("Divisions (U)", ref _tessU, 0.1f, 1, 64);
            ImGui.DragInt("Divisions (V)", ref _tessV, 0.1f, 1, 64);
        }
    }
}
