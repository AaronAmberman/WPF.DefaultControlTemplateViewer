namespace DefaultControlTemplateViewer
{
    public class TemplateEntry : ViewModelBase
    {
        private string? content;
        private string? header;

        public string? Content
        {
            get => content;
            set
            {
                content = value;
                OnPropertyChanged();
            }
        }

        public string? Header 
        { 
            get => header; 
            set
            {
                header = value;
                OnPropertyChanged();
            }
        }
    }
}
