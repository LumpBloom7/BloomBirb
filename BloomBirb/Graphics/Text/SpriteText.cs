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
        Font = font;
    }

    private Shader spriteShader;
    public Font Font { get; set; }

    private string text = "";

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

    private List<int> cursorPosList = new List<int>();

    private List<DrawableSprite> glyphs = new List<DrawableSprite>();

    private void layoutGlyphs(string newText, string oldText)
    {
        int changeBeginIndex = 0;
        int cursorPos = 0;

        // Find change start index
        for (int i = 0; i < newText.Length && i < oldText.Length; ++i)
        {
            if (newText[i] != oldText[i])
                break;

            changeBeginIndex++;
            cursorPos = cursorPosList[i];
        }

        int baseLine = Font.GetBaseLine();
        int yPos = Font.GetLineHeight() - Font.GetBaseLine();

        // Clear excess sprites
        for (int i = glyphs.Count - 1; i >= newText.Length-1; --i)
        {
            Remove(glyphs[i]);
            glyphs.RemoveAt(i);
            cursorPosList.RemoveAt(i);
        }

        for (int i = changeBeginIndex; i < newText.Length; ++i)
        {
            // need new sprite
            if (glyphs.Count == i)
            {
                var sprite = new DrawableSprite(spriteShader);
                glyphs.Add(sprite);
                Add(sprite);
                cursorPosList.Add(0);
            }

            var glyphTexture = Font.GetCharacterTexture(newText[i]);
            var charInfo = Font.GetCharacterInfo(newText[i]);
            int kerningAmount = (i > 0) ? Font.GetKerningAmount(newText[i - 1], newText[i]) : 0;
            int dip = Math.Max(0, charInfo.Height + charInfo.YOffset - baseLine);

            glyphs[i].Texture = glyphTexture;
            glyphs[i].Position = new Vector2(cursorPos + charInfo.XOffset + kerningAmount, yPos - dip);
            glyphs[i].Size = new Vector2(charInfo.Width, charInfo.Height);

            cursorPos += charInfo.XAdvance + kerningAmount;
            cursorPosList[i] = cursorPos;
        }



    }
}
