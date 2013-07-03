using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;

namespace Baza_pizzerii {
    class UserReview : INotifyPropertyChanged {
        PizzeriaPage outerClass;
        public UserReview(PizzeriaPage outerclass) {
            upVoteCommand = new DelegateCommand(OnUpVoteCommand);
            downVoteCommand = new DelegateCommand(OnDownVoteCommand);
            this.outerClass = outerclass;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public string id_feedback {
            get;
            set;
        }
        public string feedback {
            get;
            set;
        }
        private bool assigned = false;
        private int _grade_value;
        public int grade_value {
            get {
                return _grade_value;
            }
            set {
                int val = value;
                _grade_value += val;
                if (assigned) this.outerClass.UpdateFeedback(id_feedback, val);
                assigned = true;
                OnPropertyChanged("grade_value");
            }
        }
        public int grade {
            get;
            set;
        }

        private ICommand _upVoteCommand;
        private ICommand _downVoteCommand;
        private int increase = 0;
        public ICommand upVoteCommand {
            get {
                return _upVoteCommand;
            }
            set {
                _upVoteCommand = value;
            }
        }
        public ICommand downVoteCommand {
            get {
                return _downVoteCommand;
            }
            set {
                _downVoteCommand = value;
            }
        }
        void OnUpVoteCommand(object aParameter) {
            if (increase < 1) {
                increase += 1;
                grade_value = 1;
            }
        }
        void OnDownVoteCommand(object aParameter) {
            if (increase > -1) {
                increase -= 1;
                grade_value = -1;
            }
        }

    }
}
