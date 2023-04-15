namespace BloomBirb.Fonts.BMFont.DescriptorBlocks;

public readonly record struct KerningPair
{
    public uint First { get; private init; }
    public uint Second { get; private init; }
    public short Amount { get; private init; }

    public KerningPair(ReadOnlySpan<byte> span)
    {
        First = (uint)(span[0] + (span[1] << 8) + (span[2] << 16) + (span[3] << 24));
        Second = (uint)(span[4] + (span[5] << 8) + (span[6] << 16) + (span[7] << 24));
        Amount = (short)(span[8] + (span[9] << 8));
    }
}
