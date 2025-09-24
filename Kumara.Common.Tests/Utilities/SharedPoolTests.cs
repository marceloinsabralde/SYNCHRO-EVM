// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using Kumara.Common.Utilities;

namespace Kumara.Common.Tests.Utilities;

public class SharedPoolTests
{
    [Fact]
    public async Task StartsWithAllItemsInPool()
    {
        var items = new[] { 1, 2, 3 };
        var pool = new SharedPool<int>(items);

        var results = new List<int>();
        for (int i = 0; i < items.Length; i++)
        {
            results.Add(await pool.CheckoutAsync(TestContext.Current.CancellationToken));
        }

        results.ShouldBe(items);
    }

    [Fact]
    public async Task CanCheckoutAndCheckin()
    {
        var pool = new SharedPool<string>(new[] { "apple" });

        var item = await pool.CheckoutAsync(TestContext.Current.CancellationToken);
        item.ShouldBe("apple");

        await pool.CheckinAsync(item, TestContext.Current.CancellationToken);

        var item2 = await pool.CheckoutAsync(TestContext.Current.CancellationToken);
        item2.ShouldBe("apple");
    }

    [Fact]
    public async Task ErrorsWhenCheckingInWrongValue()
    {
        var pool = new SharedPool<string>(new[] { "apple" });

        var item = await pool.CheckoutAsync(TestContext.Current.CancellationToken);
        item.ShouldBe("apple");

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await pool.CheckinAsync("banana", TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task BlocksWhenPoolIsEmpty()
    {
        var pool = new SharedPool<int>(new[] { 1 });
        var item = await pool.CheckoutAsync(TestContext.Current.CancellationToken);

        var checkoutStarted = new TaskCompletionSource();
        var checkoutCompleted = new TaskCompletionSource<int>();

        var checkoutTask = Task.Run(
            async () =>
            {
                checkoutStarted.SetResult();
                var result = await pool.CheckoutAsync(TestContext.Current.CancellationToken);
                checkoutCompleted.SetResult(result);
            },
            TestContext.Current.CancellationToken
        );

        await checkoutStarted.Task;

        checkoutCompleted.Task.IsCompleted.ShouldBeFalse();

        await pool.CheckinAsync(item, TestContext.Current.CancellationToken);

        var result = await checkoutCompleted.Task;
        result.ShouldBe(item);
    }

    [Fact]
    public async Task CheckoutCanBeCanceled()
    {
        var pool = new SharedPool<int>(new[] { 1 });
        var item = await pool.CheckoutAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await pool.CheckoutAsync(cts.Token);
        });

        await pool.CheckinAsync(item, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SupportsConcurrentAccess()
    {
        var pool = new SharedPool<int>(Enumerable.Range(1, 10));

        var tasks = Enumerable
            .Range(0, 100)
            .Select(async _ =>
            {
                var item = await pool.CheckoutAsync(TestContext.Current.CancellationToken);
                await Task.Delay(5); // simulate work
                await pool.CheckinAsync(item, TestContext.Current.CancellationToken);
            });

        await Task.WhenAll(tasks);
    }
}
