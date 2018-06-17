using System.Collections.ObjectModel;

namespace ySlidy
{
    public class Person : ViewModelBase
    {
        private string _name;

        private bool _edit;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                Edit = false;
                Notify("Name");
            }
        }

        public bool Edit
        {
            get
            {
                return _edit;
            }
            set
            {
                _edit = value;
                Notify("Edit");
            }
        }

        public ObservableCollection<Person> Children
        {
            get;
            set;
        }
    }
}
