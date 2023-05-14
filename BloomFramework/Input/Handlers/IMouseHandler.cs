using System.Numerics;
using Silk.NET.Input;

namespace BloomFramework.Input.Handlers;

public interface IMouseHandler
{
    void OnMouseUp(IMouse mouse, MouseButton button) { }

    IMouseHandler? OnMouseDown(IMouse mouse, MouseButton button) { return null; }

    void OnMouseMove(IMouse mouse, Vector2 position) { }

    void OnClick(IMouse mouse, MouseButton button, Vector2 position) { }

    void OnDoubleClick(IMouse mouse, MouseButton button, Vector2 position) { }
}
