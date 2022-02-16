using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SysTrayWinUI3Poc.Controls;
using SysTrayWinUI3Poc.Helper;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SysTrayWinUI3Poc
{
    public sealed partial class NavigationRootPage : Page
    {
        #region Data Members
        public static NavigationRootPage Current;
        #endregion

        #region Properties
        public Action NavigationViewLoaded { get; set; }
        public Microsoft.UI.Xaml.Controls.NavigationView NavigationView
        {
            get { return NavigationViewControl; }
        }
        public TitleBar PageHeader => UIHelper.GetDescendantsOfType<TitleBar>(NavigationViewControl).FirstOrDefault();
        #endregion

        #region Ctor
        public NavigationRootPage()
        {
            this.InitializeComponent();
            Current = this;


            App.Instance.Window.SetTitleBar(AppTitleBar);
            App.Instance.AppWindow.Changed += AppWindow_Changed;
        }
        #endregion

        #region Events
        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength * 2, currMargin.Top, 0, currMargin.Bottom);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, 0, currMargin.Bottom);
            }

            UpdateAppTitleMargin(sender);
            UpdateHeaderMargin(sender);
        }

        private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange)
            {
                UpdateAppTitle(App.Instance.AppWindow.TitleBar);
            }
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // Clicked on an item that is already selected,
            // Avoid navigating to the same page again causing movement.
            if (args.InvokedItemContainer.IsSelected) return;

            if (args.IsSettingsInvoked)
            {
                if (rootFrame.CurrentSourcePageType != typeof(SettingsPage))
                {
                    rootFrame.Navigate(typeof(SettingsPage));
                }
            }
        }

        private async void rootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.SourcePageType == typeof(LoginPage))
            {
                NavigationViewControl.SelectedItem = null;
                await Task.Delay(50);

                try
                {
                    if (NavigationViewControl.IsPaneVisible)
                        NavigationViewControl.IsPaneVisible = false;
                }
                catch { }
            }
            else
            {
                NavigationViewControl.IsPaneVisible = true;
                UpdateAppTitle(App.Instance.AppWindow.TitleBar);
                UpdateHeaderMargin(NavigationViewControl);
            }
        }

        private void rootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //Clear the back stack if navigated from Login page.
            if (rootFrame.BackStackDepth > 0)
            {
                var loginPage = rootFrame.BackStack.FirstOrDefault(s => s.SourcePageType == typeof(LoginPage));

                if (loginPage is not null
                    || e.SourcePageType == typeof(LoginPage))
                {
                    rootFrame.BackStack.Clear();
                }
            }
        }
        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public string GetAppTitleFromSystem()
        {
            return Windows.ApplicationModel.Package.Current.DisplayName;
        }

        void UpdateAppTitle(AppWindowTitleBar coreTitleBar)
        {
            ////ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.RightInset, currMargin.Bottom);
        }

        private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
        {
            // Delay necessary to ensure NavigationView visual state can match navigation
            //Task.Delay(500).ContinueWith(_ => this.NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateAppTitleMargin(Microsoft.UI.Xaml.Controls.NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                AppTitle.TranslationTransition = new Vector3Transition();

                if ((sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent, 0, 0);
                }
                else
                {
                    AppTitle.Translation = new System.Numerics.Vector3(largeLeftIndent, 0, 0);
                }
            }
            else
            {
                Thickness currMargin = AppTitle.Margin;

                if ((sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
                else
                {
                    AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
            }
        }

        private void UpdateHeaderMargin(Microsoft.UI.Xaml.Controls.NavigationView sender)
        {
            if (PageHeader != null)
            {
                if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
                {
                    Current.PageHeader.HeaderPadding = (Thickness)App.Current.Resources["PageHeaderMinimalPadding"];
                }
                else
                {
                    Current.PageHeader.HeaderPadding = (Thickness)App.Current.Resources["PageHeaderDefaultPadding"];
                }
            }
        }
    }
}
