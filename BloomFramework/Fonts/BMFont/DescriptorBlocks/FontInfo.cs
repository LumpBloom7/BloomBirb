using System.Text;

namespace BloomFramework.Fonts.BMFont.DescriptorBlocks;

public readonly record struct FontInfo
{
    [Flags]
    public enum FontFlags
    {
        None = 0,
        Smooth = 1 << 7,
        Unicode = 1 << 6,
        Italic = 1 << 5,
        Bold = 1 << 4,
        FixedHeight = 1 << 3,
    }

    public short FontSize { get; private init; }
    public FontFlags BitField { get; private init; }
    public ushort Charset { get; private init; }
    public ushort StretchH { get; private init; }
    public ushort Antialiasing { get; private init; }
    public byte PaddingUp { get; private init; }
    public byte PaddingRight { get; private init; }
    public byte PaddingDown { get; private init; }
    public byte PaddingLeft { get; private init; }
    public byte SpacingHoriz { get; private init; }
    public byte SpacingVert { get; private init; }
    public byte Outline { get; private init; }
    public string FontName { get; private init; }

    public FontInfo(ReadOnlySpan<byte> span)
    {
        FontSize = (short)(span[0] + (span[1] << 8));
        BitField = (FontFlags)span[2];
        Charset = span[3];
        StretchH = (ushort)(span[4] + (span[5] << 8));
        Antialiasing = span[6];
        PaddingUp = span[7];
        PaddingRight = span[8];
        PaddingDown = span[9];
        PaddingLeft = span[10];
        SpacingHoriz = span[11];
        SpacingVert = span[12];
        Outline = span[13];
        FontName = Encoding.Default.GetString(span.Slice(14, span.Length - 15)); // Remove the trailing null char
    }

}
