using System.Text;
using BloomBirb.Font.BMFont.DescriptorBlocks;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Textures;

namespace BloomBirb.Font.BMFont;

public class Font
{
    public readonly FontInfo FontInfo;
    public readonly Common CommonInfo;
    public readonly string[] PageNames;
    public readonly Dictionary<uint, CharacterInfo> Characters = new Dictionary<uint, CharacterInfo>();
    public readonly Dictionary<uint, Dictionary<uint, KerningPair>> KerningPairs = new();

    private readonly Texture[] pageTextures;

    private readonly OpenGLRenderer renderer;

    public Font(OpenGLRenderer renderer, Stream stream)
    {
        this.renderer = renderer;

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

        pageTextures = new Texture[CommonInfo.Pages];
    }

    private static readonly byte[] valid_header_template = { 66, 77, 70, 3 };

    private static bool isValidFontStream(Stream stream)
    {
        if (!stream.CanRead)
            return false;

        var header = new byte[4];

        stream.ReadExactly(header, 0, 4);

        return header.SequenceEqual(valid_header_template);
    }
}
