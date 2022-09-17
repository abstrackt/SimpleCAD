using System.Runtime.Serialization;
using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace SharpSceneSerializer.DTOs.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ObjectType
    {
        [EnumMember(Value = "torus")]
        torus = 0,
        [EnumMember(Value = "bezierC0")]
        bezierC0 = 1,
        [EnumMember(Value = "bezierC2")]
        bezierC2 = 2,
        [EnumMember(Value = "interpolatedC2")]
        interpolatedC2 = 3,
        [EnumMember(Value = "bezierPatchC0")]
        bezierPatchC0 = 4,
        [EnumMember(Value = "bezierPatchC2")]
        bezierPatchC2 = 5,
        [EnumMember(Value = "bezierSurfaceC0")]
        bezierSurfaceC0 = 6,
        [EnumMember(Value = "bezierSurfaceC2")]
        bezierSurfaceC2 = 7,
    }
}

