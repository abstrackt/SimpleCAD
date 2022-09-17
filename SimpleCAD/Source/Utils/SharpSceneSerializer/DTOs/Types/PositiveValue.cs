using System.ComponentModel.DataAnnotations;

namespace SharpSceneSerializer.DTOs.Types;

public record PositiveValue([Required][Range(0f, float.MaxValue)] float Value);