using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpSceneSerializer.DTOs.Enums;

namespace SharpSceneSerializer.DTOs.Interfaces
{
    [JsonInterfaceConverter(typeof(InterfaceConverter<IGeometryObject>))]
    //[JsonConverter(typeof(GeometryObjectConverter))]
    public interface IGeometryObject
    {
        [Required]
        public ObjectType ObjectType { get; }
    }
}