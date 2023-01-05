namespace BloomBirb.Framework
{
    /// <summary>
    /// A Bindable is an wrapper to an object that is able to bind with another Bindable.
    /// When the value of a bindable changes, the change is propogated to any bindables bound to itself.
    /// Events can be registered in order to execute code when the value of a Bindable changes.
    /// </summary>
    /// <typeparam name="T">The object type that this Bindable wraps</typeparam>
    public class Bindable<T>
    {
        public class ValueChangedEvent
        {
            public readonly T NewValue;
            public readonly T OldValue;

            public ValueChangedEvent(T oldValue, T newValue)
            {
                NewValue = newValue;
                OldValue = oldValue;
            }
        }

        private Action<ValueChangedEvent>? callback;

        private WeakList<Bindable<T>> bindings = new();

        private WeakReference<Bindable<T>> weakReferenceInstance = null!;

        private WeakReference<Bindable<T>> weakReference => weakReferenceInstance ?? new WeakReference<Bindable<T>>(this);

        private T value;
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value)) return;

                TriggerValueChange(value, this);
            }
        }

        public Bindable(T value = default!)
        {
            this.value = value;
        }

        protected void TriggerValueChange(T newValue, Bindable<T> source)
        {
            var oldValue = value;
            value = newValue;

            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                callback?.Invoke(new ValueChangedEvent(oldValue, newValue));

            foreach (var binding in bindings)
                binding.Value = newValue;
        }

        public void BindTo(Bindable<T> otherBindable)
        {
            Value = otherBindable.Value;
            bindings.Add(otherBindable.weakReference);
            otherBindable.bindings.Add(weakReference);
        }

        public void UnbindFrom(Bindable<T> otherBindable)
        {
            bindings.Remove(otherBindable.weakReference);
            otherBindable.bindings.Remove(weakReference);
        }

        public void BindValueChanged(Action<ValueChangedEvent> e, bool runImmediately = false)
        {
            callback += e;

            if (runImmediately)
                e.Invoke(new ValueChangedEvent(Value, Value));
        }
    }
}
