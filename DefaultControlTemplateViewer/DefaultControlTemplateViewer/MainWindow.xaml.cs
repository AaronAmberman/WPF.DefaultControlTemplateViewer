using System.Windows;

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
                // this will allow us to dynamically assign its resource (can't bind resource dictionaries)
                ContentControlRenderer = ContentRenderer
            };

            DataContext = vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ReadTypesFromPresentationFrameworkAssembly();
        }
    }
}
