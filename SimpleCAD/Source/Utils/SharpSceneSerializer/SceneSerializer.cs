using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SharpSceneSerializer.DTOs;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.VisualBasic;
using System;

namespace SharpSceneSerializer
{
    public static class SceneSerializer
    {
        static string SerializeRegex = "(\\\"\\$type\\\":[\\s]*\\\"[^\\\"]*\\\",)";

        static string DeserializeTorusRegex = "(\\\"objectType\\\":[\\s]*\\\"torus\\\",)";
        static string DeserializeCurveC0Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierC0\\\",)";
        static string DeserializeCurveC2Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierC2\\\",)";
        static string DeserializeInterpolatedC2Regex = "(\\\"objectType\\\":[\\s]*\\\"interpolatedC2\\\",)";
        static string DeserializePatchC0Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierPatchC0\\\",)";
        static string DeserializePatchC2Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierPatchC2\\\",)";
        static string DeserializeSurfaceC0Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierSurfaceC0\\\",)";
        static string DeserializeSurfaceC2Regex = "(\\\"objectType\\\":[\\s]*\\\"bezierSurfaceC2\\\",)";

        private static string torusTypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.Torus\",\"objectType\":\"torus\",";
        private static string curveC0TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierC0\",\"objectType\":\"bezierC0\",";
        private static string curveC2TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierC2\",\"objectType\":\"bezierC2\",";
        private static string interpolatedC2TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.InterpolatedC2\",\"objectType\":\"interpolatedC2\",";
        private static string bezierPatchC0TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierPatchC0\",\"objectType\":\"bezierPatchC0\",";
        private static string bezierPatchC2TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierPatchC2\",\"objectType\":\"bezierPatchC2\",";
        private static string bezierSurfaceC0TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC0\",\"objectType\":\"bezierSurfaceC0\",";
        private static string bezierSurfaceC2TypeText =
            "\"$type\":\"SharpSceneSerializer.DTOs.GeometryObjects.BezierSurfaceC2\",\"objectType\":\"bezierSurfaceC2\",";

        public static bool Serialize(Scene scene, string filePath, bool overwrite = true)
        {
            string jsonString = JsonSerializer.Serialize(scene, new JsonSerializerOptions(
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }));
            Console.WriteLine(SerializeRegex);
            string output = Regex.Replace(jsonString, SerializeRegex, "");
            if (!overwrite && File.Exists(filePath))
                return false;
            File.WriteAllText(filePath, output);
            return true;
        }

        public static (bool succeded, Scene scene) Deserialize(string filePath, string schemaPath)
        {
            if (!File.Exists(filePath) || !File.Exists(schemaPath))
                return (false, null);

            string jsonFileContent = File.ReadAllText(filePath);
            string jsonSchemaContent = File.ReadAllText(schemaPath);

            JSchema schema = JSchema.Parse(jsonSchemaContent);
            JObject jsonObject = JObject.Parse(jsonFileContent);
            if (!jsonObject.IsValid(schema))
                return (false, null);


            jsonFileContent = Regex.Replace(jsonFileContent, @"\s", "");

            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeTorusRegex, torusTypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeCurveC0Regex, curveC0TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeCurveC2Regex, curveC2TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeInterpolatedC2Regex, interpolatedC2TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializePatchC0Regex, bezierPatchC0TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializePatchC2Regex, bezierPatchC2TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeSurfaceC0Regex, bezierSurfaceC0TypeText);
            jsonFileContent = Regex.Replace(jsonFileContent, DeserializeSurfaceC2Regex, bezierSurfaceC2TypeText);

            Scene scene = JsonSerializer.Deserialize<Scene>(jsonFileContent,
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

            return (true, scene);
        }
    }
}