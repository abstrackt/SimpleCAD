using System.ComponentModel.DataAnnotations;
using SharpSceneSerializer.DTOs.Interfaces;
using SharpSceneSerializer.DTOs.Types;

namespace SharpSceneSerializer.DTOs.GeometryObjects;

public class PointRef
{
    [Required]
    public uint Id { get; init; }
}