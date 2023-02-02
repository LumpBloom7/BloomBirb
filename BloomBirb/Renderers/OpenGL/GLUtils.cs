using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL;

public static class GLUtils
{
    public static void SetVAO<T>(GL gl) where T : IVertex
    {
        int stride = T.Size;
        int offset = 0;

        var layout = T.Layout;
        for (uint i = 0; i < layout.Length; ++i)
        {
            var currentAttribEntry = layout[i];
            unsafe
            {
                gl.VertexAttribPointer(i, currentAttribEntry.count, (GLEnum)toGLVertexAttribPointerType(currentAttribEntry.type), false, (uint)stride, (void*)offset);
            }
            offset += currentAttribEntry.count * currentAttribEntry.typeSize;
            gl.EnableVertexAttribArray(i);
        }
    }

    private static VertexAttribPointerType toGLVertexAttribPointerType(VertexAttributeType type)
    {
        int typeEnum = (int)type;
        typeEnum += (int)VertexAttribPointerType.Byte;

        return (VertexAttribPointerType)typeEnum;
    }
}
