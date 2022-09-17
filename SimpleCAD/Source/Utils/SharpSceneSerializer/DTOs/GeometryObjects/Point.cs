using System.ComponentModel.DataAnnotations;
using SharpSceneSerializer.DTOs.Interfaces;
using SharpSceneSerializer.DTOs.Types;

namespace SharpSceneSerializer.DTOs.GeometryObjects
{
    public class Point
    {
        [Required]
        public uint Id { get; init; }
        public string Name { get; init; }
        [Required]
        public Float3 Position { get; init; }
    }
}

