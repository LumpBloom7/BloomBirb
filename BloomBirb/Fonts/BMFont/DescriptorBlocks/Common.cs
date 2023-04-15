namespace  BloomBirb.Fonts.BMFont.DescriptorBlocks;

public readonly record struct Common
{
    public enum ChannelUsage
    {
        Glyph = 0,
        Outline = 1,
        OutlinedGlyph = 2,
        Zero = 3,
        One = 4
    };

    public ushort LineHeight { get; private init; }
    public ushort Baseline { get; private init; }
    public ushort ScaleW { get; private init; }
    public ushort ScaleH { get; private init; }
    public ushort Pages { get; private init; }
    public bool Packed { get; private init; }
    public ChannelUsage AlphaChannel { get; private init; }
    public ChannelUsage RedChannel { get; private init; }
    public ChannelUsage GreenChannel { get; private init; }
    public ChannelUsage BlueChannel { get; private init; }

    public Common(ReadOnlySpan<byte> span)
    {
        LineHeight = (ushort)(span[0] + (span[1] << 8));
        Baseline = (ushort)(span[2] + (span[3] << 8));
        ScaleW = (ushort)(span[4] + (span[5] << 8));
        ScaleH = (ushort)(span[6] + (span[7] << 8));
        Pages = (ushort)(span[8] + (span[9] << 8));
        Packed = (span[10] & 1) == 1;
        AlphaChannel = (ChannelUsage)span[11];
        RedChannel = (ChannelUsage)span[12];
        GreenChannel = (ChannelUsage)span[13];
        BlueChannel = (ChannelUsage)span[14];
    }
}
