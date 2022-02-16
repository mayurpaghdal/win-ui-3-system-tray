using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SysTrayWinUI3Poc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;

            //Short-cut key Ctrl + S
            //KeyboardAccelerator keyboardAccelerator = new KeyboardAccelerator();
            //keyboardAccelerator.IsEnabled = true;
            //keyboardAccelerator.Key = Windows.System.VirtualKey.S;
            //keyboardAccelerator.Modifiers = Windows.System.VirtualKeyModifiers.Control;
            //myButton.KeyboardAccelerators.Add(keyboardAccelerator);
        }



        //private void myButton_Click(object sender, RoutedEventArgs e)
        //{
        //    myButton.Content = "Clicked";
        //    NavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
        //    //Set AppIcon and tool tip
        //}
    }
}
