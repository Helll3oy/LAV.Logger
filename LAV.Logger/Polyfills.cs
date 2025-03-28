using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace LAV.Logger
{
#if NET452 // !NETSTANDARD2_1_OR_GREATER && NETSTANDARD2_0 || NET462_OR_GREATER
    //public class CustomArrayBufferWriter<T>
    //{
    //    private T[] _buffer;
    //    private int _writtenCount;

    //    public CustomArrayBufferWriter(int initialCapacity = 256)
    //    {
    //        _buffer = new T[initialCapacity];
    //    }

    //    public Span<T> GetSpan(int sizeHint = 0)
    //    {
    //        if (sizeHint == 0)
    //            sizeHint = 1;

    //        if (_writtenCount + sizeHint > _buffer.Length)
    //        {
    //            // Double the buffer size or meet the sizeHint
    //            int newSize = Math.Max(_buffer.Length * 2, _writtenCount + sizeHint);
    //            Array.Resize(ref _buffer, newSize);
    //        }

    //        return new Span<T>(_buffer, _writtenCount, _buffer.Length - _writtenCount);
    //    }

    //    public void Advance(int count)
    //    {
    //        if (count < 0 || _writtenCount + count > _buffer.Length)
    //            throw new ArgumentOutOfRangeException(nameof(count));

    //        _writtenCount += count;
    //    }

    //    public ReadOnlySpan<T> WrittenSpan => new ReadOnlySpan<T>(_buffer, 0, _writtenCount);
    //}

    public class Utf8JsonWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly bool _indented;
        private int _indentLevel;
        private bool _isFirstProperty = true;

        public Utf8JsonWriter(Stream stream, bool indented = false)
        {
            _writer = new StreamWriter(stream, Encoding.UTF8);
            _indented = indented;
        }

        public Utf8JsonWriter(StringBuilder builder, bool indented = false)
        {
            _writer = new StringWriter(builder);
            _indented = indented;
        }

        public void WriteStartObject(string name = null)
        {
            if (name != null)
            {
                WritePropertyName(name);
            }
            else
            {
                WriteIndent();
            }
            _writer.Write('{');
            _indentLevel++;
            _isFirstProperty = true;
        }

        public void WriteEndObject()
        {
            _indentLevel--;
            WriteIndent();
            _writer.Write('}');
            _isFirstProperty = false;
        }

        public void WriteStartArray(string name = null)
        {
            if (name != null)
            {
                WritePropertyName(name);
            }
            else
            {
                WriteIndent();
            }
            _writer.Write('[');
            _indentLevel++;
            _isFirstProperty = true;
        }

        public void WriteEndArray()
        {
            _indentLevel--;
            WriteIndent();
            _writer.Write(']');
            _isFirstProperty = false;
        }

        public void WritePropertyName(string name)
        {
            WriteIndent();
            if (!_isFirstProperty)
                _writer.Write(',');
            _writer.Write($"\"{EscapeString(name)}\":");
            _isFirstProperty = false;
        }

        public void WriteStringValue(string value)
        {
            WriteIndent();
            _writer.Write($"\"{EscapeString(value)}\"");
        }

        public void WriteString(string name, string value)
        {
            WritePropertyName(name);
            _writer.Write($"\"{EscapeString(value)}\"");
        }

        public void WriteNumberValue(int value)
        {
            WriteIndent();
            _writer.Write(value);
        }
        public void WriteNumberValue(long value)
        {
            WriteIndent();
            _writer.Write(value);
        }

        public void WriteNumber(string name, int value)
        {
            WritePropertyName(name);
            _writer.Write(value);
        }
        public void WriteNumber(string name, long value)
        {
            WritePropertyName(name);
            _writer.Write(value);
        }

        public void WriteBooleanValue(bool value)
        {
            WriteIndent();
            _writer.Write(value ? "true" : "false");
        }

        public void WriteBoolean(string name, bool value)
        {
            WritePropertyName(name);
            _writer.Write(value ? "true" : "false");
        }

        public void WriteNullValue()
        {
            WriteIndent();
            _writer.Write("null");
        }

        public void WriteNull(string name)
        {
            WritePropertyName(name);
            _writer.Write("null");
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        private void WriteIndent()
        {
            if (_indented)
            {
                _writer.WriteLine();
                for (int i = 0; i < _indentLevel; i++)
                    _writer.Write("  ");
            }
        }

        private static string EscapeString(string value)
        {
            return value?.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\b", "\\b")
                        .Replace("\f", "\\f")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t");
        }
    }

#endif

#if NET452
    public class ArrayPool<T>
    {
        private static readonly ConcurrentBag<T[]> _pool = new ConcurrentBag<T[]>();

        private static readonly Lazy<ArrayPool<T>> _instance = new Lazy<ArrayPool<T>>(() => new ArrayPool<T>());

        public static ArrayPool<T> Shared => _instance.Value;

        public T[] Rent(int minimumLength)
            {
                if (_pool.TryTake(out var array) && array.Length >= minimumLength)
                {
                    return array;
                }
                return new T[minimumLength];
            }

            public void Return(T[] array, bool clearArray = false)
            {
                if (clearArray)
                {
                    Array.Clear(array, 0, array.Length);
                }
                _pool.Add(array);
            }
    }

    internal class AsyncLocal<T>
    {
        private readonly string _name;

        public AsyncLocal(string name = null)
        {
            _name = name ?? Guid.NewGuid().ToString();
        }

        public T Value
        {
            get => (T)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(_name);
            set => System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(_name, value);
        }
    }

    public class CustomScopeProvider
    {
        private readonly AsyncLocal<Stack<object>> _scopeStack = new AsyncLocal<Stack<object>>();  

        public IDisposable Push(object state) => new DisposableScope(_scopeStack, state);

        public IEnumerable<object> GetScopeValues()
        {
            if (_scopeStack.Value == null || _scopeStack.Value.Count == 0)
                yield break;

            foreach (var item in _scopeStack.Value)
                yield return item;
        }

        public void ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
            if (_scopeStack.Value == null)
                return;

            foreach (var scope in _scopeStack.Value)
                callback(scope, state);
        }

        private sealed class DisposableScope : IDisposable
        {
            private readonly AsyncLocal<Stack<object>> _scopeStack;

            public DisposableScope(AsyncLocal<Stack<object>> scopeStack, object state)
            {
                _scopeStack = scopeStack;
                Push(state);
            }

            public void Push(object state)
            {
                _scopeStack.Value ??= new Stack<object>();
                _scopeStack.Value.Push(state);
            }

            public void Dispose()
            {
                _scopeStack.Value?.Pop();
            }
        }
    }
