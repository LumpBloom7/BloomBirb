using System.Text;
using BloomFramework.Fonts.BMFont.DescriptorBlocks;
using BloomFramework.Renderers.OpenGL.Textures;
using BloomFramework.ResourceStores;
using Silk.NET.Maths;

namespace BloomFramework.Fonts.BMFont;

public class Font
{
    public readonly FontInfo FontInfo;
    public readonly Common CommonInfo;
    public readonly string[] PageNames = null!;
    public readonly Dictionary<uint, CharacterInfo> Characters = new Dictionary<uint, CharacterInfo>();
    public readonly Dictionary<uint, Dictionary<uint, KerningPair>> KerningPairs = new();

    private readonly TextureStore textures;

    public Font(Stream stream, TextureStore backingTextureStore)
    {
        textures = backingTextureStore;

        if (!isValidFontStream(stream))
            throw new InvalidDataException("Font provided is not a valid BMF format.");

        while (stream.CanRead)
        {
            int blockType = stream.ReadByte();

            if (blockType == -1) // End of stream
                break;

            byte[] length = new byte[4];
            stream.ReadExactly(length, 0, 4);
            int blockLength = length[0] + (length[1] << 8) + (length[2] << 16) + (length[3] << 24);
            byte[] blockData = new byte[blockLength];
            stream.ReadExactly(blockData, 0, blockLength);

            switch (blockType)
            {
                case 1:
                    FontInfo = new FontInfo(blockData);
                    break;

                case 2:
                    CommonInfo = new Common(blockData);
                    break;

                case 3:
                    PageNames = new string[CommonInfo.Pages];
                    int stringLength = blockLength / PageNames.Length - 1;
                    for (int i = 0; i < PageNames.Length; ++i)
                    {
                        int start = PageNames.Length * i;
                        PageNames[i] = Encoding.Default.GetString(blockData.AsSpan(start, stringLength));
                    }

                    break;
                case 4:
                {
                    int n = blockLength / 20;
                    for (int i = 0; i < n; ++i)
                    {
                        var charInfo = new CharacterInfo(blockData.AsSpan(i * 20, 20));
                        Characters.Add(charInfo.Id, charInfo);
                    }

                    break;
                }

                case 5:
                {
                    int n = blockLength / 10;
                    for (int i = 0; i < n; ++i)
                    {
                        var kerningPair = new KerningPair(blockData.AsSpan(i * 10, 10));

                        if (!KerningPairs.TryGetValue(kerningPair.First, out var nested))
                            KerningPairs.Add(kerningPair.First, nested = new Dictionary<uint, KerningPair>());

                        nested.Add(kerningPair.Second, kerningPair);
                    }

                    break;
                }
            }
        }
    }

    public ITextureUsage GetCharacterTexture(char character)
    {
        if (!Characters.TryGetValue(character, out var charInfo))
            charInfo = Characters['?'];

        ushort page = charInfo.PageNumber;

        var texture = textures.Get(PageNames[page]);
        return new TextureUsage(texture.BackingTexture,
            new Rectangle<int>(charInfo.XPosition, charInfo.YPosition, charInfo.Width, charInfo.Height),
            true);
    }

    public CharacterInfo GetCharacterInfo(char character)
    {
        if (!Characters.TryGetValue(character, out var charInfo))
            charInfo = Characters['?'];

        return charInfo;
    }

    public int GetKerningAmount(char firstChar, char secondChar)
    {
        if (!KerningPairs.TryGetValue(firstChar, out var dictionary))
            return 0;

        return !dictionary.TryGetValue(secondChar, out var kerningPair) ? 0 : kerningPair.Amount;
    }

    public int GetLineHeight() => CommonInfo.LineHeight;

    public int GetBaseLine() => CommonInfo.Baseline;

    private static readonly byte[] valid_header_template = { 66, 77, 70, 3 };

    private static bool isValidFontStream(Stream stream)
    {
        if (!stream.CanRead)
            return false;

        byte[] header = new byte[4];

        stream.ReadExactly(header, 0, 4);

        return header.SequenceEqual(valid_header_template);
    }
}
