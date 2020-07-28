using System;
using System.Windows.Input;

namespace SierraHOTAS.ViewModels.Commands
{
    //TODO: do this later if async needed in Execute method: https://johnthiriet.com/mvvm-going-async-with-async-command/#
    public class CommandHandlerWithParameter<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public CommandHandlerWithParameter(Action<T> execute)
            : this(execute, null)
        {
        }

        public CommandHandlerWithParameter(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
