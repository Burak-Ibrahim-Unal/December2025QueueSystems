using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEducation.Examples.Observer
{
    internal class UserSubject
    {
        private List<IUserObserver> _observers = new List<IUserObserver>();

        internal void RegisteredObserver(IUserObserver userObserver) 
        { 
            _observers.Add(userObserver); 
        }

        internal void Notify(User user)
        {
            foreach (IUserObserver observer in _observers)
            {
                observer.ProcessOtherOperations();
            }
        }
    }
}
