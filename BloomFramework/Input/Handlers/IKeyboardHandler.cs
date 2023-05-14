using Silk.NET.Input;

namespace BloomFramework.Input.Handlers;

public interface IKeyboardHandler
{
    void OnKeyChar(IKeyboard keyboard, char character) { }
    IKeyboardHandler? OnKeyPressed(IKeyboard keyboard, Key key, int scancode) { return null; }
    void OnKeyReleased(IKeyboard keyboard, Key key, int scancode) { }
}
