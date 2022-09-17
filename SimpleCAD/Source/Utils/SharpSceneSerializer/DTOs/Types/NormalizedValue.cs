using System.ComponentModel.DataAnnotations;

namespace SharpSceneSerializer.DTOs.Types;

public record NormalizedValue([Required] [Range(0f, 1f)] float Value);
