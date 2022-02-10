using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DefaultControlTemplateViewer
{
    /*
     * Potential ideas for getting more than just a basic text view for text display...
     * https://github.com/icsharpcode/AvalonEdit
     * https://github.com/WPFDevelopersOrg/XamlViewer
     * https://github.com/thinkpixellab/kaxaml
     * https://docs.microsoft.com/en-us/dotnet/api/system.windows.markup.xamlreader?view=windowsdesktop-6.0
     * https://docs.microsoft.com/en-us/dotnet/api/system.windows.markup.xamlwriter?view=windowsdesktop-6.0
     */
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainWindowViewModel
            {
                TabControl = DisplayTabControl
            };

            DataContext = vm;

            // this will allow us to dynamically allow us to assign its resource to the programmatically loaded (can't bind resource dictionaries)
            vm.ContentControlRenderer = ContentRenderer;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ReadTypesFromPresentationFrameworkAssembly();
        }
    }
}
