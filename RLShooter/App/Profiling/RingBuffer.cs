namespace RLShooter.App.Profiling;

/// <summary>
/// A generic ring buffer for storing and calculating averages of a specified numeric type.
/// </summary>
/// <typeparam name="T">The type of values to store in the buffer. Must be a numeric struct type implementing <see cref="INumber{T}"/>.</typeparam>
public class RingBuffer<T> where T : struct, INumber<T>
{
    private readonly T[]  rawValues;
    private readonly T[]  avgValues;
    private readonly int  length;
    private          int  head = 0;
    private          int  tail;
    private          T    sum;
    private          T    countT;
    private          int  count         = 0;
    private          bool averageValues = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class with the specified length.
    /// </summary>
    /// <param name="length">The maximum length of the ring buffer.</param>
    public RingBuffer(int length) {
        rawValues   = new T[length];
        avgValues   = new T[length];
        this.length = length;
    }

    public T this[int index] {
        get => rawValues[(index + tail) % length];
        set => rawValues[(index + tail) % length] = value;
    }

    /// <summary>
    /// Gets the raw values stored in the ring buffer.
    /// </summary>
    public T[] Raw => rawValues;

    /// <summary>
    /// Gets the values stored in the ring buffer. If <see cref="AverageValues"/> is set to true, it returns the averaged values; otherwise, it returns the raw values.
    /// </summary>
    public T[] Values => averageValues ? avgValues : rawValues;

    /// <summary>
    /// Gets the value count of the ring buffer.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Gets the maximum length of the ring buffer.
    /// </summary>
    public int Length => length;

    /// <summary>
    /// Gets the tail position in the ring buffer.
    /// </summary>
    public int Tail => tail;

    /// <summary>
    /// Gets the head position in the ring buffer.
    /// </summary>
    public int Head => head;

    /// <summary>
    /// Gets the value at the tail position in the ring buffer.
    /// </summary>
    public T TailValue => Values[tail];

    /// <summary>
    /// Gets the value at the head position in the ring buffer.
    /// </summary>
    public T HeadValue => Values[head];

    /// <summary>
    /// Gets or sets a value indicating whether the values in the ring buffer should be averaged.
    /// </summary>
    public bool AverageValues {
        get => averageValues;
        set => averageValues = value;
    }

    /// <summary>
    /// Adds a value to the ring buffer, updating the calculated average if necessary.
    /// </summary>
    /// <param name="value">The value to add to the ring buffer.</param>
    public void Add(T value) {
        if (value < default(T)) {
            value = default;
        }

        // Subtract the oldest value from the sum if the buffer is full
        if (count == length) {
            sum -= rawValues[tail];
        } else {
            count++;
            countT++;
        }

        // Add the new value to the sum
        sum += value;

        avgValues[head] = CalculateAverage();
        rawValues[head] = value;

        head = (head + 1) % length;
        tail = (head - count + length) % length;
    }

    /// <summary>
    /// Calculates the average of the values in the ring buffer.
    /// </summary>
    /// <returns>The calculated average value.</returns>
    public T CalculateAverage() {
        if (count == 0) {
            // The buffer is empty, return the default value of T
            return default;
        }

        // Calculate and return the average
        return sum / countT;
    }
}