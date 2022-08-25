using System;
using System.Runtime.InteropServices;

namespace KeqingNiuza.Service;

internal static class Native
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();


    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int index);


    public static bool IsWindowBeyondBounds(double width, double heigh)
    {
        var p = GetDesktopWindow();
        var hdc = GetDC(IntPtr.Zero);
        // 屏幕横向每英寸像素数
        var dpiX = GetDeviceCaps(hdc, 88);
        // 屏幕纵向每英寸像素数
        var dpiY = GetDeviceCaps(hdc, 90);
        _ = GetWindowRect(p, out var lpRect);
        var windowX = lpRect.Width * 96 / dpiX;
        var windowY = lpRect.Height * 96 / dpiY;
        if (width > windowX - 60 || heigh > windowY - 60)
            return true;
        return false;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public int X
    {
        get => Left;
        set
        {
            Right -= Left - value;
            Left = value;
        }
    }

    public int Y
    {
        get => Top;
        set
        {
            Bottom -= Top - value;
            Top = value;
        }
    }

    public int Height
    {
        get => Bottom - Top;
        set => Bottom = value + Top;
    }

    public int Width
    {
        get => Right - Left;
        set => Right = value + Left;
    }
}