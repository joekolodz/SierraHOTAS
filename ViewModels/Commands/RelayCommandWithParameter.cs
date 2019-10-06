using System;
using System.Windows.Input;

namespace SierraHOTAS.ViewModel.Commands
{
    //TODO: do this later...https://johnthiriet.com/mvvm-going-async-with-async-command/#
    public class RelayCommandWithParameter : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommandWithParameter(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommandWithParameter(Action<object> execute, Func<object, bool> canExecute = null)
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
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
