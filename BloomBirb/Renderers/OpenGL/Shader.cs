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
    private readonly uint handle;
    private GL gl;

    public Shader(uint vertShader, uint fragShader)
    {
        gl = OpenGLRenderer.GlContext;

        if (vertShader == 0)
            throw new ArgumentNullException(nameof(vertShader));

        if (fragShader == 0)
            throw new ArgumentNullException(nameof(fragShader));


        handle = gl.CreateProgram();
        gl.AttachShader(handle, vertShader);
        gl.AttachShader(handle, fragShader);
        gl.LinkProgram(handle);

        //Check for linking errors.
        gl.GetProgram(handle, GLEnum.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(handle)}");

        gl.DetachShader(handle, vertShader);
        gl.DetachShader(handle, fragShader);
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
        handle = gl.CreateProgram();
        //Attach the individual shaders.
        gl.AttachShader(handle, vertex);
        gl.AttachShader(handle, fragment);
        gl.LinkProgram(handle);
        //Check for linking errors.
        gl.GetProgram(handle, GLEnum.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(handle)}");

        //Detach and delete the shaders
        gl.DetachShader(handle, vertex);
        gl.DetachShader(handle, fragment);
        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);
    }

    public void Use()
    {
        //Using the program
        gl.UseProgram(handle);
    }

    #region SetUniforms

    //Uniforms are properties that applies to the entire geometry
    public void SetUniform(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }

    public void SetUniform(string name, bool value)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value ? 1 : 0);
    }

    public void SetUniform(string name, Vector2 vector)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform2(location, vector);
    }
    public void SetUniform(string name, Vector3 vector)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform3(location, vector);
    }

    public void SetUniform(string name, Vector4 vector)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform4(location, vector);
    }

    public unsafe void SetUniform(string name, Matrix4X4<float> matrix)
    {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.UniformMatrix4(location, 1, false, (float*)&matrix);
    }

    #endregion

    public void Dispose()
    {
        //Remember to delete the program when we are done.
        gl.DeleteProgram(handle);

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
