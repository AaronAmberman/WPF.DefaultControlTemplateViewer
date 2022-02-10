using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DefaultControlTemplateViewer
{
    /// <summary>Abstract base view model type.</summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Call when a property value is changed.</summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>Abstract base view model type.</summary>
    /// <typeparam name="T">The type of model.</typeparam>
    /// <seealso cref="ViewModelBase" />
    public abstract class ViewModelBase<T> : ViewModelBase
    {
        /// <summary>Gets the model.</summary>
        public T Model { get; protected set; }
    }
}
