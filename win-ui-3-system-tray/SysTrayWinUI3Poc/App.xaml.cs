using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SysTrayWinUI3Poc.Common;
using SysTrayWinUI3Poc.Helper;
using SysTrayWinUI3Poc.Helpers;
using SysTrayWinUI3Poc.NativeWindowing;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.System;
using Windows.UI.Core.Preview;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SysTrayWinUI3Poc
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        #region Assembly references
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public delegate IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
        #endregion

        #region Data Members
        public const int HOTKEY_ID = 9000;
        private IntPtr hWnd;
        #endregion

        #region Properties
        public static App Instance { get; private set; }
        internal Window Window { get; set; }
        internal AppWindow AppWindow { get; set; }
        internal RootFrameNavigationHelper Navigation { get; set; }

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            InitializeComponent();

            Instance = this;
        }
        #endregion

        #region Event Handlers
        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            sender.Hide();
            args.Cancel = true;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            //UpdateNavigationBasedOnSelectedPage(GetRootFrame());
            deferral.Complete();
        }
        #endregion

        #region Overridden Methods
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();
            // Get the AppWindow for our XAML Window
            AppWindow = GetAppWindowForCurrentWindow(Window);
            if (AppWindow != null)
            {
                var _presenter = AppWindow.Presenter as OverlappedPresenter;
                _presenter.IsAlwaysOnTop = true;
                //_presenter.IsMaximizable = true;
                //_presenter.IsMinimizable = true;
                // You now have an AppWindow object and can call its methods to manipulate the window.
                // Just to do something here, let's change the title of the window...
                //m_window.Title = "WinUI ❤️ AppWindow";
                //m_appWindow.Hide();
                AppWindow.Closing += AppWindow_Closing;
                WindowExtensions.Reposition(AppWindow, 1, 2);
            }

            await EnsureWindow(args.UWPLaunchActivatedEventArgs);
#if RELEASE
            this.Window?.Hide();
#endif
            SetupTrayIcon();

            //Set Global hotKeys
            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this.Window);
            SetGlobalHotkeys();

            //To Set Titlebar icon in default Window TitleBar
            //WindowExtensions.ChangeIcon(m_appWindow,"trayicon.ico");
        }
        #endregion

        #region Private Methods

        public async Task EnsureWindow(IActivatedEventArgs args)
        {
            Frame rootFrame = GetRootFrame();

            ThemeHelper.Initialize();

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated
                    || args.PreviousExecutionState == ApplicationExecutionState.Suspended)
            {
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }

                this.Window.Activate();

                UpdateNavigationBasedOnSelectedPage(rootFrame);
                return;
            }

            Type targetPageType = typeof(LoginPage);
            string targetPageArguments = string.Empty;

            if (args.Kind == ActivationKind.Launch)
            {
                targetPageArguments = ((Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)args).Arguments;
            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                //Match match;

                string targetId = string.Empty;

                targetPageArguments = targetId;

                //bool IsMatching(string parent, string expression)
                //{
                //    match = Regex.Match(parent, expression);
                //    return match.Success;
                //}
            }

            rootFrame.Navigate(targetPageType, targetPageArguments);

            //if (targetPageType == typeof(MainPage))
            //{
            //    ((Microsoft.UI.Xaml.Controls.NavigationViewItem)((NavigationRootPage)this.Window.Content).NavigationView.MenuItems[0]).IsSelected = true;
            //}

            // Ensure the current window is active
            this.Window.Activate();
        }

        internal Frame GetRootFrame()
        {
            Frame rootFrame;
            if (this.Window.Content is not NavigationRootPage rootPage)
            {
                rootPage = new NavigationRootPage();
                rootFrame = (Frame)rootPage.FindName("rootFrame");
                if (rootFrame == null)
                {
                    throw new Exception("Root frame not found");
                }
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                this.Window.Content = rootPage;
            }
            else
            {
                rootFrame = (Frame)rootPage.FindName("rootFrame");
            }

            //Set Navigation Global Handler
            Navigation = new RootFrameNavigationHelper(rootFrame, rootPage.NavigationView);

            return rootFrame;
        }

        internal void UpdateNavigationBasedOnSelectedPage(Frame rootFrame)
        {
            //// Check if we brought back an ItemPage
            //if (rootFrame.Content is ItemPage itemPage)
            //{
            //    // We did, so bring the selected item back into view
            //    string name = itemPage.Item.Title;
            //    if (this.Window.Content is NavigationRootPage nav)
            //    {
            //        // Finally brings back into view the correct item.
            //        // But first: Update page layout!
            //        nav.EnsureItemIsVisibleInNavigation(name);
            //    }
            //}
        }

        private void SetupTrayIcon()
        {
            var trayService = new TrayService();

            if (trayService != null)
            {
                trayService.Initialize();
                trayService.ClickHandler = () =>
                {
                    if (!AppWindow.IsVisible)
                        AppWindow?.Show();
                    else
                        AppWindow.Hide();
                };
            }
        }
        private void SetGlobalHotkeys()
        {
            //var keyboardHookManager = new KeyboardHookManager();
            //keyboardHookManager.Start();

            //var keyT = 0x54;
            //keyboardHookManager.RegisterHotkey(NonInvasiveKeyboardHookLibrary.ModifierKeys.Control |
            //    NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt,
            //    keyT, () =>
            //{
            //    PInvoke.User32.ShowWindow(hWnd, PInvoke.User32.WindowShowStyle.SW_SHOW);
            //    PInvoke.User32.ShowWindow(hWnd, PInvoke.User32.WindowShowStyle.SW_RESTORE);
            //    PInvoke.User32.SetForegroundWindow(hWnd);
            //});
        }
        #endregion

        #region Internal Methods
        internal static AppWindow GetAppWindowForCurrentWindow(Window window)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
        #endregion
    }
}
