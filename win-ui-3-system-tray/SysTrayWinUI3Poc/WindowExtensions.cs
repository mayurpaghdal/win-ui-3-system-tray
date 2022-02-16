using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;

namespace SysTrayWinUI3Poc
{
    public static class WindowExtensions
    {
        public static IntPtr Hwnd { get; set; }

        public static void SetIcon(string iconFilename)
        {
            if (Hwnd == IntPtr.Zero)
                return;

            var hIcon = PInvoke.User32.LoadImage(IntPtr.Zero, iconFilename,
               PInvoke.User32.ImageType.IMAGE_ICON, 16, 16, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);
           
            PInvoke.User32.SendMessage(Hwnd, PInvoke.User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);
        }

        public static void BringToFront()
        {
            PInvoke.User32.ShowWindow(Hwnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
            PInvoke.User32.ShowWindow(Hwnd, PInvoke.User32.WindowShowStyle.SW_RESTORE);

            _ = PInvoke.User32.SetForegroundWindow(Hwnd);
        }

        public static void MinimizeToTray()
        {
            PInvoke.User32.ShowWindow(Hwnd, PInvoke.User32.WindowShowStyle.SW_MINIMIZE);
            PInvoke.User32.ShowWindow(Hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }

        public static void Reposition(this AppWindow appWindow, int row, int col, int totalRows = 2, int totalColumns = 3)
        {
            IntPtr hwndDesktop = PInvoke.User32.GetDesktopWindow();
            PInvoke.RECT rectParent;
            PInvoke.User32.GetClientRect(hwndDesktop, out rectParent);

            var width = (int)(rectParent.right - rectParent.left) / totalColumns;
            var height = (int)(rectParent.bottom - rectParent.top) / totalRows;

            var winPosition = new RectInt32(width * col, height * row, width, height);

            appWindow.MoveAndResize(winPosition);
        }

        public static void ChangeIcon(this AppWindow appWindow, string iconFileName)
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), iconFileName);
            appWindow.SetIcon(filePath);
        }
        public static void RepositionToSpecific(this AppWindow appWindow, int x, int y, int width, int height)
        {
            IntPtr hwndDesktop = PInvoke.User32.GetDesktopWindow();
            PInvoke.RECT rectParent;
            PInvoke.User32.GetClientRect(hwndDesktop, out rectParent);

            var winPosition = new RectInt32(x,y,width,height);

            appWindow.MoveAndResize(winPosition);
        }
    }

}
