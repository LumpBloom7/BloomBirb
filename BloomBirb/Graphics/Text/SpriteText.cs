using System.Numerics;
using BloomBirb.Fonts.BMFont;
using BloomBirb.Graphics.Containers;
using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.Graphics.Text;

public class SpriteText : CompositeDrawable<DrawableSprite>
{
    public SpriteText(Shader shader, Font font)
    {
        spriteShader = shader;
        this.Font = font;
    }

    private Shader spriteShader;
    public Font Font { get; set; }

    private string text;

    public string Text
    {
        get => text;


        set
        {
            if (value.Equals(text)) return;
            layoutGlyphs(value, text);
            text = value;
        }
    }

    private List<DrawableSprite> glyphs = new List<DrawableSprite>();

    private void layoutGlyphs(string newText, string? oldText)
    {
        int changeBeginIndex = 0;

        /*// We don't wanna recalculate unchanged parts
        if (oldText is not null)
        {
            for (int i = 0; i < newText.Length && i < oldText.Length; ++i)
            {
                if (newText[i] != oldText[i])
                    break;

                changeBeginIndex++;
            }
        }*/

        foreach (var glyph in glyphs)
        {
            Remove(glyph);
        }
        glyphs.Clear();

        int currentPosX = 0;
        int baseLine = Font.getBaseLine();

        int yPos = Font.getLineHeight() - Font.getBaseLine();
        for (int i = changeBeginIndex; i < newText.Length; ++i)
        {
            var glyphTexture = Font.getCharacterTexture(newText[i]);
            var charInfo = Font.getCharacterInfo(newText[i]);
            var kerningAmount = (i > 0) ? Font.GetKerningAmount(newText[i - 1], newText[i]) : 0;

            var dip = Math.Max(0, charInfo.Height + charInfo.YOffset - baseLine);

            var glyph = new DrawableSprite(glyphTexture, spriteShader);
            //currentPosX += charInfo.XOffset;
            glyph.Position = new Vector2(currentPosX + charInfo.XOffset + kerningAmount , yPos - dip);
            glyph.Size = new Vector2(charInfo.Width, charInfo.Height);
            currentPosX += charInfo.XAdvance + kerningAmount;
            glyphs.Add(glyph);
            Add(glyph);
        }
    }
}
