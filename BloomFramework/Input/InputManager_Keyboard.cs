using BloomFramework.Input.Handlers;
using Silk.NET.Input;

namespace BloomFramework.Input;

public partial class InputManager
{
    private record struct KeyPressHandler(IKeyboard Keyboard, Key Key, IKeyboardHandler TopLevelHandler);

    private void onKeyboardConnected(IKeyboard keyboard)
    {
        keyboard.KeyChar += onKeyboardChar;
        keyboard.KeyDown += onKeyPressed;
        keyboard.KeyUp += onKeyReleased;
    }

    private void onKeyboardDisconnected(IKeyboard keyboard)
    {
        keyboard.KeyChar -= onKeyboardChar;
        keyboard.KeyDown -= onKeyPressed;
        keyboard.KeyUp -= onKeyReleased;
    }

    private Dictionary<KeyPressHandler, IKeyboardHandler> keypressHandlers = new();

    private void onKeyboardChar(IKeyboard keyboard, char character)
    {
        foreach (var handler in keyboardHandlers)
            handler.OnKeyChar(keyboard, character);
    }

    private void onKeyPressed(IKeyboard keyboard, Key key, int scancode)
    {
        // This is expected to be recursive
        foreach (var handler in keyboardHandlers)
        {
            var result = handler.OnKeyPressed(keyboard, key, scancode);

            if (result is not null)
                keypressHandlers.Add(new KeyPressHandler(keyboard, key, handler), result);
        }
    }

    private void onKeyReleased(IKeyboard keyboard, Key key, int scancode)
    {
        foreach (var handler in keyboardHandlers)
        {
            var tmp = new KeyPressHandler(keyboard, key, handler);
            if (!keypressHandlers.TryGetValue(tmp, out var pressHandler))
                continue;

            pressHandler.OnKeyReleased(keyboard, key, scancode);
            keypressHandlers.Remove(tmp);
        }
    }
}
