using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL;

public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint handle;
    private readonly OpenGlRenderer renderer;
    private GL context => renderer.Context;

    private Dictionary<string, (int location, GLEnum type)> uniforms = new();

    public Shader(OpenGlRenderer renderer, params uint[] shadersParts)
    {
        this.renderer = renderer;

        Debug.Assert(context is not null);

        handle = context.CreateProgram();

        foreach (uint part in shadersParts)
            context.AttachShader(handle, part);

        context.LinkProgram(handle);

        foreach (uint part in shadersParts)
            context.DetachShader(handle, part);

        //Check for linking errors.
        context.GetProgram(handle, GLEnum.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {context.GetProgramInfoLog(handle)}");

        cacheUniforms();
    }

    private void cacheUniforms()
    {
        Debug.Assert(context is not null);
        context.GetProgram(handle, GLEnum.ActiveUniforms, out int count);

        for (int i = 0; i < count; i++)
        {
            context.GetActiveUniform(handle, (uint)i, 100, out uint _, out int _, out GLEnum type, out string name);
            int location = context.GetUniformLocation(handle, name);

            uniforms.Add(name, (location, type));
        }
    }

    public void Bind() => renderer.BindShader(handle);

    public void SetUniform<T>(string name, T value) where T : unmanaged => SetUniform(name, ref value);

    public void SetUniform<T>(string name, ref T value) where T : unmanaged
    {
        Bind();
        //Setting a uniform on a shader using a name.
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        switch (value)
        {
            case bool boolVal:
                context.Uniform1(info.location, boolVal ? 1 : 0);
                return;

            case float floatVal:
                context.Uniform1(info.location, floatVal);
                return;

            case int intVal:
                context.Uniform1(info.location, intVal);
                return;

            case Vector2 vec2Val:
                context.Uniform2(info.location, ref vec2Val);
                return;

            case Vector3 vec3Val:
                context.Uniform3(info.location, ref vec3Val);
                return;

            case Vector4 vec4Val:
                context.Uniform4(info.location, ref vec4Val);
                return;

            case Matrix3 mat3Val:
                unsafe
                {
                    context.UniformMatrix3(info.location, 1, false, (float*)&mat3Val);
                }

                return;
        }
    }

    public void Dispose()
    {
        //Remember to delete the program when we are done.
        context.DeleteProgram(handle);
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        Dispose();
    }
}
