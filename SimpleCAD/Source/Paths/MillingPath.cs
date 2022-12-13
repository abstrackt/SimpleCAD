using OpenTK.Mathematics;

namespace SimpleCAD.Source.Paths
{
    public class MillingPath
    {
        public HeadType headType = HeadType.Round;
        public float headSize;
        public List<Vector3> points = new();

        public void Save(string directory, string name)
        {
            if (Directory.Exists(directory))
            {
                var ext = string.Empty;

                if (headType == HeadType.Round)
                {
                    ext += "k";
                }
                else if (headType == HeadType.Flat)
                {
                    ext += "f";
                }

                ext += (headSize < 1 ? "0" : "") + (int)(headSize * 10);

                var instructions = string.Empty;

                for (int i = 0; i < points.Count; i++)
                {
                    var instruction = $"N{i + 1}G01X{points[i].X.ToString("0.000").Replace(',', '.')}Y{points[i].Y.ToString("0.000").Replace(',', '.')}Z{points[i].Z.ToString("0.000").Replace(',', '.')}\n";
                    instructions += instruction;
                }

                File.WriteAllText(Path.Combine(directory, $"{name}.{ext}"), instructions);
            }
            else
            {
                throw new FileNotFoundException("No directory named " + directory);
            }
        }
    }
}
