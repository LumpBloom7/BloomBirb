using System.Numerics;
using BloomFramework.Input.Handlers;
using Silk.NET.Input;

namespace BloomFramework.Input;

public partial class InputManager
{
    private record struct MouseDownHandler(IMouse Keyboard, MouseButton Key, IMouseHandler TopLevelHandler);

    private void onMouseConnected(IMouse mouse)
    {
        mouse.MouseUp += onMouseUp;
        mouse.MouseDown += onMouseDown;
        mouse.MouseMove += onMouseMove;
        mouse.Click += onClick;
        mouse.DoubleClick += onDoubleClick;
        mouse.DoubleClickTime = 50;
    }

    private void onMouseDisconnected(IMouse mouse)
    {
        mouse.MouseUp -= onMouseUp;
        mouse.MouseDown -= onMouseDown;
        mouse.MouseMove -= onMouseMove;
        mouse.Click -= onClick;
        mouse.DoubleClick -= onDoubleClick;
    }

    private readonly Dictionary<MouseDownHandler, IMouseHandler> mouseDownHandlers = new();

    private void onMouseDown(IMouse mouse, MouseButton button)
    {
        foreach (var handler in mouseHandlers)
        {
            var result = handler.OnMouseDown(mouse, button);

            if (result is not null)
                mouseDownHandlers.Add(new(mouse, button, handler), result);
        }
    }

    private void onMouseUp(IMouse mouse, MouseButton button)
    {
        foreach (var handler in mouseHandlers)
        {
            if (!mouseDownHandlers.TryGetValue(new(mouse, button, handler), out var mouseHandler))
                continue;

            mouseHandler.OnMouseUp(mouse, button);
        }
    }

    private void onMouseMove(IMouse mouse, Vector2 position)
    {
        foreach (var handler in mouseHandlers)
            handler.OnMouseMove(mouse, toDrawSpace(position));
    }

    private void onClick(IMouse mouse, MouseButton button, Vector2 position)
    {
        foreach (var handler in mouseHandlers)
        {
            handler.OnClick(mouse, button, toDrawSpace(position));
        }
    }

    private void onDoubleClick(IMouse mouse, MouseButton button, Vector2 position)
    {
        foreach (var handler in mouseHandlers)
            handler.OnDoubleClick(mouse, button, toDrawSpace(position));
    }

    private Vector2 toDrawSpace(Vector2 position)
    {
        var windowSize = new Vector2(window.Size.X, window.Size.Y) * 0.5f;
        return (position / windowSize - Vector2.One) * new Vector2(1,-1);
    }
}
