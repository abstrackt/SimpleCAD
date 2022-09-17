using System.ComponentModel.DataAnnotations;
using SharpSceneSerializer.DTOs.Enums;
using SharpSceneSerializer.DTOs.Interfaces;
using SharpSceneSerializer.DTOs.Types;

namespace SharpSceneSerializer.DTOs.GeometryObjects
{
    public class Torus : IGeometryObject
    {
        [Required]
        public ObjectType ObjectType => ObjectType.torus;
        [Required]
        public uint Id { get; init; }
        public string Name { get; init; }
        [Required]
        public Float3 Position { get; init; }
        [Required]
        public Float3 Rotation { get; init; }
        [Required]
        public Float3 Scale { get; init; }
        [Required]
        public Uint2 Samples { get; init; }
        [Required]
        [Range(0, float.MaxValue)]
        public float SmallRadius { get; init; }
        [Required]
        [Range(0, float.MaxValue)]
        public float LargeRadius { get; init; }
    }
}

