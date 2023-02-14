using BloomBirb.Graphics;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Textures;

namespace BloomBirb.Renderers;

public class DrawableBatchTree
{
    private Dictionary<Shader, ShaderNode> shaderNodes = new();

    private Queue<ShaderNode> list = new();

    public void Add(Shader shader, Texture texture, Drawable drawable)
    {
        if (!shaderNodes.TryGetValue(shader, out var node))
            shaderNodes[shader] = node = new ShaderNode();

        if (node.Count == 0)
            list.Enqueue(node);

        node.Add(texture, drawable);
    }

    public void DrawAll(OpenGLRenderer renderer)
    {
        while (list.TryDequeue(out var shaderNode))
            shaderNode.DrawAll(renderer);
    }
}

public class ShaderNode
{
    public int Count => list.Count;

    private Dictionary<Texture, TextureNode> textureNodes = new();

    private Queue<TextureNode> list = new();

    public void Add(Texture texture, Drawable drawable)
    {
        if (!textureNodes.TryGetValue(texture, out var node))
            textureNodes[texture] = node = new TextureNode();

        if (node.Count == 0)
            list.Enqueue(node);

        node.Add(drawable);
    }

    public void DrawAll(OpenGLRenderer renderer)
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

    public void DrawAll(OpenGLRenderer renderer)
    {
        while (drawableQueue.TryDequeue(out var drawable))
            drawable.Draw(renderer);
    }
}
