using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace Mekmak.Gman.Jade
{
    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        
        public Command(Action execute) : this(_ => execute()) { }
        public Command(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        public void Execute(object parameter) { _execute(parameter); }
    }
}
