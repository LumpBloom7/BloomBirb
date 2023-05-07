namespace BloomFramework.Graphics.Containers;

public class Container : Container<Drawable> { }
public class Container<T> : CompositeDrawable<T> where T : Drawable { }
