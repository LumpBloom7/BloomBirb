using System.Text;
using BloomFramework.Renderers.OpenGL;
using Silk.NET.OpenGL;
using Shader = BloomFramework.Renderers.OpenGL.Shader;

namespace BloomFramework.ResourceStores;

// TODO: This needs to be platform agnostic at some point
public class ShaderStore : IDisposable
{
    private record ShaderParts(uint Vert, uint Frag);

    private readonly string prefix;

    private readonly Dictionary<ShaderParts, Shader> shaderCache = new();

    private readonly Dictionary<string, uint> vertexParts = new();
    private readonly Dictionary<string, uint> fragParts = new();

    private readonly IResourceStore resources;

    private readonly OpenGlRenderer renderer;

    public ShaderStore(OpenGlRenderer renderer, IResourceStore resources, string prefix = "Shaders")
    {
        this.renderer = renderer;
        this.resources = resources;
        this.prefix = prefix;
    }

    public Shader Get(string vertexPart, string fragmentPart)
    {
        uint vertHandle = getShaderPart(ShaderType.VertexShader, vertexPart);
        uint fragHandle = getShaderPart(ShaderType.FragmentShader, vertexPart);

        ShaderParts parts = new(vertHandle, fragHandle);

        if (!shaderCache.TryGetValue(parts, out var shader))
        {
            shader = renderer.CreateShader(vertHandle, fragHandle);
            shaderCache.Add(parts, shader);
        }

        return shader;
    }

    private uint getShaderPart(ShaderType type, string name)
    {
        Dictionary<string, uint> cache;

        if (type == ShaderType.VertexShader)
            cache = vertexParts;
        else if (type == ShaderType.FragmentShader)
            cache = fragParts;
        else
            return 0;

        // Try without fallback extension
        uint result = retrieveOrLoadPart(cache, type, $"{name}");

        // Try with fallback extension
        if (result == 0)
            result = retrieveOrLoadPart(cache, type, $"{name}{extensionFor(type)}");

        return result;
    }

    private uint retrieveOrLoadPart(Dictionary<string, uint> cache, ShaderType type, string name)
    {
        if (!cache.TryGetValue(name, out uint handle))
        {
            string src = loadFile(name);

            if (string.IsNullOrEmpty(src))
                return 0;

            handle = renderer.CreateShaderPart(type, src);
            cache.Add(name, handle);
        }

        return handle;
    }

    private static string extensionFor(ShaderType type) => type switch
    {
        ShaderType.VertexShader => ".vert",
        ShaderType.FragmentShader => ".frag",
        _ => ""
    };

    private string loadFile(string path)
    {
        StringBuilder sb = new();
        loadFile(path, sb);
        return sb.ToString();
    }

    private void loadFile(string path, StringBuilder sb)
    {
        const string inc_pref = @"#include ";

        Stream? stream = resources.Get($"{prefix}.{path}");

        if (stream is null)
            return;

        using (StreamReader sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine()!;

                if (line.StartsWith(inc_pref))
                {
                    string cleaned = line.Substring(9).Trim('"', '\'').Replace('/', '.');
                    loadFile(cleaned, sb);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
        }
    }

    private bool isDisposed;

    protected void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            foreach (var shader in shaderCache)
                shader.Value.Dispose();
        }

        foreach (var vertPart in vertexParts)
            renderer.Context?.DeleteShader(vertPart.Value);

        foreach (var fragPart in fragParts)
            renderer.Context?.DeleteShader(fragPart.Value);

        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ShaderStore()
    {
        Dispose();
    }
}
