using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogScrub.Gui.Common
{
    /// <summary>
    /// Base class for view models implementing INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the property value and raises PropertyChanged if the value changed
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>True if the value was changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets the property value and raises PropertyChanged if the value changed, with additional action
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value</param>
        /// <param name="onChanged">Action to execute when value changes</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>True if the value was changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                onChanged?.Invoke();
                return true;
            }
            return false;
        }
    }
}