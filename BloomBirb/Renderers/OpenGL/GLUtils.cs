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
        for (int i = 0; i < layout.Count; ++i)
        {
            var currentAttribEntry = layout[i];
            unsafe
            {
                gl.VertexAttribPointer((uint)i, currentAttribEntry.Count,
                    (GLEnum)toGLVertexAttribPointerType(currentAttribEntry.Type), false, (uint)stride, (void*)offset);
            }

            offset += currentAttribEntry.Count * sizeOf(currentAttribEntry.Type);
            gl.EnableVertexAttribArray((uint)i);
        }
    }

    private static VertexAttribPointerType toGLVertexAttribPointerType(VertexAttributeType type)
        => (VertexAttribPointerType)((int)type + (int)VertexAttribPointerType.Byte);

    private static int sizeOf(VertexAttributeType type) => type switch
    {
        VertexAttributeType.Byte or VertexAttributeType.UnsignedByte => sizeof(byte),
        VertexAttributeType.Short or VertexAttributeType.UnsignedShort => sizeof(short),
        VertexAttributeType.Int or VertexAttributeType.UnsignedInt => sizeof(int),
        VertexAttributeType.Float => sizeof(float),
        VertexAttributeType.Double => sizeof(double),
        _ => 0,
    };
}
