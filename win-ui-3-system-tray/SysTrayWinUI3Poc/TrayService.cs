using Hardcodet.Wpf.TaskbarNotification.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysTrayWinUI3Poc.NativeWindowing;
using SysTrayWinUI3Poc.Services;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;

namespace SysTrayWinUI3Poc
{
    public class TrayService : ITrayService
    {
        WindowsTrayIcon tray;

        public Action ClickHandler { get; set; }
        public bool IsDisposed { get; private set; }

        public void Initialize()
        {
            tray = new WindowsTrayIcon("trayicon.ico");
            tray.RightClick = () =>
            {
                //var win = (MainWindow)App.m_window;
                //var menu = win.GetAppContextMenu();
                //menu.ShowMode = FlyoutShowMode.Standard;

                //App.m_window.Close();
                //App.m_appWindow.Hide();
                //var contextMenuWindow = new ContextMenuWindow();
                //var contetxWindow = App.GetAppWindowForCurrentWindow(contextMenuWindow);
                //if (contetxWindow != null)
                //{
                //    contetxWindow.Title = "WinUI ❤️ AppWindow Context";
                //    contetxWindow.Show();
                //}
                //Hardcodet.Wpf.TaskbarNotification.Interop.Point position = TrayInfo.GetTrayLocation();
                //var x = position.X - App.m_appWindow.Size.Width;
                //var y = position.Y - 300;
                //WindowExtensions.RepositionSpecifc(contetxWindow, (int)x, (int)y, 300, 300);
            };

            tray.LeftClick = () =>
            {
                WindowExtensions.BringToFront();
                ClickHandler?.Invoke();
            };
        }
    }
}
