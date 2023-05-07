namespace BloomFramework.Graphics;

[Flags]
public enum Anchor
{
    TopLeft = Top | Left,
    TopCentre = Top | Centre,
    TopRight = Top | Right,
    MiddleLeft = Middle  |Left,
    MiddleCentre = Middle | Centre,
    MiddleRight = Middle | Right,
    BottomLeft = Bottom  | Left,
    BottomCentre = Bottom | Centre,
    BottomRight = Bottom | Right,

    Left = 0,
    Centre = 1 << 0,
    Right = 1 << 1,
    Bottom = 0,
    Middle = 1 << 2,
    Top = 1 << 3,
}
