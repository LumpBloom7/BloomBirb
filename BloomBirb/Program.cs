﻿using BloomBirb.Extensions;
using BloomBirb.Graphics;
using BloomBirb.ResourceStores;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Texture = BloomBirb.Graphics.Texture;

namespace BloomBirb
{
    public class Program
    {
        private static IWindow? window;
        private static GL? gl;

        //Our new abstracted objects, here we specify what the types are.
        private static BufferObject<float>? vbo;
        private static BufferObject<uint>? ebo;
        private static VertexArrayObject<float, uint>? vao;
        private static BloomBirb.Graphics.Shader? shader;

        // Note to self Screen origin is bottom left, (0,0) is centre
        // UV origin is topleft.
        private static readonly float[] vertices =
        {
            //X    Y      Z     U   V
            -0.5f, -0.5f , 1, 0, 1,
             0.5f, -0.5f , 1, 1, 1,
            -0.5f,  0.5f , 1, 0, 0,
             0.5f,  0.5f , 1, 1, 0
        };

        private static readonly uint[] indices =
        {
            0, 1, 2,
            1, 3, 2
        };

        private static EmbeddedResourceStore? resources = new();

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "You spin me right round baby right round. Like a record baby right round round round.";
            window = Window.Create(options);

            window.Load += onLoad;
            window.Render += onRender;
            window.Closing += onClose;

            window.Run();
        }

        private static Texture? texture;
        private static void onLoad()
        {
            IInputContext input = window?.CreateInput()!;
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += onKeyDown;
            }

            gl = OpenGLRenderer.CreateContext(window!);

            //Instantiating our new abstractions
            ebo = new BufferObject<uint>(indices, BufferTargetARB.ElementArrayBuffer);
            vbo = new BufferObject<float>(vertices, BufferTargetARB.ArrayBuffer);
            vao = new VertexArrayObject<float, uint>(vbo, ebo);

            //Telling the VAO object how to lay out the attribute pointers
            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            shader = resources?.Shaders.Get("Texture", "Texture");

            texture = resources?.Textures.Get("sticky");
        }

        private static unsafe void onRender(double obj)
        {
            gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

            //Binding and using our VAO and shader.
            vao?.Bind();
            shader?.Use();
            //Setting a uniform.
            texture?.Bind();

            gl?.Enable(GLEnum.Blend);
            gl?.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            float time = ((DateTime.Now.Second % 5) + DateTime.Now.Millisecond / 1000f) / 5f;
            float timeRot = (DateTime.Now.Second + DateTime.Now.Millisecond / 1000f) / 60f;

            float rot = MathF.Sin(timeRot * 6.28f) * 360;
            float shear = MathF.Sin(time * 6.28f + 1) * 0.25f;
            float shearY = MathF.Sin(time * 6.28f + 2) * 0.25f;
            float sclX = MathF.Sin(time * 6.28f + 3) * 1;
            float sclY = MathF.Sin(time * 6.28f + 4) * 1;
            float trnsX = MathF.Sin(time * 6.28f + 5) * 0.5f;
            float trnsY = MathF.Sin(time * 6.28f + 6) * 0.5f;

            var t = Matrix4X4.Transpose(Matrix4X4<float>.Identity.RotateDegrees(rot).Shear(shear, shearY).Scale(0.5f + sclX, 0.5f + sclY).Translate(trnsX, trnsY));

            shader?.SetUniform("u_TransMat", t);
            shader?.SetUniform("u_Texture0", 0);
            shader?.SetUniform("u_Circle", 0);
            shader?.SetUniform("u_ScreenSpaceCentreX", 400f);
            shader?.SetUniform("u_ScreenSpaceCentreY", 300f);
            shader?.SetUniform("u_CircleRadius", 150f);

            gl?.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private static void onClose()
        {
            //Remember to dispose all the instances.
            vbo?.Dispose();
            ebo?.Dispose();
            vao?.Dispose();
            shader?.Dispose();
        }

        private static void onKeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                window?.Close();
            }
        }
    }
}
