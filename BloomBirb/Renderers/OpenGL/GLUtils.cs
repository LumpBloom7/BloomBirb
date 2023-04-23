using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL;

public static class GlUtils
{
    public static void SetVao<T>(GL gl) where T : IVertex
    {
        int stride = Unsafe.SizeOf<T>();

        var vertexMembers = getVertexMembers(typeof(T));

        for(int i = 0 ; i < vertexMembers.Count;++i){
            (var attribute, nint offset) = vertexMembers[i];

            unsafe
            {
                gl.VertexAttribPointer((uint)i, attribute.Count, (GLEnum)toGlVertexAttribPointerType(attribute.AttributeType), false, (uint)stride, (void*)offset);
            }

            gl.EnableVertexAttribArray((uint)i);
        }
    }

    private static VertexAttribPointerType toGlVertexAttribPointerType(VertexAttributeType type)
        => (VertexAttribPointerType)((int)type + (int)VertexAttribPointerType.Byte);

    private static List<(VertexMemberAttribute attribute, nint offset)> getVertexMembers(Type type ,nint offset = 0)
    {
        List<(VertexMemberAttribute attribute, nint offset)> entries = new ();
        var vertexMembers = type.GetFields();

        foreach(var vertexMember in vertexMembers){
            nint memberOffset = Marshal.OffsetOf(type, vertexMember.Name) + offset;

            if(vertexMember.FieldType.IsAssignableTo(typeof(IVertex)))
            {
                entries.AddRange(getVertexMembers(vertexMember.FieldType, memberOffset));
                continue;
            }

            var attrib = vertexMember.GetCustomAttribute<VertexMemberAttribute>();
            // This field has the attribute, use it
            if(attrib is not null)
                entries.Add(new (attrib, memberOffset));
        }

        return entries;
    }
}
