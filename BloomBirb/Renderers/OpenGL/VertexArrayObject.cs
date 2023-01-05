using System;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL
{
    //The vertex array object abstraction.
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint handle;
        private GL gl;

        public VertexArrayObject(BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            //Saving the GL instance.
            gl = OpenGLRenderer.GlContext;

            //Setting out handle and binding the VBO and EBO to this VAO.
            handle = gl.GenVertexArray();
            Bind();
            vbo.Bind();
            ebo.Bind();
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
        {
            //Setting up a vertex attribute pointer
            gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offSet * sizeof(TVertexType)));
            gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            //Binding the vertex array.
            gl.BindVertexArray(handle);
        }

        public void Dispose()
        {
            //Remember to dispose this object so the data GPU side is cleared.
            //We dont delete the VBO and EBO here, as you can have one VBO stored under multiple VAO's.
            gl.DeleteVertexArray(handle);
            GC.SuppressFinalize(this);
        }
    }
}
