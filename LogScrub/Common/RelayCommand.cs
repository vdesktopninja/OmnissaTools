using System.Windows.Input;

namespace LogScrub.Gui.Common
{
    /// <summary>
    /// A command that relays its functionality to other objects by invoking delegates
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of RelayCommand
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of RelayCommand with no parameter
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Forces a reevaluation of the CanExecute method
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// A generic command that relays its functionality to other objects by invoking delegates
    /// </summary>
    /// <typeparam name="T">Type of the command parameter</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of RelayCommand
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        /// <summary>
        /// Forces a reevaluation of the CanExecute method
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}