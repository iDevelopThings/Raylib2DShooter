using System.Collections;
using RLShooter.App.Events;


namespace RLShooter.App.Editor;

public interface IEditorSelectable {
    public bool IsSelectedInEditor { get; set; }
}

public class SelectionCollection : ICollection<object>, IEnumerable, IEnumerable<object> {
    private readonly List<object> _objects = [];
    private          Type         type;

    public object this[int index] {
        get => _objects[index];
        set => _objects[index] = value;
    }

    public int Count => _objects.Count;

    public bool   SelectedMultiple => _objects.Count > 1;
    public Type   Type             => type;
    public object SyncRoot         => this;

    public bool IsReadOnly => ((ICollection<object>) _objects).IsReadOnly;

    public static readonly SelectionCollection Global = new();


    private readonly EventHandlers<SelectionCollection, SelectionCollection> SelectionChangedEvent = new();
    public event EventHandler<SelectionCollection, SelectionCollection> SelectionChanged {
        add => SelectionChangedEvent.AddHandler(value);
        remove => SelectionChangedEvent.RemoveHandler(value);
    }

    public void AddSelection(object obj) {
        if (obj is IEditorSelectable selectable) {
            selectable.IsSelectedInEditor = true;
        }

        if (_objects.Count == 0) {
            type = obj.GetType();
        } else if (!(type?.IsInstanceOfType(obj) ?? true)) {
            type = null;
        }

        if (_objects.Contains(obj)) {
            return;
        }

        _objects.Add(obj);

        SelectionChangedEvent.Invoke(this, this);
    }

    public void AddOverwriteSelection(object obj) {
        ClearSelection();
        AddSelection(obj);
    }

    public void AddMultipleSelection(IEnumerable<object> objects) {
        foreach (var obj in objects) {
            AddSelection(obj);
        }
    }

    public bool RemoveSelection(object item) {
        if (item is IEditorSelectable selectable) {
            selectable.IsSelectedInEditor = false;
        }

        var result = _objects.Remove(item);

        if (_objects.Count == 0) {
            type = null;
        }

        SelectionChangedEvent.Invoke(this, this);

        return result;
    }

    public void RemoveMultipleSelection(IEnumerable<object> objects) {
        foreach (var obj in objects) {
            RemoveSelection(obj);
        }
    }

    public void ClearSelection() {
        foreach (var obj in _objects) {
            if (obj is IEditorSelectable selectable) {
                selectable.IsSelectedInEditor = false;
            }
        }

        _objects.Clear();
        type = null;

        SelectionChangedEvent.Invoke(this, this);
    }

    public object First() => _objects.Count == 0 ? null : _objects[0];

    public object Last() => _objects[^1];

    public T First<T>() {
        for (var i = 0; i < _objects.Count; i++) {
            var obj = _objects[i];
            if (obj is T t) {
                return t;
            }
        }

        return default;
    }

    public T Last<T>() {
        for (var i = _objects.Count - 1; i >= 0; i--) {
            var obj = _objects[i];
            if (obj is T t) {
                return t;
            }
        }

        return default;
    }

    public bool TryGetLast<T>(out T obj) {
        for (var i = _objects.Count - 1; i >= 0; i--) {
            if (_objects[i] is not T t)
                continue;

            obj = t;
            return true;
        }

        obj = default;
        return false;
    }

    public IEnumerable<T> Get<T>() {
        foreach (var obj in _objects) {
            if (obj is T t) {
                yield return t;
            }
        }
    }

    public bool Contains(object item) {
        return _objects.Contains(item);
    }

    public void CopyTo(object[] array, int arrayIndex) {
        _objects.CopyTo(array, arrayIndex);
    }

    public IEnumerator<object> GetEnumerator() {
        return _objects.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _objects.GetEnumerator();
    }

    public void Add(object item) {
        AddSelection(item);
    }

    public void Clear() {
        ClearSelection();
    }

    public bool Remove(object item) {
        return RemoveSelection(item);
    }
    public bool HasAny()    => _objects.Count > 0;
    public bool HasAny<T>() => _objects.Any(o => o is T);
}