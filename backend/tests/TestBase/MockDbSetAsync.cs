using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections;
using System.Linq.Expressions;

namespace CommerceHub.TestBase;

public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(source.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
        return mock;
    }
}

public class TestAsyncQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _inner;
    private readonly IAsyncQueryProvider _provider;

    public TestAsyncQueryable(IQueryable<T> inner, IAsyncQueryProvider provider)
    {
        _inner = inner;
        _provider = provider;
    }

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => _provider;
    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
}

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression)
    {
        var innerQuery = _inner.CreateQuery(expression);
        var elementType = innerQuery.ElementType;
        return (IQueryable)Activator.CreateInstance(
            typeof(TestAsyncQueryable<>).MakeGenericType(elementType),
            innerQuery, this)!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var inner = _inner.CreateQuery<TElement>(expression);
        return new TestAsyncQueryable<TElement>(inner, this);
    }

    public object Execute(Expression expression) => _inner.Execute(expression)!;
#pragma warning disable CS8603
    public TResult Execute<TResult>(Expression expression) => (TResult)_inner.Execute(expression)!;
#pragma warning restore CS8603

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                .MakeGenericMethod(innerType)
                .Invoke(this, [expression]);

            var fromResult = typeof(Task).GetMethods()
                .First(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethod && m.GetParameters().Length == 1)
                .MakeGenericMethod(innerType);
            return (TResult)fromResult.Invoke(null, [result])!;
        }

        return (TResult)Execute(expression)!;
    }
}

public class TestAsyncEnumerator<TEntity> : IAsyncEnumerator<TEntity>
{
    private readonly IEnumerator<TEntity> _inner;
    public TestAsyncEnumerator(IEnumerator<TEntity> inner) => _inner = inner;
    public TEntity Current => _inner.Current;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
}
