using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Xml;

namespace DefaultControlTemplateViewer
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CollectionView? cv;
        private Assembly? presentationFramework;
        private string? typeFilter;
        private TypePropertiesMap? selectedTypeMap;
        private object? selectedControlTemplate;
        private int selectedThemeIndex;

        public ContentControl? ContentControlRenderer { get; set; } = null;

        public object? SelectedTemplate 
        { 
            get => selectedControlTemplate; 
            set
            {
                selectedControlTemplate = value;
                OnPropertyChanged();
            }
        }

        public int SelectedThemeIndex
        {
            get => selectedThemeIndex;
            set
            {
                selectedThemeIndex = value;

                string uri = string.Empty;

                switch (value)
                {
                    case 0:
                        uri = "/PresentationFramework.Aero;v4.0.0.0;Component/themes/aero.normalcolor.xaml";
                        break;
                    case 1:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.normalcolor.xaml";
                        break;
                    case 2:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.homestead.xaml";
                        break;
                    case 3:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.metallic.xaml";
                        break;
                    case 4:
                        uri = "/PresentationFramework.Classic;v4.0.0.0;Component/themes/classic.xaml";
                        break;
                    case 5:
                        uri = "/PresentationFramework.Royale;v4.0.0.0;Component/themes/royale.normalcolor.xaml";
                        break;
                }

                try
                {
                    Uri themeUri = new Uri(uri, UriKind.Relative);
                    ResourceDictionary themeResources = (ResourceDictionary)Application.LoadComponent(themeUri);

                    // assign the read in resource dictionary to our grid wrapper in the ControlTemplate tab
                    // this will allow the template text to be properly retrieved
                    if (ContentControlRenderer != null)
                        ContentControlRenderer.Resources = themeResources;

                    if (SelectedTypeMap != null)
                    {
                        GetAndDisplayControlTemplate();
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to load data for selected theme.", "Theme Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                OnPropertyChanged();
            }
        }

        public TypePropertiesMap? SelectedTypeMap
        {
            get => selectedTypeMap;
            set
            {
                selectedTypeMap = value;

                GetAndDisplayControlTemplate();

                OnPropertyChanged();
            }
        }

        public TabControl? TabControl { get; set; }

        public string? TypeFilter
        {
            get => typeFilter;
            set
            {
                typeFilter = value;

                if (cv != null) cv.Refresh();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<TypePropertiesMap> TypePropertiesMapping { get; set; } = new ObservableCollection<TypePropertiesMap>();

        public MainWindowViewModel()
        {
            SelectedThemeIndex = 0; // trigger initial theme selection
        }

        private void GetAndDisplayControlTemplate()
        {
            try
            {
                if (presentationFramework == null) return;
                if (SelectedTypeMap == null) return;
                if (SelectedTypeMap.Type == null) return;
                if (string.IsNullOrEmpty(SelectedTypeMap.Type.FullName)) return;

                // remove all tabs except our Sample tab
                if (TabControl != null)
                {
                    TabControl.Items.Clear();
                }

                object? instance = presentationFramework.CreateInstance(SelectedTypeMap.Type.FullName);
                Type? instanceType = instance?.GetType();
                Window? window = null;
                NavigationWindow? navigationWindow = null;

                if (instance == null) return;
                if (instanceType == null) return;

                if (instanceType == typeof(ToolTip) || instanceType == typeof(Window))
                {
                    // cannot be set as a child so cannot be bound

                    window = (Window)instance;
                    window.WindowState = WindowState.Minimized;
                    window.ShowInTaskbar = false;
                    window.Show();
                    window.Hide();
                }
                else if (instanceType == typeof(NavigationWindow))
                {
                    // cannot be set as a child so cannot be bound

                    navigationWindow = (NavigationWindow)instance;
                    navigationWindow.WindowState = WindowState.Minimized;
                    navigationWindow.ShowInTaskbar = false;
                    navigationWindow.Show();
                    navigationWindow.Hide();
                }
                else if (typeof(ContextMenu).IsAssignableFrom(instanceType))
                {
                    // need to assign the context menu to the control that SelectedTemplate is bound to
                    Frame frame = new Frame
                    {
                        ContextMenu = (ContextMenu)instance
                    };

                    SelectedTemplate = frame;
                }
                else if (typeof(Page).IsAssignableFrom(instanceType))
                {
                    Frame frame = new Frame()
                    {
                        Content = instance
                    };

                    SelectedTemplate = frame;
                }
                else
                {
                    /*
                     * we have to have this bound or else we don't get the FrameworkTemplate because there is no
                     * instance of it at run time, however if it is bound to the UI to a ContentControl then the
                     * WPF engine will instantiate the FrameworkTemplate and then we can read it...then we can
                     * write it in plain text
                     */
                    SelectedTemplate = instance;
                }

                foreach (PropertyInfo pi in SelectedTypeMap.TypeProperties)
                {
                    string name = pi.Name;

                    // get the template that the property uses
                    FrameworkTemplate? ft = pi.GetValue(instance) as FrameworkTemplate;

                    string templateString = string.Empty;

                    if (ft != null)
                    {
                        string filename = Path.GetTempFileName();

                        File.Delete(filename); // delete the file it generated

                        filename = Path.ChangeExtension(filename, "xml");

                        // write out XAML
                        XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8);
                        writer.Formatting = Formatting.Indented;

                        XamlWriter.Save(ft, writer);

                        writer.Flush();
                        writer.Close();
                        writer.Dispose();

                        // read it back in
                        templateString = File.ReadAllText(filename);

                        File.Delete(filename);
                    }

                    // we will a a tab to the tab control for each property that we have to display data for
                    // but only if the string is not null, we don't care to show empty properties
                    if (!string.IsNullOrWhiteSpace(templateString))
                    {
                        TabItem tabItem = new TabItem
                        {
                            Header = name
                        };

                        TextBox tb = new TextBox
                        {
                            Text = templateString,
                            IsReadOnly = true
                        };

                        ScrollViewer sv = new ScrollViewer();
                        sv.Content = tb;
                        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        tabItem.Content = sv;
                        tabItem.Padding = new Thickness(5);

                        if (TabControl != null)
                        {
                            TabControl.Items.Insert(0, tabItem);
                            TabControl.SelectedIndex = 0;
                        }
                    }
                }

                if (window != null) window.Close();
                if (navigationWindow != null) navigationWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred attempting to get the template information. {ex.Message}", "Control Template Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReadTypesFromPresentationFrameworkAssembly()
        {
            try
            {
                Type frameworkTemplateType = typeof(FrameworkTemplate);

                presentationFramework = Assembly.Load("PresentationFramework");

                List<Type> types = presentationFramework.GetTypes().ToList();

                foreach (Type type in types)
                {
                    if (type.IsAbstract) { continue; }
                    if (type.ContainsGenericParameters) { continue; }
                    if (type.GetConstructor(new Type[] { }) == null) { continue; }

                    List<PropertyInfo> templatedProperties = new List<PropertyInfo>();
                    List<PropertyInfo> properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();

                    foreach (PropertyInfo prop in properties)
                    {
                        if (frameworkTemplateType.IsAssignableFrom(prop.PropertyType))
                        {
                            templatedProperties.Add(prop);
                        }
                    }

                    if (templatedProperties.Count == 0) { continue; }

                    TypePropertiesMapping.Add(new TypePropertiesMap
                    {
                        Type = type,
                        TypeProperties = templatedProperties
                    });

                    cv = (CollectionView)CollectionViewSource.GetDefaultView(TypePropertiesMapping);
                    cv.Filter = TypeFiltering;
                }
            }
            catch
            {
                MessageBox.Show("Unable to load the assembly information for PresentationFramework.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TypeFiltering(object obj)
        {
            TypePropertiesMap? map = obj as TypePropertiesMap;

            if (map == null) { return false; }

            if (!string.IsNullOrWhiteSpace(TypeFilter))
            {
                if (map.Type != null && map.Type.Name.Contains(TypeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (map.Type != null && map.Type.Namespace != null && map.Type.Namespace.Contains(TypeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (map.Type != null && map.Type.BaseType != null && map.Type.BaseType.Name.Contains(TypeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
