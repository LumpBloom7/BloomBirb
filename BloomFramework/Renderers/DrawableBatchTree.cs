using BloomFramework.Graphics;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Textures;

namespace BloomFramework.Renderers;

public class DrawableBatchTree
{
    private Dictionary<Shader, ShaderNode> shaderNodes = new();

    private Queue<ShaderNode> list = new();

    public void Add(Shader shader, ITexture texture, Drawable drawable)
    {
        if (!shaderNodes.TryGetValue(shader, out var node))
            shaderNodes[shader] = node = new ShaderNode();

        if (node.Count == 0)
            list.Enqueue(node);

        node.Add(texture, drawable);
    }

    public void DrawAll(OpenGlRenderer renderer)
    {
        while (list.TryDequeue(out var shaderNode))
            shaderNode.DrawAll(renderer);
    }
}

public class ShaderNode
{
    public int Count => list.Count;

    private Dictionary<ITexture, TextureNode> textureNodes = new();

    private Queue<TextureNode> list = new();

    public void Add(ITexture texture, Drawable drawable)
    {
        if (!textureNodes.TryGetValue(texture, out var node))
            textureNodes[texture] = node = new TextureNode();

        if (node.Count == 0)
            list.Enqueue(node);

        node.Add(drawable);
    }

    public void DrawAll(OpenGlRenderer renderer)
    {
        while (list.TryDequeue(out var textureNode))
            textureNode.DrawAll(renderer);
    }
}

public class TextureNode
{
    public int Count => drawableQueue.Count;

    private Queue<Drawable> drawableQueue = new();

    public void Add(Drawable drawable) => drawableQueue.Enqueue(drawable);

    public void DrawAll(OpenGlRenderer renderer)
    {
        while (drawableQueue.TryDequeue(out var drawable))
            drawable.Draw(renderer);
    }
}
