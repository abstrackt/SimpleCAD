using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Utils;
using OpenTK.Graphics.OpenGL4;
using SimpleCAD.Source.Intersections;

namespace SimpleCAD.Source.Paths
{
    public class MillingPathCreator
    {
        private static MillingPathCreator instance = null;
        public static MillingPathCreator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MillingPathCreator();
                }
                return instance;
            }
        }

        private MillingPathCreator()
        {
            _heightmapTexture = new Texture(PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float);
            _heightmapTexture.LoadNull(1, 1);
        }

        public const int PIXELS_PER_UNIT = 64;
        public const int ROUGH_SAMPLES_PER_UNIT = 16;
        public const float FLOOR_THICKNESS = 1.5f;

        public IntPtr HeightmapHandle => (IntPtr)(_heightmapTexture == null ? 0 : _heightmapTexture.Handle);

        private Framebuffer? _heightmapBuffer = null;
        private Texture? _heightmapTexture = null;
        private Matrix4 _viewMatrix;
        private Matrix4 _projMatrix;
        private int _texWidth, _texHeight;
        private float _width, _height, _thickness;

        public void SetUpFramebuffer(float width, float height, float thickness)
        {
            int texWidth = (int)(PIXELS_PER_UNIT * width);
            int texHeight = (int)(PIXELS_PER_UNIT * height);

            _width = width;
            _height = height;
            _thickness = thickness;

            _projMatrix = Matrix4.CreateOrthographic(width, height, 1f, 20f);
            _viewMatrix = Matrix4.LookAt(Vector3.UnitY * 20f, Vector3.Zero, Vector3.UnitX);

            _heightmapBuffer = new Framebuffer(texWidth, texHeight);
            _heightmapTexture = new Texture(PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float);

            _heightmapTexture.LoadNull(texWidth, texHeight);

            _texWidth = texWidth;
            _texHeight = texHeight;
        }

        private float[,] MakeRoundTemplate(float headSize)
        {
            var pixelRadius = (int)(headSize / _width * _texWidth);

            var template = new float[2 * pixelRadius + 1, 2 * pixelRadius + 1];

            for (int x = 0; x <= 2 * pixelRadius; x++)
            {
                for (int y = 0; y <= 2 * pixelRadius; y++)
                {
                    var p = new Vector2(x - pixelRadius, y - pixelRadius);
                    var r = p.Length / pixelRadius;
                    r *= headSize;
                    if (p.X * p.X + p.Y * p.Y <= pixelRadius * pixelRadius)
                    {
                        var h = -(float)Math.Sqrt(headSize * headSize - r * r);
                        template[x, y] = h + headSize;
                    }
                }
            }

            return template;
        }

        public MillingPath GenerateRoughMillingPath(float headSize)
        {
            var path = new MillingPath();

            path.headType = HeadType.Round;
            path.headSize = headSize;

            var template = MakeRoundTemplate(headSize * .5f);

            // Render scene into heightmap
            if (_heightmapBuffer != null && _heightmapTexture != null)
            {
                _heightmapBuffer.Use(_heightmapTexture);

                var scene = Scene.Instance;

                // Render all complex models

                var elementList = new List<RenderableElement>();

                for (int i = 0; i < scene.complexModels.Count; i++)
                {
                    elementList.Add(scene.complexModels[i]);
                }

                scene.camera.CustomRender(elementList, _viewMatrix, _projMatrix);

                float[,] pixels = new float[_texWidth, _texHeight];

                GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                GL.ReadPixels(0, 0, _texWidth, _texHeight, PixelFormat.Red, PixelType.Float, pixels);

                Framebuffer.UseDefault();

                var xStride = Math.Max(PIXELS_PER_UNIT / ROUGH_SAMPLES_PER_UNIT, 1);
                var yStride = (int)Math.Max((headSize * .5f) / _height * _texHeight, 1);
                var right = true;

                int y = 0;
                int x = 0;

                path.points.Add(new Vector3(0, -150, 100));

                while (y < _texHeight)
                {
                    var yCoord = y / (float)_texHeight * _height - _height * .5f;

                    if (right)
                    {
                        while (x < _texWidth)
                        {
                            var xCoord = x / (float)_texWidth * _width - _width * .5f;
                            path.points.Add(new Vector3(
                                xCoord * 10, 
                                yCoord * 10,
                                (Math.Max(GetHeight(x, y, template, pixels), (_thickness - FLOOR_THICKNESS) / 2) + FLOOR_THICKNESS) * 10));
                            x += xStride;
                        }
                        x = _texWidth - 1;
                        right = false;
                    }
                    else
                    {
                        while (x >= 0)
                        {
                            var xCoord = x / (float)_texWidth * _width - _width * .5f;
                            path.points.Add(new Vector3(
                                xCoord * 10, 
                                yCoord * 10,
                                (Math.Max(GetHeight(x, y, template, pixels), (_thickness - FLOOR_THICKNESS) / 2) + FLOOR_THICKNESS) * 10));
                            x -= xStride;
                        }
                        x = 0;
                        right = true;
                    }

                    y += yStride;
                }

                y = _texHeight - 1;

                while (y >= 0)
                {
                    var yCoord = y / (float)_texHeight * _height - _height * .5f;

                    if (right)
                    {
                        while (x < _texWidth)
                        {
                            var xCoord = x / (float)_texWidth * _width - _width * .5f;
                            path.points.Add(new Vector3(
                                xCoord * 10, 
                                yCoord * 10,
                                (GetHeight(x, y, template, pixels) + FLOOR_THICKNESS) * 10));
                            x += xStride;
                        }
                        x = _texWidth - 1;
                        right = false;
                    }
                    else
                    {
                        while (x >= 0)
                        {
                            var xCoord = x / (float)_texWidth * _width - _width * .5f;
                            path.points.Add(new Vector3(
                                xCoord * 10,
                                yCoord * 10,
                                (GetHeight(x, y, template, pixels) + FLOOR_THICKNESS) * 10));
                            x -= xStride;
                        }
                        x = 0;
                        right = true;
                    }

                    y -= yStride;
                }

                path.points.Add(new Vector3(0, -150, 100));
            }

            return path;
        }

        public MillingPath GeneratePerimeterMillingPath(float headSize)
        {
            var path = new MillingPath();

            path.headType = HeadType.Flat;
            path.headSize = headSize;

            var scene = Scene.Instance;

            if (scene.TryFindByName("body", out var body) &&
                scene.TryFindByName("lid", out var lid) &&
                scene.TryFindByName("handle", out var handle) &&
                scene.TryFindByName("floor", out var floor) &&
                scene.TryFindByName("edgesw", out var edge1) &&
                scene.TryFindByName("edgenw", out var edge2) &&
                scene.TryFindByName("edgene", out var edge3) &&
                scene.TryFindByName("edgese", out var edge4) &&
                scene.TryFindByName("left", out var left) &&
                scene.TryFindByName("upright", out var upright) &&
                scene.TryFindByName("downright", out var downright) &&
                scene.TryFindByName("handlefloor", out var handlefloor))
            {
                scene.SetupIntersection(body, left, false, out var i1, headSize * .5f);
                scene.SetupIntersection(lid, edge1, false, out var i2, headSize * .5f);
                scene.SetupIntersection(lid, edge2, false, out var i3, headSize * .5f);
                scene.SetupIntersection(lid, edge3, false, out var i4, headSize * .5f);
                scene.SetupIntersection(lid, edge4, false, out var i5, headSize * .5f);
                scene.SetupIntersection(body, upright, false, out var i6, headSize * .5f);
                scene.SetupIntersection(handle, handlefloor, false, out var i7, headSize * .5f);
                scene.SetupIntersection(body, downright, false, out var i8, headSize * .5f);

                path.points.Add(new Vector3(0, 10, -15));

                var c1 = IntersectionData.FindClosestPair(i1, i2);

                var tmp = new List<Vector3>();

                for (int i = 0; i < c1.i1; i++)
                {
                    tmp.Add(i1.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = i2.points.Count - 1; i > c1.i2; i--)
                {
                    tmp.Add(i2.points[i]);
                }

                tmp.Reverse();
                path.points.AddRange(tmp);
                tmp.Clear();

                var ex = IntersectionData.Extrapolate(i2, i3, true, true);

                path.points.Add(ex);

                tmp.AddRange(i3.points);
                path.points.AddRange(tmp);
                tmp.Clear();

                ex = IntersectionData.Extrapolate(i3, i4, true, false);

                path.points.Add(ex);

                // Add milling rest of base
                var currentZ = -7.5f;

                bool makePass = true;

                var basePath = new List<Vector3>();

                basePath.Add(new Vector3(0, 0, currentZ));

                for (int i = 2; i < path.points.Count - 1; i++)
                {
                    if (Math.Abs(path.points[i + 1].Z - currentZ) >= headSize - 0.001f)
                    {
                        makePass = true;
                    }

                    if (makePass == true || i == path.points.Count - 2)
                    {
                        var nextZ = path.points[i].Z;
                        basePath.Add(new Vector3(7.5f, 0, currentZ));
                        basePath.Add(new Vector3(7.5f, 0, nextZ));
                        basePath.Add(new Vector3(path.points[i].X, 0, nextZ));
                        currentZ = nextZ;
                        makePass = false;
                    }
                }

                basePath.Add(new Vector3(7.5f, 0, currentZ));
                basePath.Add(new Vector3(7.5f, 0, 7.5f));
                basePath.Add(new Vector3(0, 0, 7.5f));

                var offset = path.points.Count;

                tmp.AddRange(i4.points);
                path.points.AddRange(tmp);
                tmp.Clear();

                ex = IntersectionData.Extrapolate(i4, i5, false, true);

                path.points.Add(ex);

                var c2 = IntersectionData.FindClosestPair(i5, i6);
                var c3 = IntersectionData.FindClosestPair(i6, i7);

                for (int i = 0; i < c2.i1; i++)
                {
                    tmp.Add(i5.points[i]);
                }
              
                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c2.i2; i < c3.i1; i++)
                {
                    tmp.Add(i6.points[i]);
                }

                var c4 = IntersectionData.FindClosestPair(i7, i8);

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c3.i2; i < c4.i1; i++)
                {
                    tmp.Add(i7.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c4.i2; i < i8.points.Count; i++)
                {
                    tmp.Add(i8.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                currentZ = 7.5f;

                makePass = true;

                for (int i = offset + 2; i < path.points.Count - 1; i++)
                {
                    if (Math.Abs(path.points[i + 1].Z - currentZ) >= headSize - 0.001f)
                    {
                        makePass = true;
                    }

                    if (makePass == true || i == path.points.Count - 2)
                    {
                        var nextZ = path.points[i].Z;
                        basePath.Add(new Vector3(-7.5f, 0, currentZ));
                        basePath.Add(new Vector3(-7.5f, 0, nextZ));
                        basePath.Add(new Vector3(path.points[i].X, 0, nextZ));
                        currentZ = nextZ;
                        makePass = false;
                    }
                }

                basePath.Add(new Vector3(-7.5f, 0, currentZ));
                basePath.Add(new Vector3(-7.5f, 0, -7.5f));
                basePath.Add(new Vector3(0, 0, -7.5f));

                path.points.AddRange(basePath);


                for (int i = 0; i < path.points.Count; i++)
                {
                    var p = path.points[i];
                    p.Y = 0;

                    path.points[i] = new Vector3(p.X * 10, p.Z * 10, FLOOR_THICKNESS * 10);
                }
            }

            return path;
        }

        public MillingPath GeneratePreciseMillingPath(float headSize)
        {
            var path = new MillingPath();

            path.headType = HeadType.Round;
            path.headSize = headSize;

            var scene = Scene.Instance;

            if (scene.TryFindByName("body", out var body) &&
                scene.TryFindByName("lid", out var lid) &&
                scene.TryFindByName("handle", out var handle) &&
                scene.TryFindByName("floor", out var floor) &&
                scene.TryFindByName("edgesw", out var edge1) &&
                scene.TryFindByName("edgenw", out var edge2) &&
                scene.TryFindByName("edgene", out var edge3) &&
                scene.TryFindByName("edgese", out var edge4) &&
                scene.TryFindByName("left", out var left) &&
                scene.TryFindByName("upright", out var upright) &&
                scene.TryFindByName("downright", out var downright) &&
                scene.TryFindByName("handlefloor", out var handlefloor))
            {
                scene.cursorPos = new Vector3(-2, 0, -3);

                scene.SetupIntersection(body, lid, false, out var i1, 0, headSize * .5f);
                scene.SetupIntersection(body, handle, false, out var i2, 0, headSize * .5f);
                scene.SetupIntersection(body, handle, true, out var i3, 0, headSize * .5f);

                scene.cursorPos = new Vector3(-2, 0, -3);

                var bodyMask = new bool[512, 512];
                var handleMask = new bool[512, 512];
                var lidMask = new bool[512, 512];

                GetIntersectMask(body, new List<IntersectionData>() { i1, i2, i3 }, new List<bool>() { false, false, false }, true, ref bodyMask);

                GetIntersectMask(handle, new List<IntersectionData>() { i2, i3 }, new List<bool>() { false, true }, false, ref handleMask);

                GetIntersectMask(lid, new List<IntersectionData>() { i1 }, new List<bool>() { true }, false, ref lidMask);

                var g1 = body.ParametricGeometry;

                var g2 = handle.ParametricGeometry;

                var g3 = lid.ParametricGeometry;

                float u = 0;
                float v = 0;
                float width = bodyMask.GetLength(0);
                float height = bodyMask.GetLength(1);

                bool edgeFound = false;
                bool right = true;
                bool first = true;

                var transition = Vector3.Zero;

                while (v < g1.RangeV)
                {
                    var p = g1.Sample(u, v, headSize * .5f);
                    if (!bodyMask[(int)(u * width / g1.RangeU), (int)(v * height / g1.RangeV)] && p.Y >= headSize * .5f)
                    {
                        edgeFound = true;
                    }
                    else if (edgeFound)
                    {
                        edgeFound = false;
                        right = !right;
                        v += 0.039f;
                    }

                    if (edgeFound)
                    {
                        if (first)
                        {
                            first = false;
                            transition = p;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(p);
                    }

                    if (right)
                    {
                        u += 0.05f;
                    }
                    else
                    {
                        u -= 0.05f;
                    }

                    if (u < 0 || u >= g1.RangeU)
                    {
                        u = Math.Clamp(u, 0, g1.RangeU - 0.01f);
                        right = !right;
                        v += 0.039f;
                    }
                }

                transition = path.points[^1];
                transition.Y = 5;

                path.points.Add(transition);

                u = -g2.RangeU;
                v = 0;
                width = handleMask.GetLength(0);
                height = handleMask.GetLength(1);

                edgeFound = false;
                right = true;
                first = true;

                while (v < g2.RangeV)
                {
                    var p = g2.Sample(u, v, headSize * .5f);
                    if (!handleMask[(int)(((u + g2.RangeU) % g2.RangeU) * width / g2.RangeU), (int)(v * height / g2.RangeV)] && p.Y >= headSize * .5f)
                    {
                        edgeFound = true;
                    }
                    else if (edgeFound)
                    {
                        edgeFound = false;
                        right = !right;
                        v += 0.05f;
                    }

                    if (edgeFound)
                    {
                        if (first)
                        {
                            first = false;
                            transition = p;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(p);
                    }

                    if (right)
                    {
                        u += 0.05f;
                    }
                    else
                    {
                        u -= 0.05f;
                    }

                    if (u <= -g2.RangeU || u >= g2.RangeU)
                    {
                        u = Math.Clamp(u, 0, g2.RangeU - 0.01f);
                        right = !right;
                        v += 0.05f;
                    }
                }

                transition = path.points[^1];
                transition.Y = 5;

                path.points.Add(transition);

                u = .5f * g3.RangeU;
                v = 0;
                width = lidMask.GetLength(0);
                height = lidMask.GetLength(1);

                edgeFound = false;
                right = true;
                first = true;

                while (u > -g3.RangeU * .5f)
                {
                    var p = g3.Sample(u, v, headSize * .5f);
                    if (!lidMask[(int)(((u + g3.RangeU) % g3.RangeU) * width / g3.RangeU), (int)(v * height / g3.RangeV)] && p.Y >= headSize * .5f)
                    {
                        edgeFound = true;
                    }
                    else if (edgeFound)
                    {
                        edgeFound = false;
                        right = !right;
                        u -= 0.05f;
                    }

                    if (edgeFound)
                    {
                        if (first)
                        {
                            first = false;
                            transition = p;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(p);
                    }

                    if (right)
                    {
                        v += 0.05f;
                    }
                    else
                    {
                        v -= 0.05f;
                    }

                    if (v < 0 || v >= 0.95)
                    {
                        v = Math.Clamp(v, 0, g3.RangeV - 0.01f);
                        right = !right;
                        u -= 0.05f;
                    }
                }


                transition = path.points[^1];
                transition.Y = 5;

                path.points.Add(transition);

                u = .5f * g3.RangeU;
                v = g3.RangeV - 0.95f;
                width = lidMask.GetLength(0);
                height = lidMask.GetLength(1);

                edgeFound = false;
                right = true;
                first = true;

                while (u > -g3.RangeU * .5f)
                {
                    var p = g3.Sample(u, v, headSize * .5f);
                    if (!lidMask[(int)(((u + g3.RangeU) % g3.RangeU) * width / g3.RangeU), (int)(v * height / g3.RangeV)] && p.Y >= headSize * .5f)
                    {
                        edgeFound = true;
                    }
                    else if (edgeFound)
                    {
                        edgeFound = false;
                        right = !right;
                        u -= 0.05f;
                    }

                    if (edgeFound)
                    {
                        if (first)
                        {
                            first = false;
                            transition = p;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(p);
                    }

                    if (right)
                    {
                        v += 0.05f;
                    }
                    else
                    {
                        v -= 0.05f;
                    }

                    if (v < g3.RangeV - 0.95f || v >= g3.RangeV)
                    {
                        v = Math.Clamp(v, g3.RangeV - 0.95f, g3.RangeV - 0.01f);
                        right = !right;
                        u -= 0.05f;
                    }
                }

                // Mill blade edge

                transition = path.points[^1];
                transition.Y = 5;

                path.points.Add(transition);

                first = true;

                var edgeZ = 5.7f;

                for (u = -g3.RangeU * .5f; u < g3.RangeU * .5f; u+=0.02f)
                {
                    var p = g3.Sample(u, 1.5f, 0);
                    if (p.Y >= headSize * .5f)
                    {
                        p.Z = edgeZ;
                        var center = new Vector3(0, -0.05f, edgeZ);

                        var n = headSize * .5f * (p - center).Normalized();

                        p += n;

                        if (first)
                        {
                            first = false;
                            transition = p;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(p);
                    }
                }

                transition = path.points[^1];
                transition.Y = 5;
                path.points.Add(transition);

                scene.SetupIntersection(body, left, false, out var i4, 0, headSize * .5f);
                scene.SetupIntersection(lid, edge1, false, out var i5, 0, headSize * .5f);
                scene.SetupIntersection(lid, edge2, false, out var i6, 0, headSize * .5f);
                scene.SetupIntersection(lid, edge3, false, out var i7, 0, headSize * .5f);
                scene.SetupIntersection(lid, edge4, false, out var i8, 0, headSize * .5f);
                scene.SetupIntersection(body, upright, false, out var i9, 0, headSize * .5f);
                scene.SetupIntersection(handle, handlefloor, false, out var i10, 0, headSize * .5f);
                scene.SetupIntersection(body, downright, false, out var i11, 0, headSize * .5f);

                path.points.Add(new Vector3(0, 5, -7.5f));

                for (int i = 0; i < path.points.Count; i++)
                {
                    var p = path.points[i];
                    path.points[i] = new Vector3(p.X * 10, p.Z * 10, (p.Y + FLOOR_THICKNESS - headSize / 2) * 10);
                }

                var offset = path.points.Count;

                var c1 = IntersectionData.FindClosestPair(i4, i5);

                var tmp = new List<Vector3>();

                for (int i = 0; i < c1.i1; i++)
                {
                    tmp.Add(i4.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = i5.points.Count - 1; i > c1.i2; i--)
                {
                    tmp.Add(i5.points[i]);
                }

                tmp.Reverse();
                path.points.AddRange(tmp);
                tmp.Clear();

                var ex = IntersectionData.Extrapolate(i5, i6, true, true);

                path.points.Add(ex);

                tmp.AddRange(i6.points);
                path.points.AddRange(tmp);
                tmp.Clear();

                ex = IntersectionData.Extrapolate(i6, i7, true, false);

                path.points.Add(ex);

                tmp.AddRange(i7.points);
                path.points.AddRange(tmp);
                tmp.Clear();

                ex = IntersectionData.Extrapolate(i7, i8, false, true);

                path.points.Add(ex);

                var c2 = IntersectionData.FindClosestPair(i8, i9);
                var c3 = IntersectionData.FindClosestPair(i9, i10);

                for (int i = 0; i < c2.i1; i++)
                {
                    tmp.Add(i8.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c2.i2; i < c3.i1; i++)
                {
                    tmp.Add(i9.points[i]);
                }

                var c4 = IntersectionData.FindClosestPair(i10, i11);

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c3.i2; i < c4.i1; i++)
                {
                    tmp.Add(i10.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                for (int i = c4.i2; i < i11.points.Count; i++)
                {
                    tmp.Add(i11.points[i]);
                }

                path.points.AddRange(tmp);
                tmp.Clear();

                scene.SetupIntersection(body, floor, false, out var i12, 0, headSize * .5f);
                scene.SetupIntersection(handle, floor, false, out var i13, 0, headSize * .5f);

                transition = path.points[^1];
                transition.Y = 5;
                path.points.Add(transition);
                first = true;

                var c5 = IntersectionData.FindClosestPair(i12, i13);
                var c6 = IntersectionData.FindClosestPair(i12, i13, c5.i1, c5.i2);

                for (int i = c6.i1; i < c5.i1; i++)
                {
                    if (first)
                    {
                        first = false;
                        transition = i12.points[i];
                        transition.Y = 5;
                        path.points.Add(transition);
                    }

                    path.points.Add(i12.points[i]);
                }

                for (int i = c5.i2; i < c6.i2; i++)
                {
                    path.points.Add(i13.points[i]);
                }

                var curr1 = c6.i1;
                var curr2 = c6.i2;

                var lastZ = i12.points[curr1].Z;
                var swap = false;

                var diff = headSize * .25f;

                while (curr1 <= c5.i1 && curr2 >= c5.i2)
                {
                    if (swap)
                    {
                        var p = i13.points[curr2];
                        path.points.Add(p);
                        if (Math.Abs(p.Z - lastZ) > diff)
                        {
                            lastZ = p.Z;
                            swap = false;
                            while (curr1 <= c5.i1 && i12.points[curr1].Z > p.Z)
                            {
                                curr1++;
                            }
                        }
                        curr2--;
                    }
                    else
                    {
                        var p = i12.points[curr1];
                        path.points.Add(p);
                        if (Math.Abs(p.Z - lastZ) > diff)
                        {
                            lastZ = p.Z;
                            swap = true;
                            while (curr2 >= c5.i2 && i13.points[curr2].Z > p.Z)
                            {
                                curr2--;
                            }
                        }
                        curr1++;
                    }
                }


                transition = path.points[^1];
                transition.Y = 5;
                path.points.Add(transition);
                first = true;

                foreach (var point in i2.points)
                {
                    if (point.Z >= headSize * .5f)
                    {
                        if (first)
                        {
                            first = false;
                            transition = point;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(point);
                    }
                }

                transition = path.points[^1];
                transition.Y = 5;
                path.points.Add(transition);
                first = true;

                foreach (var point in i3.points)
                {
                    if (point.Z >= headSize * .5f)
                    {
                        if (first)
                        {
                            first = false;
                            transition = point;
                            transition.Y = 5;
                            path.points.Add(transition);
                        }
                        path.points.Add(point);
                    }
                }

                for (int i = offset; i < path.points.Count; i++)
                {
                    var p = path.points[i];
                    path.points[i] = new Vector3(p.X * 10, p.Z * 10, (p.Y + FLOOR_THICKNESS - headSize * .5f + 0.05f) * 10);
                }
            }

            return path;
        }

        private void GetIntersectMask(SceneModel m, List<IntersectionData> data, List<bool> cutNonzero, bool first, ref bool[,] mask)
        {
            var intersections = IntersectionManager.Instance;

            if (data.Count != cutNonzero.Count)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                var texture = intersections.GetIntersectTexture(m.ParametricGeometry, data[i].parameters, first);

                if (texture.GetLength(0) != mask.GetLength(0) || 
                    texture.GetLength(1) != mask.GetLength(1))
                {
                    continue;
                }

                for (int x = 0; x < texture.GetLength(0); x++)
                {
                    for (int y = 0; y < texture.GetLength(1); y++)
                    {
                        var value = (cutNonzero[i] ? texture[x, y] == 255 : texture[x, y] == 0);
                        mask[x, y] |= value;
                    }
                }
            }
        }

        private float GetHeight(int xCenter, int yCenter, float[,] template, float[,] pixels)
        {
            var h = 0;
            float max = pixels[xCenter, yCenter];

            var xSize = template.GetLength(0);
            var ySize = template.GetLength(1);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    var xTemp = x - (xSize / 2);
                    var yTemp = y - (ySize / 2);
                    if (xTemp * xTemp + yTemp * yTemp <= (xSize / 2) * (xSize / 2))
                    {
                        var xCoord = xCenter + xTemp;
                        var yCoord = yCenter + yTemp;

                        if (xCoord >= 0 && xCoord < _texWidth &&
                            yCoord >= 0 && yCoord < _texHeight)
                        {
                            var current = pixels[xCoord, yCoord] - template[x, y];

                            if (current > max)
                            {
                                max = current;
                            }
                        }
                    }
                }
            }

            return max;
        }
    }
}
