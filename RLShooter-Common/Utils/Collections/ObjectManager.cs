using System.Collections;

namespace RLShooter.Common.Utils.Collections;

public class ObjectManager<T> : IEnumerable<T> where T : class {
    private readonly T[]        _objects;
    private readonly Stack<int> _freeList; // Tracks free indices
    private readonly int        _maxSize;

    public ObjectManager(int maxSize) {
        _maxSize  = maxSize;
        _objects  = new T[_maxSize];
        _freeList = new Stack<int>(_maxSize);

        // Initialize the free list with all indices
        for (var i = _maxSize - 1; i >= 0; i--) {
            _freeList.Push(i);
        }
    }
    public bool CanCreateObject() => _freeList.Count > 0;
    /// <summary>
    /// Adds an object to the manager, returning true if successful.
    /// </summary>
    public bool Add(T obj) {
        if (_freeList.Count == 0)
            return false; // No free slot available

        var index = _freeList.Pop(); // Get the next free index
        _objects[index] = obj;
        return true;
    }

    /// <summary>
    /// Removes an object from the manager, returning true if successful.
    /// </summary>
    public bool Remove(T obj) {
        for (var i = 0; i < _maxSize; i++) {
            if (_objects[i] != obj)
                continue;

            _objects[i] = null; // Remove the object
            _freeList.Push(i);  // Reclaim the index
            return true;
        }
        return false; // Object not found
    }

    /// <summary>
    /// Removes an object at the specified index.
    /// </summary>
    public bool RemoveAt(int index) {
        if (index < 0 || index >= _maxSize || _objects[index] == null)
            return false; // Invalid index or no object at the index

        _objects[index] = null; // Remove the object
        _freeList.Push(index);  // Reclaim the index
        return true;
    }

    /// <summary>
    /// Returns an enumerable collection of active objects.
    /// </summary>
    public IEnumerable<T> GetActiveObjects() {
        foreach (var obj in _objects) {
            if (obj != null)
                yield return obj;
        }
    }

    /// <summary>
    /// Loops through all active objects and applies the provided action.
    /// </summary>
    public void ForEach(Action<T> action) {
        foreach (var obj in _objects) {
            if (obj != null)
                action(obj);
        }
    }
    public IEnumerator<T>   GetEnumerator() => GetActiveObjects().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}