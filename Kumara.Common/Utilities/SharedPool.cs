// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Threading.Channels;

namespace Kumara.Common.Utilities;

public class SharedPool<T>
{
    private readonly Channel<T> _channel;
    private readonly HashSet<T> _checkedOut;

    public SharedPool(IEnumerable<T> items)
    {
        _channel = Channel.CreateBounded<T>(
            new BoundedChannelOptions(items.Count()) { FullMode = BoundedChannelFullMode.Wait }
        );

        foreach (var item in items)
        {
            if (!_channel.Writer.TryWrite(item))
            {
                throw new InvalidOperationException("Failed to write to channel");
            }
        }

        _checkedOut = new();
    }

    public async Task<T> CheckoutAsync(CancellationToken cancellationToken = default)
    {
        var item = await _channel.Reader.ReadAsync(cancellationToken);
        lock (_checkedOut)
        {
            _checkedOut.Add(item);
        }
        return item;
    }

    public async Task CheckinAsync(T item, CancellationToken cancellationToken = default)
    {
        lock (_checkedOut)
        {
            if (!_checkedOut.Remove(item))
            {
                throw new InvalidOperationException("Item was not checked out from this pool.");
            }
        }
        await _channel.Writer.WriteAsync(item, cancellationToken);
    }
}
