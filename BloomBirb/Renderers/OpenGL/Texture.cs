﻿using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL;

public class Texture : IDisposable
{
    private readonly uint handle;

    private readonly OpenGLRenderer renderer;
    private GL context => renderer.Context!;

    public bool HasTransparencies { get; private set; }

    // Creates blank texture
    public unsafe Texture(OpenGLRenderer renderer)
    {
        this.renderer = renderer;

        handle = context.GenTexture();

        Bind();

        context.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 1, 1, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, new Rgba32(1f, 1f, 1f));

        setParameters();
    }

    public unsafe Texture(OpenGLRenderer renderer, Stream? stream)
    {
        this.renderer = renderer;
        handle = context.GenTexture();
        Bind();

        using (var img = Image.Load<Rgba32>(stream))
        {
            context.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, null);

            img.ProcessPixelRows(accessor =>
            {
                for (int i = 0; i < accessor.Height; ++i)
                {
                    var rowSpan = accessor.GetRowSpan(i);
                    if (!HasTransparencies)
                    {
                        foreach (var pixel in rowSpan)
                            if (pixel.A < 255)
                            {
                                HasTransparencies = true;
                                break;
                            }
                    }

                    fixed (void* data = accessor.GetRowSpan(i))
                        context.TexSubImage2D(TextureTarget.Texture2D, 0, 0, i, (uint)accessor.Width, 1U,
                            PixelFormat.Rgba,
                            PixelType.UnsignedByte, data);
                }
            });
        }

        setParameters();
    }

    private void setParameters()
    {
        // Set some parameters
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)GLEnum.LinearMipmapLinear);
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        context.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0) => renderer.BindTexture(handle, textureUnit);

    public void Dispose()
    {
        context.DeleteTexture(handle);

        GC.SuppressFinalize(this);
    }

    ~Texture()
    {
        Dispose();
    }
}
