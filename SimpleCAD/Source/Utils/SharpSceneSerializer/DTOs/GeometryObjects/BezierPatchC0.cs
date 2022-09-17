using System.ComponentModel.DataAnnotations;
using SharpSceneSerializer.DTOs.Enums;
using SharpSceneSerializer.DTOs.Interfaces;
using SharpSceneSerializer.DTOs.Types;

namespace SharpSceneSerializer.DTOs.GeometryObjects
{
    public class BezierPatchC0 : IGeometryObject
    {
        [Required]
        public ObjectType ObjectType => ObjectType.bezierPatchC0;
        [Required]
        public uint Id { get; init; }
        public string Name { get; init; }
        [Required]
        public PointRef[] controlPoints { get; init; } = new PointRef[16];
        [Required]
        public Uint2 Samples { get; init; }
    }
}

