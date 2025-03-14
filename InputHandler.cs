using System;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace C_Sharp_GL;

internal static class InputHandler
{
    public static void InitializeInputs()
    {
        var window = WindowManager.GetWindow();
        var input = window.CreateInput(); 

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            Console.WriteLine("Closing window...");
            WindowManager.GetWindow().Close();
        }

        if (key == Key.Space)
        {
            Console.WriteLine("Stoppping simulation...");
            Renderer.Pause();
        }

        
    }
}