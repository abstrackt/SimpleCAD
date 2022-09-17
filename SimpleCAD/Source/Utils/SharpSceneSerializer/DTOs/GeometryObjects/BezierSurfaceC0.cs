using System.ComponentModel.DataAnnotations;
using SharpSceneSerializer.DTOs.Enums;
using SharpSceneSerializer.DTOs.Interfaces;
using SharpSceneSerializer.DTOs.Types;

namespace SharpSceneSerializer.DTOs.GeometryObjects
{
    public class BezierSurfaceC0 : IGeometryObject
    {
        [Required]
        public ObjectType ObjectType => ObjectType.bezierSurfaceC0;
        [Required]
        public uint Id { get; init; }
        public string Name { get; init; }
        [Required]
        public BezierPatchC0[] Patches { get; init; }
        [Required]
        public Bool2 ParameterWrapped { get; init; }
        [Required]
        public Uint2 Size { get; set; }
    }
}

