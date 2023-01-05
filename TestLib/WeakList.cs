using System.Collections;

namespace BloomBirb.Framework
{
    /// <summary>
    /// A list that holds weak references of objects, and performs cleanups whenever possible.
    /// </summary>
    public class WeakList<T> : IEnumerable<T> where T : class
    {
        private readonly struct InvalidatableWeakReference
        {
            public readonly WeakReference<T> Reference;
            public readonly long ObjectHashCode;

            public InvalidatableWeakReference(T item)
            {
                ObjectHashCode = EqualityComparer<T>.Default.GetHashCode();
                Reference = new WeakReference<T>(item);
            }

            public InvalidatableWeakReference(WeakReference<T> reference)
            {
                Reference = reference;
                ObjectHashCode = !reference.TryGetTarget(out var item) ? 0 : EqualityComparer<T>.Default.GetHashCode(item);
            }
        }

        private struct WeakListEnumerator : IEnumerator<T>
        {
            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            private int index = -1;

            private WeakList<T> weakList;

            private int version = 0;

            public WeakListEnumerator(WeakList<T> weakList)
            {
                this.weakList = weakList;
                Current = default!;
                version = weakList.enumeratorVersion;
            }

            public bool MoveNext()
            {
                if (weakList.enumeratorVersion != version)
                    throw new InvalidOperationException($"May not add or remove items from this {nameof(WeakList<T>)} during enumeration.");

                while (index + 1 < weakList.Count)
                {
                    ++index;

                    if (!weakList.items[index].Reference.TryGetTarget(out var target))
                        continue;

                    Current = target;
                    return true;
                }

                return false;

            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
                Current = default!;
                index = -1;
            }
        }

        private readonly List<InvalidatableWeakReference> items = new();

        public int Count => items.Count;

        private int enumeratorVersion = 0;

        public void Add(T item) => add(new(item));

        public void Add(WeakReference<T> item) => add(new(item));

        private void add(InvalidatableWeakReference reference)
        {
            ++enumeratorVersion;
            items.Add(reference);

            trim();
        }

        public bool Remove(T item)
        {
            int itemHash = EqualityComparer<T>.Default.GetHashCode(item);
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].ObjectHashCode != itemHash)
                    continue;

                if (!items[i].Reference.TryGetTarget(out var refItem) || !EqualityComparer<T>.Default.Equals(item, refItem))
                    continue;

                items.RemoveAt(i);
                trim();
                ++enumeratorVersion;
                return true;
            }

            return false;
        }


        public bool Remove(WeakReference<T> reference)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].Reference != reference)
                    continue;

                items.RemoveAt(i);
                trim();
                ++enumeratorVersion;
                return true;
            }

            return false;
        }

        public void Clear() => items.Clear();

        public bool Contains(T item)
        {
            int itemHashCode = EqualityComparer<T>.Default.GetHashCode(item);

            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].ObjectHashCode != itemHashCode)
                    continue;

                if (!items[i].Reference.TryGetTarget(out var target) || !EqualityComparer<T>.Default.Equals(item, target))
                    continue;

                return true;
            }

            return false;
        }

        public bool Contains(WeakReference<T> reference)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].Reference != reference)
                    continue;

                return true;
            }

            return false;
        }

        private void trim() => items.RemoveAll(r => r.Reference == null || !r.Reference.TryGetTarget(out _));

        public IEnumerator<T> GetEnumerator() => new WeakListEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
