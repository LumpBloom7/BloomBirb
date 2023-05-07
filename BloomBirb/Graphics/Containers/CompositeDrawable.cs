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

    public void AddRange(T[] drawables)
    {
        foreach (var drawable in drawables)
            Add(drawable);
    }

    public void Remove(T drawable)
    {
        children.Remove(drawable);
        drawable.Parent = null;
    }

    internal override void LoadInternal()
    {
        base.LoadInternal();

        foreach (var child in children)
            child.LoadInternal();
    }

    internal override void UpdateInternal(double dt)
    {
        base.UpdateInternal(dt);

        foreach (var child in children)
            child.UpdateInternal(dt);
    }

    public override void QueueDraw(OpenGlRenderer renderer)
    {
        base.QueueDraw(renderer);

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
