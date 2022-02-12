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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;

namespace DefaultControlTemplateViewer
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CollectionView? cv;
        private ICommand? genAeroCommand;
        private ICommand? genLunaCommand;
        private ICommand? genLunaHomesteadCommand;
        private ICommand? genLunaMetallicCommand;
        private ICommand? genClassicCommand;
        private ICommand? genRoyaleCommand;
        private Assembly? presentationFramework;
        private string? resourceDictionaryString;
        private TypePropertiesMap? selectedTypeMap;
        private object? selectedTemplateInstance;
        private int selectedThemeIndex;
        private TemplateEntry? selectedTemplateEntry;
        private string? typeFilter;

        public ContentControl? ContentControlRenderer { get; set; } = null;

        public Dispatcher? Dispatcher { get; set; } = null;

        public ICommand GenAeroCommand => genAeroCommand ??= new RelayCommand(GenerateAero);
        public ICommand GenLunaCommand => genLunaCommand ??= new RelayCommand(GenerateLuna);
        public ICommand GenLunaHomesteadCommand => genLunaHomesteadCommand ??= new RelayCommand(GenerateLunaHomestead);
        public ICommand GenLunaMetallicCommand => genLunaMetallicCommand ??= new RelayCommand(GenerateMetallic);
        public ICommand GenClassicCommand => genClassicCommand ??= new RelayCommand(GenerateClassic);
        public ICommand GenRoyaleCommand => genRoyaleCommand ??= new RelayCommand(GenerateRoyale);

        public string? ResourceDictionaryString 
        { 
            get => resourceDictionaryString; 
            set
            {
                resourceDictionaryString = value;
                OnPropertyChanged();
            }
        }

        public TemplateEntry? SelectedTemplateEntry
        {
            get => selectedTemplateEntry;
            set
            {
                selectedTemplateEntry = value;
                OnPropertyChanged();
            }
        }

        public object? SelectedTemplateInstance
        {
            get => selectedTemplateInstance;
            set
            {
                selectedTemplateInstance = value;
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
                        uri = "";
                        break;
                    case 1:
                        uri = "/PresentationFramework.Aero;v4.0.0.0;Component/themes/aero.normalcolor.xaml";
                        break;
                    case 2:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.normalcolor.xaml";
                        break;
                    case 3:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.homestead.xaml";
                        break;
                    case 4:
                        uri = "/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.metallic.xaml";
                        break;
                    case 5:
                        uri = "/PresentationFramework.Classic;v4.0.0.0;Component/themes/classic.xaml";
                        break;
                    case 6:
                        uri = "/PresentationFramework.Royale;v4.0.0.0;Component/themes/royale.normalcolor.xaml";
                        break;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(uri))
                    {
                        Uri themeUri = new Uri(uri, UriKind.Relative);
                        ResourceDictionary themeResources = (ResourceDictionary)Application.LoadComponent(themeUri);

                        // assign the read in resource dictionary to our ContentControl
                        // this will allow the template text to be properly retrieved
                        if (ContentControlRenderer != null)
                            ContentControlRenderer.Resources = themeResources;
                    }
                    else
                    {
                        // gets the default template without any theming applied
                        if (ContentControlRenderer != null)
                            ContentControlRenderer.Resources.Clear();
                    }

                    if (SelectedTypeMap != null)
                    {
                        GetAndDisplayControlTemplate();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load data for selected theme. {ex.Message}", "Theme Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (value == null)
                {
                    // if the user selected nothing than clear the right side
                    Templates?.Clear();
                }
                else
                {
                    GetAndDisplayControlTemplate();
                }

                OnPropertyChanged();
            }
        }

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

        public ObservableCollection<TemplateEntry>? Templates { get; set; } = new ObservableCollection<TemplateEntry>();

        public ObservableCollection<TypePropertiesMap> TypePropertiesMapping { get; set; } = new ObservableCollection<TypePropertiesMap>();

        public MainWindowViewModel()
        {
            SelectedThemeIndex = 0; // trigger initial theme selection
        }

        private void GenTheme(string resourceDictionary)
        {
            /*
             * This cannot be multi-threaded because XamlWriter.Save requires STA threaded apartments. So
             * if a thread is started to run the method than it throws an exception. Just have to deal with
             * the lag that is produced by this operation.
             */

            try
            {
                string filename = Path.GetTempFileName();

                File.Delete(filename); // delete the file it generated

                filename = Path.ChangeExtension(filename, "xaml");

                // get the resource dictionary
                Uri uri = new Uri(resourceDictionary, UriKind.Relative);

                ResourceDictionary themeResources = (ResourceDictionary)Application.LoadComponent(uri);

                XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;

                XamlWriter.Save(themeResources, writer);

                writer.Close();
                writer.Dispose();

                // read it back in
                string resourceDictionaryString = File.ReadAllText(filename);

                File.Delete(filename);

                ResourceDictionaryString = resourceDictionaryString;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred attempting to generate Aero ResourceDictionary. {ex.Message}");
            }
        }

        private void GenerateAero()
        {
            GenTheme("/PresentationFramework.Aero;v4.0.0.0;Component/themes/aero.normalcolor.xaml");

            MessageBox.Show("Aero generated.", "Complete");
        }

        private void GenerateLuna()
        {
            GenTheme("/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.normalcolor.xaml");

            MessageBox.Show("Luna generated.", "Complete");
        }

        private void GenerateLunaHomestead()
        {
            GenTheme("/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.homestead.xaml");

            MessageBox.Show("Luna Homestead generated.", "Complete");
        }

        private void GenerateMetallic()
        {
            GenTheme("/PresentationFramework.Luna;v4.0.0.0;Component/themes/luna.metallic.xaml");

            MessageBox.Show("Luna Metallic generated.", "Complete");
        }

        private void GenerateClassic()
        {
            GenTheme("/PresentationFramework.Classic;v4.0.0.0;Component/themes/classic.xaml");

            MessageBox.Show("Classic generated.", "Complete");
        }

        private void GenerateRoyale()
        {
            GenTheme("/PresentationFramework.Royale;v4.0.0.0;Component/themes/royale.normalcolor.xaml");

            MessageBox.Show("Royale generated.", "Complete");
        }

        private void GetAndDisplayControlTemplate()
        {
            try
            {
                // verify data
                if (presentationFramework == null) return;
                if (SelectedTypeMap == null) return;
                if (SelectedTypeMap.Type == null) return;
                if (string.IsNullOrEmpty(SelectedTypeMap.Type.FullName)) return;

                Templates?.Clear();

                // get instance of FrameworkTemplate object and instantiate it and render it (via binding)
                object? instance = presentationFramework.CreateInstance(SelectedTypeMap.Type.FullName);
                Type? instanceType = instance?.GetType();

                if (instance == null) return;
                if (instanceType == null) return;

                Window? window = null;
                NavigationWindow? navigationWindow = null;

                // do something to ensure data is retrievable (some what type specific)
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

                    SelectedTemplateInstance = frame;
                }
                else if (typeof(Page).IsAssignableFrom(instanceType))
                {
                    Frame frame = new Frame()
                    {
                        Content = instance
                    };

                    SelectedTemplateInstance = frame;
                }
                else
                {
                    /*
                     * we have to have this bound or else we don't get the FrameworkTemplate because there is no
                     * instance of it at run time, however if it is bound to the UI to a ContentControl then the
                     * WPF engine will instantiate the FrameworkTemplate and then we can read it...then we can
                     * write it in plain text
                     */
                    SelectedTemplateInstance = instance;
                }

                // loop through each FrameworkTemplate property type we have asking for the contents
                foreach (PropertyInfo pi in SelectedTypeMap.TypeProperties)
                {
                    string name = pi.Name;

                    // get the template that the property uses
                    FrameworkTemplate? ft = pi.GetValue(instance) as FrameworkTemplate;

                    string templateString = string.Empty;

                    // write out XAML if we were able to cast the object appropriately
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

                    // output the data if we got some, we don't want to show empty entries
                    if (!string.IsNullOrWhiteSpace(templateString))
                    {
                        Templates?.Add(new TemplateEntry { Content = templateString, Header = name });
                    }
                }

                // set our selected entry to the view shows the template text
                if (Templates?.Count > 0)
                {
                    SelectedTemplateEntry = Templates[0];
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
                List<TypePropertiesMap> results = new List<TypePropertiesMap>();

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

                    results.Add(new TypePropertiesMap
                    {
                        Type = type,
                        TypeProperties = templatedProperties
                    });

                    cv = (CollectionView)CollectionViewSource.GetDefaultView(TypePropertiesMapping);
                    cv.Filter = TypeFiltering;
                }

                // after we have our results lets sort them by namespace then name
                results = results.OrderBy(tpe => tpe.Type?.Namespace).ThenBy(tpe => tpe.Type?.Name).ToList();

                foreach (TypePropertiesMap result in results)
                {
                    TypePropertiesMapping.Add(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load the assembly information for PresentationFramework. {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
