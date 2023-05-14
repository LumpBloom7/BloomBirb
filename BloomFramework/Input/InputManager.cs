using BloomFramework.Graphics;
using BloomFramework.Input.Handlers;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace BloomFramework.Input;

public partial class InputManager
{
    private readonly IInputContext input;

    private readonly HashSet<IKeyboardHandler> keyboardHandlers = new();
    private HashSet<IMouseHandler> mouseHandlers = new();
    private HashSet<IJoystickHandler> joystickHandlers = new();

    public IReadOnlyList<IKeyboard> Keyboards => input.Keyboards;
    public IReadOnlyList<IMouse> Mice => input.Mice;
    public IReadOnlyList<IJoystick> Joysticks => input.Joysticks;

    public InputManager(IWindow window)
    {
        input = window.CreateInput();

        foreach (var keyboard in input.Keyboards)
            deviceConnected(keyboard);

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
                break;
            case IJoystick joystick:
                break;
        }
    }

    // Maybe declaring scope would be helpful, so that we don't send event to drawables that don't care
    public void RegisterInputConsumer<T>(T drawable) where T : class
    {
        switch (drawable)
        {
            case IKeyboardHandler kh:
                keyboardHandlers.Add(kh);
                break;
            case IMouseHandler mh:
                mouseHandlers.Add(mh);
                break;
            case IJoystickHandler jh:
                joystickHandlers.Add(jh);
                break;
        }
    }

    public void UnregisterInputConsumer<T>(T drawable) where T : class
    {
        switch (drawable)
        {
            case IKeyboardHandler kh:
                keyboardHandlers.Remove(kh);
                break;
            case IMouseHandler mh:
                mouseHandlers.Remove(mh);
                break;
            case IJoystickHandler jh:
                joystickHandlers.Remove(jh);
                break;
        }
    }

    //public bool IsInputConsumer<T>(Drawable drawable) => consumers.Contains(drawable);
}
