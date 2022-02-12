using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace DefaultControlTemplateViewer
{
    /// <summary>A command that allows for the use of methods.</summary>
    /// <seealso cref="ICommand" />
    [ExcludeFromCodeCoverage]
    public class RelayCommand : ICommand
    {
        #region Fields 

        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        #endregion

        #region Event Declarations

        /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="RelayCommand"/> class.</summary>
        /// <param name="execute">The method to execute.</param>
        /// <exception cref="ArgumentNullException">execute</exception>
        public RelayCommand(Action execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            this.execute = execute;
        }

        /// <summary>Initializes a new instance of the <see cref="RelayCommand"/> class.</summary>
        /// <param name="execute">The method to execute.</param>
        /// <param name="canExecute">The method to call to determine whether or not the command can execute.</param>
        /// <exception cref="ArgumentNullException">execute</exception>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Methods

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>True if this command can be executed, otherwise false.</returns>
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            // parameter isn't used for this relay command
            return canExecute?.Invoke() ?? true;
        }

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        [DebuggerStepThrough]
        public void Execute(object? parameter)
        {
            // parameter isn't used for this relay command

            execute();
        }

        #endregion
    }

    /// <summary>A command that allows for the use of methods.</summary>
    /// <typeparam name="T">The type of argument the command will receive.</typeparam>
    /// <seealso cref="ICommand" />
    [ExcludeFromCodeCoverage]
    public class RelayCommand<T> : ICommand
    {
        #region Fields 

        private readonly Action<T> execute;
        private readonly Predicate<T>? canExecute;

        #endregion

        #region Event Declarations

        /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion

        #region Constructors 

        /// <summary>Initializes a new instance of the <see cref="RelayCommand"/> class.</summary>
        /// <param name="execute">The method to execute.</param>
        public RelayCommand(Action<T> execute) : this(execute, null) { }

        /// <summary>Initializes a new instance of the <see cref="RelayCommand"/> class.</summary>
        /// <param name="execute">The method to execute.</param>
        /// <param name="canExecute">The method to call to determine whether or not the command can execute.</param>
        /// <exception cref="ArgumentNullException">execute</exception>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Methods

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>True if this command can be executed, otherwise false.</returns>
        /// <exception cref="ArgumentException">The provided argument could not be cast to the type of T.</exception>
        public bool CanExecute(object? parameter)
        {
            try
            {
                T? paramAsT = (T?)parameter;

                if (paramAsT == null) return true;

                return canExecute?.Invoke(paramAsT) ?? true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(FormattableString.Invariant($"The parameter provided to RelayCommand<{typeof(T).Name}>.CanExecute was not of the appropriate type."), nameof(parameter), ex);
            }
        }

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <exception cref="ArgumentException">The provided argument could not be cast to the type of T.</exception>
        public void Execute(object? parameter)
        {
            try
            {
                T? paramAsT = (T?)parameter;

                execute(paramAsT);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(FormattableString.Invariant($"The parameter provided to RelayCommand<{typeof(T).Name}>.Execute was not of the appropriate type."), nameof(parameter), ex);
            }
        }

        #endregion
    }
}