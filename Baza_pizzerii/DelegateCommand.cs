﻿
using System;
using System.Windows.Input;
namespace Baza_pizzerii {
    //source http://wpftutorial.net/DelegateCommand.html
    public class DelegateCommand : ICommand {
        private readonly System.Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
            : this(execute, null) {
        }

        public DelegateCommand(Action<object> execute,
                       Predicate<object> canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public  bool CanExecute(object parameter) {
            if (_canExecute == null) {
                return true;
            }

            return _canExecute(parameter);
        }

        public  void Execute(object parameter) {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged() {
            if (CanExecuteChanged != null) {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

      
    }
}
