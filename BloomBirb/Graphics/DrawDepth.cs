namespace BloomBirb.Graphics;

public static class DrawDepth
{
    public static float NextDepth { get; private set; }

    public static void Reset() => NextDepth = -1;
    public static void Increment() => NextDepth += 0.001f;
}
