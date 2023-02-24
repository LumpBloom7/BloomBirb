﻿using System.Reflection;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.ResourceStores;

public class TextureStore
{
    private static readonly string[] lookup_extensions =
    {
        "", // In case the user already specified an extension
        ".png",
        ".jpg",
        ".bmp"
    };

    private readonly List<TextureAtlas> atlases = new();
    private readonly List<Texture> textures = new();

    private readonly Dictionary<string, TextureUsage> textureCache = new();

    private readonly IResourceStore resources;

    private readonly string prefix;

    private readonly OpenGLRenderer renderer;

    private readonly int mipMapLevels;

    public TextureStore(OpenGLRenderer renderer, IResourceStore resourceStore, string prefix = "Textures", int mipLevels = 4)
    {
        this.renderer = renderer;
        resources = resourceStore;
        this.prefix = prefix;
        mipMapLevels = mipLevels;
    }

    public TextureUsage Get(string filename)
    {
        foreach (string fallbackExt in lookup_extensions)
        {
            string actualFilename = $"{filename}{fallbackExt}";
            if (!textureCache.TryGetValue(actualFilename, out var texture))
            {
                var stream = resources.Get($"{prefix}.{actualFilename}");

                if (stream is null)
                    continue;

                TextureUsage newTexture;

                using (var image = Image.Load<Rgba32>(stream))
                {
                    if (image.Width >= 2048 || image.Height >= 2048)
                        newTexture = addLargeTexture(image);
                    else
                        newTexture = addRegularTexture(image);

                }

                textureCache.Add(actualFilename, newTexture);
                return newTexture;
            }

            return texture;
        }

        return renderer.BlankTexture!;
    }

    private TextureUsage addLargeTexture(Image<Rgba32> image)
    {
        var texture = new Texture(renderer, mipMapLevels);
        texture.Initialize(image.Size());

        texture.BufferImageData(image, 0, 0);

        textures.Add(texture);
        return texture;
    }

    private TextureUsage addRegularTexture(Image<Rgba32> image)
    {
        foreach (var textureAtlas in atlases)
        {
            var textureUsage = textureAtlas.AddSubtexture(image);
            if (textureUsage is not null)
                return textureUsage;
        }

        // No fitting atlas, create new
        var newAtlas = new TextureAtlas(renderer, mipMapLevels);
        newAtlas.Initialize(new Size(4096, 4096));

        atlases.Add(newAtlas);

        return newAtlas.AddSubtexture(image)!;
    }
}
