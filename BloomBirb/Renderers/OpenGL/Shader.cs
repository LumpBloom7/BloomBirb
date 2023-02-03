using System;
using System.IO;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL;
public class Shader : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    public readonly uint Handle;
    private GL gl;

    private Dictionary<string, (int location, GLEnum type)> uniforms = new();

    public Shader(uint vertShader, uint fragShader)
    {
        gl = OpenGLRenderer.GlContext;

        if (vertShader == 0)
            throw new ArgumentNullException(nameof(vertShader));

        if (fragShader == 0)
            throw new ArgumentNullException(nameof(fragShader));


        Handle = gl.CreateProgram();
        gl.AttachShader(Handle, vertShader);
        gl.AttachShader(Handle, fragShader);
        gl.LinkProgram(Handle);

        //Check for linking errors.
        gl.GetProgram(Handle, GLEnum.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(Handle)}");

        gl.DetachShader(Handle, vertShader);
        gl.DetachShader(Handle, fragShader);
        cacheUniforms();
    }

    public Shader(Stream? vertStream, Stream? fragStream)
    {
        gl = OpenGLRenderer.GlContext;

        ArgumentNullException.ThrowIfNull(vertStream);
        ArgumentNullException.ThrowIfNull(fragStream);

        //Load the individual shaders.
        uint vertex = loadShader(ShaderType.VertexShader, vertStream);
        uint fragment = loadShader(ShaderType.FragmentShader, fragStream);

        //Create the shader program.
        Handle = gl.CreateProgram();

        //Attach the individual shaders.
        gl.AttachShader(Handle, vertex);
        gl.AttachShader(Handle, fragment);
        gl.LinkProgram(Handle);

        //Check for linking errors.
        gl.GetProgram(Handle, GLEnum.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(Handle)}");

        //Detach and delete the shaders
        gl.DetachShader(Handle, vertex);
        gl.DetachShader(Handle, fragment);

        cacheUniforms();
    }

    public void Use()
    {
        OpenGLRenderer.BindShader(this);
    }

    private void cacheUniforms()
    {
        gl.GetProgram(Handle, GLEnum.ActiveUniforms, out int count);

        for (int i = 0; i < count; i++)
        {
            gl.GetActiveUniform(Handle, (uint)i, 100, out uint _, out int _, out GLEnum type, out string name);
            int location = gl.GetUniformLocation(Handle, name);

            Console.WriteLine($"{name} {location} {type}");
            uniforms.Add(name, ((location, type)));
        }
    }

    #region SetUniforms

    //Uniforms are properties that applies to the entire geometry
    public void SetUniform(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform1(info.location, value);
    }

    public void SetUniform(string name, float value)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform1(info.location, value);
    }

    public void SetUniform(string name, bool value)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform1(info.location, value ? 1 : 0);
    }

    public void SetUniform(string name, Vector2 vector)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform2(info.location, vector);
    }

    public void SetUniform(string name, Vector3 vector)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform3(info.location, vector);
    }

    public void SetUniform(string name, Vector4 vector)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.Uniform4(info.location, vector);
    }

    public unsafe void SetUniform(string name, Matrix3 matrix)
    {
        if (!uniforms.TryGetValue(name, out var info))
            throw new Exception($"{name} uniform not found on shader.");

        gl.UniformMatrix3(info.location, 1, false, (float*)&matrix);
    }

    #endregion

    public void Dispose()
    {
        //Remember to delete the program when we are done.
        gl.DeleteProgram(Handle);

        GC.SuppressFinalize(this);
    }

    private uint loadShader(ShaderType type, Stream dataStream)
    {
        string src;
        using (var sr = new StreamReader(dataStream))
        {
            src = sr.ReadToEnd();
        }

        //To load a single shader we need to:
        //1) Load the shader from a file.
        //2) Create the handle.
        //3) Upload the source to opengl.
        //4) Compile the shader.
        //5) Check for errors.

        uint handle = gl.CreateShader(type);
        gl.ShaderSource(handle, src);
        gl.CompileShader(handle);
        string infoLog = gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}
