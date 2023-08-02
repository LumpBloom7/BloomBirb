using System.Numerics;
using BloomFramework.Graphics;
using BloomFramework.Input.Handlers;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Textures;
using BloomFramework.ResourceStores;
using Silk.NET.Input;

namespace BlackLotus.Components;

public class TestDino : DrawableSprite, IKeyboardHandler, IMouseHandler
{
    private enum DinoAnimSet
    {
        idle,
        walk,
        kick,
        hurt,
    }

    private record AnimationSet(ITexture[] Frames, int Fps, bool Looping)
    {
        public readonly double Frametime = 1f / Fps;
    };

    private readonly Dictionary<DinoAnimSet, AnimationSet> animationSets = new();


    private DinoAnimSet currentSet = DinoAnimSet.idle;
    private int currentAnimIndex;
    private double timeElapsed;
    private AnimationSet animationSet;

    public TestDino(Shader shader, TextureStore textures) : base(shader)
    {
        animationSets.Add(DinoAnimSet.idle, loadAnimSet(textures, "Dino0.idle", 4, 3));
        animationSets.Add(DinoAnimSet.walk, loadAnimSet(textures, "Dino0.walk", 6, 6));
        animationSets.Add(DinoAnimSet.kick, loadAnimSet(textures, "Dino0.kick", 3, 6));
        animationSets.Add(DinoAnimSet.hurt, loadAnimSet(textures, "Dino0.hurt", 4, 4));

        animationSet = animationSets[DinoAnimSet.idle];
    }

    protected override void Update(double dt)
    {
        base.Update(dt);

        updateAnimation(dt);

        // Update position

        const float moveSpeed = 500;
        if (currentSet is DinoAnimSet.kick or DinoAnimSet.hurt)
            return;

        var normalizedMovement =
            movementDirection.LengthSquared() == 0 ? Vector2.Zero : Vector2.Normalize(movementDirection);

        Position += normalizedMovement * (float)(moveSpeed * dt);
    }

    private Vector2 movementDirection = Vector2.Zero;
    private bool kicking;
    private bool bullied;

    private void updateAnimation(double dt)
    {
        Scale = movementDirection.X switch
        {
            < 0 => new Vector2(-1, 1),
            > 0 => new Vector2(1, 1),
            _ => Scale
        };

        // Update current animation
        timeElapsed += dt;
        while (timeElapsed > animationSet.Frametime)
        {
            currentAnimIndex++;
            timeElapsed -= animationSet.Frametime;
        }

        if (bullied)
        {
            trySwitchSet(DinoAnimSet.hurt);
            bullied = false;
        }

        else if (kicking)
        {
            trySwitchSet(DinoAnimSet.kick);
        }
        else
        {
            if(currentSet is not (DinoAnimSet.kick or DinoAnimSet.hurt))
            {
                trySwitchSet(movementDirection.LengthSquared() == 0 ? DinoAnimSet.idle : DinoAnimSet.walk);
            }
            else
            {
                if(currentAnimIndex >0 && currentAnimIndex % animationSet.Frames.Length == 0)
                    trySwitchSet(movementDirection.LengthSquared() == 0 ? DinoAnimSet.idle : DinoAnimSet.walk);
            }
        }
        Texture = animationSet.Frames[currentAnimIndex% animationSet.Frames.Length];
    }

    private void trySwitchSet(DinoAnimSet animSet)
    {
        if (currentSet == animSet) return;

        currentAnimIndex = 0;
        timeElapsed = 0;
        animationSet = animationSets[animSet];
        currentSet = animSet;
    }

    public IKeyboardHandler? OnKeyPressed(IKeyboard keyboard, Key key, int scancode)
    {
        switch (key)
        {
            case Key.Left:
                movementDirection.X -= 1;
                return this;
            case Key.Right:
                movementDirection.X += 1;
                return this;
            case Key.Up:
                movementDirection.Y += 1;
                return this;
            case Key.Down:
                movementDirection.Y -= 1;
                return this;
            case Key.Z:
                kicking = true;
                return this;
        }

        return null;
    }

    public void OnKeyReleased(IKeyboard keyboard, Key key, int scancode)
    {
        switch (key)
        {
            case Key.Left:
                movementDirection.X += 1;
                break;
            case Key.Right:
                movementDirection.X -= 1;
                break;
            case Key.Up:
                movementDirection.Y -= 1;
                break;
            case Key.Down:
                movementDirection.Y += 1;
                break;
            case Key.Z:
                kicking = false;
                break;
        }
    }

    private static AnimationSet loadAnimSet(TextureStore textures, string prefix, int frameCount, int fps, bool looping = true)
    {
        ITexture[] frames = new ITexture[frameCount];

        for (int i = 0; i < frameCount; ++i)
        {
            frames[i] = textures.Get($"{prefix}{i}");
        }

        return new AnimationSet(frames, fps, looping);
    }

    public void OnClick(IMouse mouse, MouseButton button, Vector2 position)
    {
        if (DrawQuad.Contains(position))
            bullied = true;
    }
}
