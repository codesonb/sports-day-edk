using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace EDKv5
{
    public class ObservableCollectionAdapter<T> : ObservableCollection<T>
    {
        public ObservableCollectionAdapter(IList<T> origin) : base(origin)
        {
            this.origin = origin;
        }

        IList<T> origin;

        public new void Add(T item)
        {
            origin.Add(item);
            base.Add(item);
        }

        public new bool Remove(T item)
        {
            origin.Remove(item);
            return base.Remove(item);
        }

        public new void RemoveAt(int index)
        {
            origin.RemoveAt(index);
            base.RemoveAt(index);
        }

        public new void Insert(int index, T item)
        {
            origin.Insert(index, item);
            base.Insert(index, item);
        }

        public new void InsertItem(int index, T item)
        {
            origin.Insert(index, item);
            base.InsertItem(index, item);
        }

    }
}
