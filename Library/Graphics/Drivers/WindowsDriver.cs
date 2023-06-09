﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.Graphics.Drivers;

public class WindowsDriver : Driver
{
    private IntPtr _outHandle;

    /// <summary>
    /// Returns a new instance of a driver that works via windows low-level function calls
    /// </summary>
    /// <exception cref="DriverException"></exception>
    public WindowsDriver()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr stdOutHandle = WindowsNative.GetStdHandle(WindowsNative.STD_OUTPUT_HANDLE);
            var enabled = WindowsNative.GetConsoleMode(stdOutHandle, out var outConsoleMode)
                && WindowsNative.SetConsoleMode(stdOutHandle, outConsoleMode | WindowsNative.ENABLE_PROCESSED_OUTPUT | WindowsNative.ENABLE_VIRTUAL_TERMINAL_PROCESSING);

            if (!enabled)
                throw new DriverException("Couldn't enable terminal graphics on this windows version");

            _outHandle = stdOutHandle;
        }
    }

    public override int ScreenWidth => throw new NotImplementedException();

    public override int ScreenHeight => throw new NotImplementedException();

    public override event WindowResizedDel? WindowResized;
    public override event ConsoleInputDel? KeyboardInputDown;
    public override event ConsoleInputDel? KeyboardInputUp;
    public override event MouseInputDel? MouseInputDown;
    public override event MouseInputDel? MouseInputUp;

    protected override void OnSetBackground(Color background)
    {
        throw new NotImplementedException();
    }

    protected override void OnSetForeground(Color foreground)
    {
        throw new NotImplementedException();
    }

    protected override void OnClear()
    {
        throw new NotImplementedException();
    }

    protected override void OnClear(Color background)
    {
        throw new NotImplementedException();
    }

    protected override void OnDisplay(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition)
    {
        throw new NotImplementedException();
    }

    protected override void OnDisplay(ReadOnlySpan<char> buffer, Point destinationPosition)
    {
        throw new NotImplementedException();
    }

    protected override void OnDispose()
    {
        throw new NotImplementedException();
    }
}
