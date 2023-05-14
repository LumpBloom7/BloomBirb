using BloomFramework.Graphics;
using BloomFramework.Input.Handlers;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace BloomFramework.Input;

public partial class InputManager
{
    private readonly IInputContext input;
    private readonly IWindow window;

    private readonly HashSet<IKeyboardHandler> keyboardHandlers = new();
    private HashSet<IMouseHandler> mouseHandlers = new();
    private HashSet<IJoystickHandler> joystickHandlers = new();

    public IReadOnlyList<IKeyboard> Keyboards => input.Keyboards;
    public IReadOnlyList<IMouse> Mice => input.Mice;
    public IReadOnlyList<IJoystick> Joysticks => input.Joysticks;

    public InputManager(IWindow window)
    {
        this.window = window;
        input = window.CreateInput();

        foreach (var keyboard in Keyboards)
            onKeyboardConnected(keyboard);

        foreach (var mouse in Mice)
            onMouseConnected(mouse);

        input.ConnectionChanged += onConnectionChanged;
    }

    private void onConnectionChanged(IInputDevice device, bool connected)
    {
        if (connected)
            deviceConnected(device);
        else
            deviceDisconnected(device);
    }

    private void deviceConnected(IInputDevice device)
    {
        // Register handlers
        switch (device)
        {
            case IKeyboard keyboard:
                onKeyboardConnected(keyboard);
                break;
            case IMouse mouse:
                onMouseConnected(mouse);
                break;
            case IJoystick joystick:
                break;
        }
    }

    private void deviceDisconnected(IInputDevice device)
    {
        // Register handlers
        switch (device)
        {
            case IKeyboard keyboard:
                onKeyboardDisconnected(keyboard);
                break;
            case IMouse mouse:
                onMouseDisconnected(mouse);
                break;
            case IJoystick joystick:
                break;
        }
    }

    // Maybe declaring scope would be helpful, so that we don't send event to drawables that don't care
    public void RegisterInputConsumer<T>(T drawable) where T : class
    {
        if (drawable is IKeyboardHandler kh)
            keyboardHandlers.Add(kh);

        if (drawable is IMouseHandler mh)
            mouseHandlers.Add(mh);

        if (drawable is IJoystickHandler jh)
            joystickHandlers.Add(jh);
    }

    public void UnregisterInputConsumer<T>(T drawable) where T : class
    {
        if (drawable is IKeyboardHandler kh)
            keyboardHandlers.Remove(kh);

        if (drawable is IMouseHandler mh)
            mouseHandlers.Remove(mh);

        if (drawable is IJoystickHandler jh)
            joystickHandlers.Remove(jh);
    }

    //public bool IsInputConsumer<T>(Drawable drawable) => consumers.Contains(drawable);
}
