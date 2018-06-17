using System.Collections.ObjectModel;

namespace ySlidy
{
    public class PersonCollectionViewModel : ViewModelBase
    {
        public PersonCollectionViewModel()
        {
            PersonCollection = new ObservableCollection<Person>();
        }

        private Person _current;

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        /// <value>The current.</value>
        public Person Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                Notify("Current");
            }
        }

        /// <summary>
        /// Gets or sets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        public ObservableCollection<Person> PersonCollection
        {
            get;
            set;
        }
    }
}