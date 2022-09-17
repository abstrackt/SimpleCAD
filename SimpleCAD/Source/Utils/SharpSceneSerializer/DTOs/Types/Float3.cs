using System.ComponentModel.DataAnnotations;

namespace SharpSceneSerializer.DTOs.Types;

public record Float3([Required] float X, [Required] float Y, [Required] float Z);