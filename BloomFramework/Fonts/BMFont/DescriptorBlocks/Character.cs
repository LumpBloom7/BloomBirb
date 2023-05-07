namespace BloomFramework.Fonts.BMFont.DescriptorBlocks;

public readonly record struct CharacterInfo
{
    [Flags]
    public enum ColorChannel
    {
        B = 1 << 0,
        G = 1 << 1,
        R = 1 << 2,
        A = 1 << 3,
        All = 15
    }

    public uint Id { get; private init; }
    public ushort XPosition { get; private init; }
    public ushort YPosition { get; private init; }
    public ushort Width { get; private init; }
    public ushort Height { get; private init; }
    public short XOffset { get; private init; }
    public short YOffset { get; private init; }
    public short XAdvance { get; private init; }
    public ushort PageNumber { get; private init; }
    public ColorChannel Channel { get; private init; }

    public CharacterInfo(ReadOnlySpan<byte> span)
    {
        Id = (uint)(span[0] + (span[1] << 8) + (span[2] << 16) + (span[3] << 24));
        XPosition = (ushort)(span[4] + (span[5] << 8));
        YPosition = (ushort)(span[6] + (span[7] << 8));
        Width = (ushort)(span[8] + (span[9] << 8));
        Height = (ushort)(span[10] + (span[11] << 8));
        XOffset = (short)(span[12] + (span[13] << 8));
        YOffset = (short)(span[14] + (span[15] << 8));
        XAdvance = (short)(span[16] + (span[17] << 8));
        PageNumber = span[18];
        Channel = (ColorChannel)span[19];
    }
}
