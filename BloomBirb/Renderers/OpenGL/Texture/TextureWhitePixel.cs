using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureWhitePixel : TextureUsage
{
    public TextureWhitePixel(OpenGLRenderer renderer)
        : base(new DummyTexture(renderer), new(0, 0, 1, 1), false)
    {
    }

    private class DummyTexture : ITexture
    {
        private OpenGLRenderer renderer;

        private static Texture? fallbackTex;
        private ITexture fallbackTexture
        {
            get
            {
                if (fallbackTex is null)
                {
                    fallbackTex = new Texture(renderer);
                    fallbackTex.Initialize(new(1, 1));
                    fallbackTex.SetPixel(0, 0, new Rgba32(255, 255, 255));
                }

                return fallbackTex;
            }
        }

        public DummyTexture(OpenGLRenderer renderer)
        {
            this.renderer = renderer;
        }

        public uint TextureHandle => renderer.GetBoundTexture()?.TextureHandle ?? fallbackTexture.TextureHandle;

        public SixLabors.ImageSharp.Size TextureSize => renderer.GetBoundTexture()?.TextureSize ?? fallbackTexture.TextureSize;

        public void Bind()
        {
            if (renderer.GetBoundTexture() is not TextureAtlas)
                fallbackTexture.Bind();
        }
    }
}