#endif

#if !NET452
    internal class CustomChannel<T> : System.Threading.Channels.Channel<T>
    { }
#else
    internal class CustomChannel<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _itemsAvailable = new SemaphoreSlim(0, int.MaxValue);
        private readonly SemaphoreSlim _spaceAvailable;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private bool _isCompleted = false;
        private bool disposedValue;

        public ChannelReader<T> Reader { get; }
        public ChannelWriter<T> Writer { get; }

        public CustomChannel(int capacity = int.MaxValue)
        {
            _spaceAvailable = new SemaphoreSlim(capacity, capacity);
            Reader = new ChannelReader<T>(this);
            Writer = new ChannelWriter<T>(this);
        }

        public async Task<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(CustomChannel<T>));

            await _spaceAvailable.WaitAsync(cancellationToken); // Wait for space to be available

            return true;
        }

        public async Task WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(CustomChannel<T>));

            await _spaceAvailable.WaitAsync(cancellationToken); // Wait for space to be available

            _queue.Enqueue(item); // Enqueue the item
            _itemsAvailable.Release(); // Signal that an item is available
        }

        public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(CustomChannel<T>));

            await _itemsAvailable.WaitAsync(cancellationToken); // Wait for an item to be available

            if (_queue.TryDequeue(out T item))
            {
                _spaceAvailable.Release(); // Signal that space is available
                return item;
            }

            //return default;
            throw new InvalidOperationException("No item available in the queue.");
        }

        // Internal method to mark the channel as complete
        private void CompleteInternal()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(CustomChannel<T>));

            if (!_isCompleted)
            {
                _isCompleted = true;

                if (_itemsAvailable.CurrentCount == 0)
                    _itemsAvailable.Release(int.MaxValue); // Release all waiting readers
            }
        }

        public void Complete()
        {
            _lock.Wait();
            try
            {
                CompleteInternal();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                await Task.Run(() => CompleteInternal(), cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        // Internal method to check if the channel is completed
        private bool IsCompletedInternal()
        {
            return _isCompleted && _queue.Count == 0;
        }

        public bool IsCompleted()
        {
            return IsCompletedInternal();
        }

        public Task<bool> IsCompletedAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() => IsCompletedInternal(), cancellationToken);
        }

        public void Reset()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(CustomChannel<T>));

            _lock.Wait();
            try
            {
                _isCompleted = false;
            }
            finally
            {
                _lock.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                // TODO: освободить управляемое состояние (управляемые объекты)

            }

            // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
            // TODO: установить значение NULL для больших полей
            disposedValue = true;

            _lock.Dispose();
            _itemsAvailable.Dispose();
            _spaceAvailable.Dispose();
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~CustomChannel()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class ChannelReader<T>
    {
        private readonly CustomChannel<T> _channel;

        internal ChannelReader(CustomChannel<T> channel)
        {
            _channel = channel;
        }

        // Read an item from the channel
        public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            return await _channel.ReadAsync(cancellationToken);
        }

        // Check if the channel is completed and empty
        public bool IsCompleted()
        {
            return _channel.IsCompleted();
        }

        public Task<bool> IsCompletedAsync(CancellationToken cancellationToken = default)
        {
            return _channel.IsCompletedAsync(cancellationToken);
        }
    }

    internal class ChannelWriter<T>
    {
        private readonly CustomChannel<T> _channel;

        internal ChannelWriter(CustomChannel<T> channel)
        {
            _channel = channel;
        }

        // Write an item to the channel
        public async Task WriteAsync(T item, CancellationToken cancellationToken = default)
        {
            await _channel.WriteAsync(item, cancellationToken);
        }

        // Mark the channel as complete
        public void Complete()
        {
            _channel.Complete();
        }

        public async Task CompleteAsync()
        {
            await _channel.CompleteAsync();
        }
    }
#endif

    public static class TaskHelpers
    {
#if !(NETCOREAPP2_1 || NETSTANDARD2_1 || NET5_0_OR_GREATER || NET461_OR_GREATER)
        public static readonly Task CompletedTask = Task.FromResult<object>(null);

        public static Task<T> CreateCanceledTask<T>(CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled(); // Mark the task as canceled
            return tcs.Task;
        }

        public static Task CreateCanceledTask(CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<byte>();
            tcs.SetCanceled(); // Mark the task as canceled
            return tcs.Task;
        }

        public static ValueTask<T> CreateCanceledValueTask<T>(CancellationToken cancellationToken = default)
        {
            // Create a canceled Task and wrap it in a ValueTask
            var canceledTask = CreateCanceledTask<T>(cancellationToken);
            return new ValueTask<T>(canceledTask);
        }

        public static ValueTask CreateCanceledValueTask(CancellationToken cancellationToken = default)
        {
            // Create a canceled Task and wrap it in a ValueTask
            var canceledTask = CreateCanceledTask(cancellationToken);
            return new ValueTask(canceledTask);
        }
#endif
    }
}
