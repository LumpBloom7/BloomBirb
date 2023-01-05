using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Graphics;

public class Texture : IDisposable
{
    private uint handle;
    private GL gl;

    public static readonly Texture BLANK;

    static Texture()
    {
        BLANK = new Texture();
    }

    // Creates blank texture
    private unsafe Texture()
    {
        gl = OpenGLRenderer.GlContext;

        handle = gl.GenTexture();
        Bind();
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 1, 1, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, null);

        var img = new Image<Rgba32>(Configuration.Default, 1, 1);
        img.ProcessPixelRows(accessor =>
        {
            accessor.GetRowSpan(0)[0] = new Rgba32(255, 255, 255, 255);
            fixed (void* data = accessor.GetRowSpan(0))
                gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte,
                    data);
        });

        setParameters();
    }

    public unsafe Texture(Stream? stream)
    {
        gl = OpenGLRenderer.GlContext;

        handle = gl.GenTexture();
        Bind();

        using (var img = Image.Load<Rgba32>(stream))
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, null);

            img.ProcessPixelRows(accessor =>
            {
                for (int i = 0; i < accessor.Height; ++i)
                    fixed (void* data = accessor.GetRowSpan(i))
                        gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, i, (uint)accessor.Width, 1U, PixelFormat.Rgba,
                            PixelType.UnsignedByte, data);
            });
        }

        setParameters();
    }

    private void setParameters()
    {
        // Set some parameters
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        gl.ActiveTexture(textureUnit);
        gl.BindTexture(TextureTarget.Texture2D, handle);
    }

    public void Dispose()
    {
        gl.DeleteTexture(handle);

        GC.SuppressFinalize(this);
    }
}
