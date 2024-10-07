using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

namespace Utilities
{
    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> range)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));
            
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(range)));
        }
    }
}
