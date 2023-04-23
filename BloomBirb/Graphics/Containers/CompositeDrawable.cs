using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.Graphics.Containers;

public abstract class CompositeDrawable<T> : Drawable where T : Drawable
{
    private List<T> children = new();

    public IReadOnlyList<T> Children => children;

    public void Add(T drawable)
    {
        if (drawable.Parent != null)
            throw new InvalidOperationException("Cannot add drawable to multiple parents.");

        drawable.Parent = this;
        children.Add(drawable);
    }

    public void Remove(T drawable)
    {
        if (children.Remove(drawable))
            drawable.Parent = null;
    }

    public override void QueueDraw(OpenGlRenderer renderer)
    {
        for (int i = 0; i < children.Count; ++i)
            children[^(i + 1)].QueueDraw(renderer);
    }

    public override void Invalidate()
    {
        base.Invalidate();

        for (int i = 0; i < children.Count; ++i)
            children[^(i + 1)].Invalidate();
    }
}
