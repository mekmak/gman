using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mekmak.Gman.Jade
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel) DataContext;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void EmailImage_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.RotateImageCommand.Execute(null);
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // All this to disable the script error windows..
            var webBrowser = sender as WebBrowser;
            if (webBrowser == null)
                return;

	        var objComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(webBrowser);
	        if (objComWebBrowser == null)
	        {
		        return;
	        }

	        objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });
        }
    }
}
