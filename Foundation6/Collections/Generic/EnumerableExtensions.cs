﻿namespace Foundation.Collections.Generic;

using Foundation;
using Foundation.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

public static class EnumerableExtensions
{
    private class ElseIf<T> : IElseIf<T>
    {
        private readonly IEnumerable<T> _items;

        public ElseIf([DisallowNull] IEnumerable<T> items)
        {
            _items = items.ThrowIfNull();
        }

        public IEnumerable<T> Else() => _items;

        public IEnumerable<T> Else([DisallowNull] Action<T> action)
        {
            foreach (var item in _items)
            {
                action(item);
                yield return item;
            }
        }

        public void EndIf()
        {
            foreach (var item in Else())
            {
            }
        }

        IElseIf<T> IElseIf<T>.ElseIf([DisallowNull] Func<T, bool> condition, Action<T> action)
        {
            return _items.If(condition, action);
        }
    }

    private class ElseResult<T, TResult> : IElse<T, TResult>
    {
        private readonly IEnumerable<T> _items;
        private readonly Func<T, bool> _predicate;
        private readonly Func<T, TResult> _mapIf;

        public ElseResult(
            [DisallowNull] IEnumerable<T> items,
            [DisallowNull] Func<T, bool> predicate,
            [DisallowNull] Func<T, TResult> mapIf)
        {
            _items = items.ThrowIfNull();
            _predicate = predicate.ThrowIfNull();
            _mapIf = mapIf.ThrowIfNull();
        }

        public IEnumerable<TResult> Else([DisallowNull] Func<T, TResult> map)
        {
            foreach (var item in _items)
            {
                yield return _predicate(item) ? _mapIf(item) : map(item);
            }
        }
    }

    /// <summary>
    /// Adds an item if the list is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> AddIfEmpty<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T> factory)
    {
        factory.ThrowIfNull();

        var it = items.GetEnumerator();
        if (!it.MoveNext())
        {
            yield return factory();
        }
        else
        {
            yield return it.Current;

            while (it.MoveNext()) yield return it.Current;
        }
    }

    /// <summary>
    /// Executes action after every item except the last one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> AfterEveryElement<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action action)
    {
        action.ThrowIfNull();

        var it = items.GetEnumerator();
        var next = it.MoveNext();
        while (next)
        {
            yield return it.Current;
            next = it.MoveNext();
            if (next)
                action();
        }
    }

    /// <summary>
    /// Aggregates the elements like the standard LINQ with the difference that this method does not require a seed value.
    /// The first element is taken as seed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TAccumulate"></typeparam>
    /// <param name="items"></param>
    /// <param name="seed"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Opt<TAccumulate> Aggregate<T, TAccumulate>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, TAccumulate> seed,
        [DisallowNull] Func<TAccumulate, T, TAccumulate> func)
    {
        seed.ThrowIfNull();
        func.ThrowIfNull();

        if (!items.FirstAsOpt().Is(out T? item)) return Opt.None<TAccumulate>();

        return Opt.Some(items.Aggregate(seed(item!), func));
    }

    /// <summary>
    /// Returns at least numberOfElements elements. If the number of elements is smaller, an empty enumerable is returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">Elements of the list.</param>
    /// <param name="numberOfElements"></param>
    /// <returns></returns>
    public static IEnumerable<T> AtLeast<T>([DisallowNull] this IEnumerable<T> items, int numberOfElements)
    {
        if (0 > numberOfElements) throw new ArgumentOutOfRangeException(nameof(numberOfElements), "cannot be negative");

        var it = items.GetEnumerator();

        var required = new List<T>();
        while (it.MoveNext())
        {
            required.Add(it.Current);
            if (required.Count == numberOfElements) break;
        }

        if (required.Count != numberOfElements) yield break;

        foreach (var item in required)
        {
            yield return item;
        }

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Returns at least numberOfElements elements. If the number of elements is smaller, an empty enumerable is returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="numberOfElements"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> AtLeast<T>([DisallowNull] this IEnumerable<T> items, int numberOfElements, [DisallowNull] Func<T, bool> predicate)
    {
        if (0 > numberOfElements) throw new ArgumentOutOfRangeException(nameof(numberOfElements), "cannot be negative");
        predicate.ThrowIfNull();

        var it = items.GetEnumerator();

        var required = new List<T>();
        while (it.MoveNext())
        {
            if (required.Count == numberOfElements) break;

            if (predicate(it.Current))
                required.Add(it.Current);
        }

        if (required.Count != numberOfElements) yield break;

        foreach (var item in required)
        {
            yield return item;
        }

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Returns the median of all values returned by the converter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="converter"></param>
    /// <returns></returns>
    public static decimal AverageMedian<T>([DisallowNull] this IEnumerable<T> items, Func<T, decimal>? converter = null)
    {
        var (opt1, opt2) = AverageMedianValues(items);
        if (opt1.IsNone) return 0;

        var value1 = (null == converter)
            ? Convert.ToDecimal(opt1.ValueOrThrow())
            : converter(opt1.ValueOrThrow());

        if (opt2.IsNone) return value1;

        var value2 = (null == converter)
            ? Convert.ToDecimal(opt2.ValueOrThrow())
            : converter(opt2.ValueOrThrow());

        return (value1 + value2) / 2M;
    }

    /// <summary>
    /// Returns the real values instead of a division of the median values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static (Opt<T> value1, Opt<T> value2) AverageMedianValues<T>([DisallowNull] this IEnumerable<T> items)
    {
        var sorted = items.OrderBy(x => x);
        var count = sorted.Count();
        int halfIndex = count / 2;

        return (count % 2 == 0)
            ? (Opt.Some(sorted.ElementAt(halfIndex - 1)), Opt.Some(sorted.ElementAt(halfIndex)))
            : (Opt.Some(sorted.ElementAt(halfIndex)), Opt.None<T>());
    }

    /// <summary>
    /// Creates a cartesian product from the lists lhs and rhs.
    /// </summary>
    /// <typeparam name="T">Type of the list.</typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static IEnumerable<(T, T)> CartesianProduct<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        return from l in lhs
               from r in rhs
               select (l, r);
    }

    /// <summary>
    /// Checks if lhs contains at least one element of rhs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool Contains<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        var search = new HashSet<T>(rhs);
        return search.Overlaps(lhs);
    }

    /// <summary>
    /// Creates an endless list of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> CycleEnumerate<T>([DisallowNull] this IEnumerable<T> items)
    {
        return new CyclicEnumerable<T>(items);
    }

    /// <summary>
    /// Cycles a counter between min and max. If the counter reaches max it starts with min.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns>A tuple containing a counter and the item.</returns>
    public static IEnumerable<(int, T)> CycleEnumerate<T>([DisallowNull] this IEnumerable<T> items, int min, int max)
    {
        return new CyclicEnumerable<T, int>(items, min, max, idx => idx + 1);
    }

    /// <summary>
    /// Cycles a counter between min and max. If the counter reaches max it starts with min.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TCount"></typeparam>
    /// <param name="items"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="increment">The counter will be increased by this function.</param>
    /// <returns></returns>
    public static IEnumerable<(TCount, T)> CycleEnumerate<T, TCount>(
        [DisallowNull] this IEnumerable<T> items
        , TCount min
        , TCount max
        , [DisallowNull] Func<TCount, TCount> increment)
        where TCount : IComparable<TCount>
    {
        return new CyclicEnumerable<T, TCount>(items, min, max, increment);
    }


    /// <summary>
    /// returns the symmetric difference of two lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static IEnumerable<T> Difference<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        var diff = new HashSet<T>(lhs);
        diff.SymmetricExceptWith(rhs);
        return diff;
    }

    /// <summary>
    /// Removes all duplicates from a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="comparer">a compare function to compare the items.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> DistinctBy<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T?, T?, bool> comparer)
    {
        comparer.ThrowIfNull();

        return DistinctBy(items, comparer, null);
    }

    /// <summary>
    /// Removes all duplicates from a list. With an additional hash function you can change the sorting order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="comparer"></param>
    /// <param name="hashFunc">The hash function for each single item. Example: you want all null items at the end of the list. Default: null is -1.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> DistinctBy<T>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T?, T?, bool> comparer,
        Func<T?, int>? hashFunc)
    {
        comparer.ThrowIfNull();

        return items.Distinct(new LambdaEqualityComparer<T>(comparer, hashFunc));
    }

    /// <summary>
    /// This Distinct preserves the ordinal position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> DistinctPreserveOrdinalPosition<T>([DisallowNull] this IEnumerable<T> items)
    {
        return items.Enumerate()
                    .DistinctBy(tuple => tuple.item)
                    .OrderBy(tuple => tuple.counter)
                    .Select(tuple => tuple.item);
    }

    /// <summary>
    /// returns the duplicate items of a list. If there are e.g. three of an item, 2 will returned.
    /// If distinct is true, only one example of every duplicate item is returned. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="distinct">If true, only one example of every duplicate item is returned.</param>
    /// <returns></returns>
    public static IEnumerable<T> Duplicates<T>([DisallowNull] this IEnumerable<T> items, bool distinct = false)
    {
        if (null == items) throw new ArgumentNullException(nameof(items));

        var duplicates = items.GroupBy(x => x).SelectMany(x => x.Skip(1));
        return distinct ? duplicates.Distinct() : duplicates;
    }

    /// <summary>
    /// Returns an empty enumerable if items is null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> EmptyIfNull<T>([DisallowNull] this IEnumerable<T> items)
    {
        return items ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Enumerates items starting from seed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="seed"></param>
    /// <returns>Returns tuples (item, counter).</returns>
    public static IEnumerable<(T item, int counter)> Enumerate<T>([DisallowNull] this IEnumerable<T> items, int seed = 0)
    {
        var i = seed;

        return Enumerate(items, (item) => i++);
    }

    /// <summary>
    /// Enumerates items. createValue is called on every item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="items"></param>
    /// <param name="createValue"></param>
    /// <returns>Returns a tuple (item, counter).</returns>
    public static IEnumerable<(T item, TValue counter)> Enumerate<T, TValue>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, TValue> createValue)
    {
        createValue.ThrowIfNull();

        foreach (var item in items)
            yield return (item, createValue(item));
    }

    /// <summary>
    /// Enumerates items. Starting from Min until Max. If the counter reaches Max it starts again from Min.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="minMax"></param>
    /// <returns></returns>
    public static IEnumerable<(T item, int counter)> Enumerate<T>([DisallowNull] this IEnumerable<T> items, MinMax<int> minMax)
    {
        var i = minMax.Min;
        foreach (var item in items)
        {
            if (i > minMax.Max) i = minMax.Min;

            yield return (item, i);
            i++;
        }
    }

    /// <summary>
    /// Enumerates items. Starting from Min until Max. If the counter reaches Max it starts again from Min.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue1"></typeparam>
    /// <typeparam name="TValue2"></typeparam>
    /// <param name="items"></param>
    /// <param name="createValue1"></param>
    /// <param name="createValue2"></param>
    /// <returns></returns>
    public static IEnumerable<(T, TValue1, TValue2)> Enumerate<T, TValue1, TValue2>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, TValue1> createValue1,
        [DisallowNull] Func<T, TValue2> createValue2)
    {
        foreach (var item in items)
            yield return (item, createValue1(item), createValue2(item));
    }

    /// <summary>
    /// Returns all items except item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> Except<T>([DisallowNull] this IEnumerable<T> items, T? item)
    {
        Func<T, bool> equals = null == item 
            ? x => null == x || x.Equals(item)
            : x => null != x && x.Equals(item);

        foreach (var elem in items)
        {
            if (equals(elem)) continue;

            yield return elem;
        }
    }

    /// <summary>
    /// Returns all lhs elements which are not in rhs. The comparision is made between the TKey values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <param name="keySelector"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> Except<T, TKey, TResult>(
        [DisallowNull] this IEnumerable<T> lhs,
        [DisallowNull] IEnumerable<T> rhs,
        [DisallowNull] Func<T, TKey> keySelector,
        [DisallowNull] Func<T?, TResult> resultSelector)
    {
        rhs.ThrowIfNull();
        keySelector.ThrowIfNull();
        resultSelector.ThrowIfNull();

        return lhs.Except(rhs, new LambdaEqualityComparer<T>((T? a, T? b) =>
        {
            if (null == a) return null == b;
            if (null == b) return true;

            var selectedA = keySelector(a);
            var selectedB = keySelector(b);
            if (null == selectedA) return null == selectedB;
            if (null == selectedB) return false;

            return selectedA.Equals(selectedB);
        })).Select(resultSelector);
    }

    /// <summary>
    /// Returns all elements of first which are not in second.
    /// </summary>
    /// <typeparam name="T1">Type of elements of first.</typeparam>
    /// <typeparam name="T2">Type of elements of second.</typeparam>
    /// <typeparam name="TKey">Type of the compare value.</typeparam>
    /// <typeparam name="TResult">Value which will be returned of the first list.</typeparam>
    /// <param name="lhs">First list.</param>
    /// <param name="rhs">Second list.</param>
    /// <param name="lhsKeySelector">Compare value of the elements of the first list. Must be of type TKey.</param>
    /// <param name="rhsKeySelector">Compare value of the elements of the second list. Must be of type TKey.</param>
    /// <param name="resultSelector">Rückgabe-Wert. Hier kann auch ein neuer Typ erzeugt werden.</param>
    /// <returns></returns>
    public static IEnumerable<TResult> Except<T1, T2, TKey, TResult>(
        [DisallowNull] this IEnumerable<T1> lhs,
        [DisallowNull] IEnumerable<T2> rhs,
        [DisallowNull] Func<T1, TKey> lhsKeySelector,
        [DisallowNull] Func<T2, TKey> rhsKeySelector,
        [DisallowNull] Func<T1, TResult> resultSelector)
    {
        rhs.ThrowIfNull();
        lhsKeySelector.ThrowIfNull();
        rhsKeySelector.ThrowIfNull();
        resultSelector.ThrowIfNull();

        var hashedRhs = new HashSet<TKey>(rhs.Select(rhsKeySelector));
        return lhs.Where(l => !hashedRhs.Contains(lhsKeySelector(l))).Select(resultSelector);
    }

    /// <summary>
    /// Filters and transform items. It returns only Opt.Some values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> FilterMap<T, TResult>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, Opt<TResult>> selector)
    {
        selector.ThrowIfNull();

        foreach (var item in items)
        {
            var option = selector(item);
            if (option.IsSome) yield return option.ValueOrThrow();
        }
    }

    /// <summary>
    /// Filter and transform items. The functor match is called when predicate is true otherwise noMatch is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <param name="match"></param>
    /// <param name="noMatch"></param>
    /// <returns>Tuples with item and index.</returns>
    public static IEnumerable<(TResult item, int index)> FilterMapIndexed<T, TResult>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, Opt<TResult>> selector)
    {
        selector.ThrowIfNull();

        var i = 0;
        foreach (var item in items)
        {
            var option = selector(item);
            if (option.IsSome) yield return (item: option.ValueOrThrow(), index: i);

            ++i;
        }
    }

    /// <summary>
    /// Searches items until all predicates matched exactly one time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicates"></param>
    /// <returns></returns>
    public static IEnumerable<T> FindUntil<T>([DisallowNull] this IEnumerable<T> items, params Func<T, bool>[] predicates)
    {
        var invasivePredicates = new InvasiveVerification<T>(predicates);
        var isNone = new TriState();
        var isTrue = new TriState(true);

        foreach (var item in items)
        {
            var triState = invasivePredicates.Verify(item);
            if (isNone == triState) yield break;
            if (isTrue == triState) yield return item;
        }
    }

    public static Opt<T> FirstAsOpt<T>([DisallowNull] this IEnumerable<T> items)
    {
        if (null == items) return Opt.None<T>();

        foreach (var item in items)
            return Opt.Some(item);

        return Opt.None<T>();
    }

    /// <summary>
    /// Returns the first item that matches the predicate or None.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Opt<T> FirstAsOpt<T>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, bool> predicate)
    {
        if (null == items) return Opt.None<T>();
        predicate.ThrowIfNull();

        foreach (var item in items.Where(predicate))
            return Opt.Some(item);

        return Opt.None<T>();
    }

    /// <summary>
    /// Returns the first item that matches the predicate or None.
    /// </summary>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <param name="items">a list of results.</param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Opt<TOk> FirstOk<TOk, TError>(
        [DisallowNull] this IEnumerable<Result<TOk, TError>> items, 
        [DisallowNull] Func<TOk, bool> predicate)
    {
        predicate.ThrowIfNull();

        return items.SelectOk().FirstAsOpt(predicate);
    }

    /// <summary>
    /// Returns a flat list of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collections"></param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>([DisallowNull] this IEnumerable<IEnumerable<T>> collections)
    {
        return collections.SelectMany(x => x);
    }

    /// <summary>
    /// Executes action for every item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <returns>The number of executed actions.</returns>
    public static void ForEach<T>(this IEnumerable<T> source, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes action vor every item. If the list is empty
    /// </summary>
    /// <typeparam name="T">Type of the item.</typeparam>
    /// <param name="source"></param>
    /// <param name="action">Will be executed for every item.</param>
    /// <param name="emptyAction">Will be executed if source is empty.</param>
    /// <returns>The number of executed actions.</returns>
    public static void ForEach<T>(
        [DisallowNull] this IEnumerable<T> source,
        [DisallowNull] Action<T> action,
        [DisallowNull] Action emptyAction)
    {
        action.ThrowIfNull();
        emptyAction.ThrowIfNull();

        var it = source.GetEnumerator();
        if(!it.MoveNext())
        {
            emptyAction();
            return;
        }

        var item = it.Current;
        action(item);
        
        while(it.MoveNext())
        {
            item = it.Current;
            action(item);
        }
    }

    /// <summary>
    /// Returns a list of items. The predicate allows filtering items by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> FromIndex<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<long, bool> predicate)
    {
        predicate.ThrowIfNull();

        long i = 0;
        return items.Where(item => predicate(i++));
    }

    public static IElseIf<T> If<T>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, bool> predicate,
        [DisallowNull] Action<T> action)
    {
        predicate.ThrowIfNull();
        action.ThrowIfNull();

        var @else = Enumerable.Empty<T>();

        foreach (var item in items)
        {
            if (predicate(item))
            {
                action(item);
                continue;
            }
            @else = @else.Append(item);
        }
        return new ElseIf<T>(@else);
    }

    public static IElse<T, TResult> If<T, TResult>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, bool> predicate, 
        [DisallowNull] Func<T, TResult> map)
    {
        predicate.ThrowIfNull();
        map.ThrowIfNull();

        return new ElseResult<T, TResult>(items, predicate, map);
    }

    /// <summary>
    /// Returns lhs. If lhs is empty rhs is returned;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static IEnumerable<T> IfEmpty<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        rhs.ThrowIfNull();

        var lIt = lhs.GetEnumerator();
        if (!lIt.MoveNext())
        {
            foreach (var r in rhs)
            {
                yield return r;
            }
            yield break;
        }

        yield return lIt.Current;

        while (lIt.MoveNext())
        {
            yield return lIt.Current;
        }
    }

    /// <summary>
    /// If lhs is empty it returns the items from factory otherwise lhs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static IEnumerable<T> IfEmpty<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] Func<IEnumerable<T>> factory)
    {
        factory.ThrowIfNull();

        var lit = lhs.GetEnumerator();
        if (!lit.MoveNext())
        {
            foreach (var r in factory())
            {
                yield return r;
            }
            yield break;
        }

        yield return lit.Current;

        while (lit.MoveNext())
        {
            yield return lit.Current;
        }
    }

    /// <summary>
    /// Returns a list of indices of found items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<int> IndicesOf<T>([DisallowNull] this IEnumerable<T> items, T? item)
    {
        bool equals(T? x)
        {
            if (item is null) return x is null;
            if (x is null) return false;

            return item.Equals(x);
        }
        
        var i = 0;
        foreach (var itm in items)
        {
            if (equals(itm)) yield return i;

            i++;
        }
    }

    /// <summary>
    /// Returns a list of indices of found items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate">If true the index of the item will be returned.</param>
    /// <returns></returns>
    public static IEnumerable<int> IndicesOf<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        var i = 0;
        foreach (var item in items)
        {
            if (predicate(item))
                yield return i;

            i++;
        }
    }

    /// <summary>
    /// Returns the index of the first found item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static int IndexOf<T>(this IEnumerable<T> items, T? item)
    {
        bool equals(T? x)
        {
            if (item is null) return x is null;
            if (x is null) return false;

            return item.Equals(x);
        }

        var index = 0;
        foreach (var itm in items)
        {
            if (equals(itm)) return index;

            index++;
        }
        return -1;
    }

    public static int IndexOf<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        var i = 0;
        foreach (var item in items)
        {
            if (predicate(item)) return i;

            i++;
        }
        return -1;
    }

    /// <summary>
    /// returns a list of tuples including the item and its ordinal index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<(T item, int index)> IndexTuplesOf<T>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        var i = 0;
        foreach (var elem in items)
        {
            if (predicate(elem)) yield return (item: elem, index: i);

            i++;
        }
    }

    /// <summary>
    /// Items for which the predicate applies are not returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> Ignore<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        foreach (var item in items)
        {
            if (predicate(item)) continue;

            yield return item;
        }
    }

    /// <summary>
    /// Items at indicies are not returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="indices"></param>
    /// <returns></returns>
    public static IEnumerable<T> Ignore<T>([DisallowNull] this IEnumerable<T> items, params int[] indices)
    {
        if (0 == indices.Length) yield break;

        var i = 0;
        foreach (var item in items)
        {
            if (!indices.Contains(i))
                yield return item;

            i++;
        }
    }

    /// <summary>
    /// Ignores items when predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate">First parameter is the index. Second parameter is an item.</param>
    /// <returns></returns>
    public static IEnumerable<T> Ignore<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<int, T, bool> predicate)
    {
        predicate.ThrowIfNull();

        var i = 0;
        foreach (var item in items)
        {
            if (!predicate(i, item))
                yield return item;

            i++;
        }
    }

    /// <summary>
    /// inserts an item before the equal item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> Insert<T>(
        [DisallowNull] this IEnumerable<T> items,
        T item,
        [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();
        items.Prepend(item);
        var inserted = false;
        foreach (var i in items.ThrowIfNull())
        {
            if (!inserted && predicate(i))
            {
                yield return item;
                inserted = true;
            }
            yield return i;
        }

        if (!inserted) yield return item;
    }

    /// <summary>
    /// inserts an item before the equal item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<T> Insert<T>(
        [DisallowNull] this IEnumerable<T> items,
        T item,
        [DisallowNull] IComparer<T> comparer)
    {
        comparer.ThrowIfNull();

        var inserted = false;
        foreach (var i in items.ThrowIfNull())
        {
            if (!inserted && comparer.Compare(item, i) < 1)
            {
                yield return item;
                inserted = true;
            }
            yield return i;
        }

        if (!inserted) yield return item;
    }

    /// <summary>
    /// Intersects all collections.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collections"></param>
    /// <returns>Only items that are in all collections.</returns>
    public static IEnumerable<T> Intersect<T>([DisallowNull] this IEnumerable<IEnumerable<T>> collections)
    {
        var itCollections = collections.GetEnumerator();
        if (!itCollections.MoveNext()) yield break;

        var intersected = itCollections.Current;
        while (itCollections.MoveNext())
        {
            intersected = intersected.Intersect(itCollections.Current);

            if (!intersected.Any()) break;
        }

        foreach (var item in intersected)
        {
            yield return item;
        }
    }

    /// <summary>
    /// return true if all items are in an ascending order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="compare"></param>
    /// <returns></returns>
    public static bool IsInAscendingOrder<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, T, CompareResult> compare)
    {
        items.ThrowIfNull();
        compare.ThrowIfNull();

        var it = items.GetEnumerator();

        if (!it.MoveNext()) return true;

        var prev = it.Current;
        while (it.MoveNext())
        {
            if (CompareResult.Greater == compare(prev, it.Current))
                return false;

            prev = it.Current;
        }

        return true;
    }

    /// <summary>
    /// Returns true, if all elements of items appear in the other list, the number of items and the occurrence are same.
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <typeparam name="T"></typeparam>
    public static bool IsEqualTo<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        if (null == lhs) return null == rhs;
        if (null == rhs) return false;

        var lhsArray = lhs.ToArray();
        var rhsArray = rhs.ToArray();

        if (lhsArray.Length != rhsArray.Length) return false;

        var tuples = lhsArray.MatchWithOccurrencies(rhsArray);

        foreach(var (left, right) in tuples)
        {
            if(left.count != right.count) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns true if items is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? items)
    {
        if (items == null) return true;
        return !items.Any();
    }

    /// <summary>
    /// Returns true, if all elements of lhs appear in same order and number as in rhs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool IsSameAs<T>([DisallowNull] this IEnumerable<T> lhs, [DisallowNull] IEnumerable<T> rhs)
    {
        rhs.ThrowIfNull();

        var itLhs = lhs.GetEnumerator();
        var itRhs = rhs.GetEnumerator();
        while (itLhs.MoveNext())
        {
            if (!itRhs.MoveNext()) return false;

            if (!itLhs.Current.EqualsNullable(itRhs.Current)) return false;
        }

        return !itRhs.MoveNext();
    }

    /// <summary>
    /// Checks if second is a subset of first.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool IsSubsetOf<T>([DisallowNull] this IEnumerable<T> first, [DisallowNull] IEnumerable<T> second)
    {
        second.ThrowIfNull();

        var search = new HashSet<T>(first);
        return search.IsSubsetOf(second);

    }

    /// <summary>
    /// Iterates to all items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    public static void Iterate<T>([DisallowNull] this IEnumerable<T> items)
    {
        var enumerator = items.ThrowIfNull()
                              .GetEnumerator();

        while (enumerator.MoveNext())
        {
        }
    }

    /// <summary>
    /// Returns a list of k-combinations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> KCombinations<T>([DisallowNull] this IEnumerable<T> items, int length)
        where T : IComparable
    {
        items.ThrowIfNull();

        if (length == 1) return items.Select(t => new T[] { t });

        return KCombinations(items, length - 1)
                    .SelectMany(t => items.Where(o => o.CompareTo(t.Last()) > 0),
                               (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    /// <summary>
    /// Returns a list of k-combinations with repetitions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> KCombinationsWithRepetition<T>([DisallowNull] this IEnumerable<T> items, int length) 
        where T : IComparable
    {
        items.ThrowIfNull();

        if (length == 1) return items.Select(t => new T[] { t });

        return KCombinationsWithRepetition(items, length - 1)
                    .SelectMany(t => items.Where(o => o.CompareTo(t.Last()) >= 0),
                               (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    /// <summary>
    /// Returns the last item of source if not empty.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Opt<TSource> LastAsOpt<TSource>([DisallowNull] this IEnumerable<TSource> source)
    {
        var last = source.ThrowIfNull()
                         .LastOrDefault();

        return last is TSource src ? Opt.Some(src) : Opt.None<TSource>();
    }

    /// <summary>
    /// Returns all matching items of two lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">Key that is used for matching.</typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <param name="keySelector"></param>
    /// <param name="unique"></param>
    /// <returns></returns>
    public static (IEnumerable<T> lhs, IEnumerable<T> rhs) Match<T, TKey>(
        [DisallowNull] this IEnumerable<T> lhs,
        [DisallowNull] IEnumerable<T> rhs,
        [DisallowNull] Func<T, TKey> keySelector)
        where TKey : notnull
    {
        lhs.ThrowIfNull();
        rhs.ThrowIfNull();
        keySelector.ThrowIfNull();

        var lhsMap = new MultiMap<TKey, (T, int)>();
        var rhsMap = new MultiMap<TKey, (T, int)>();
        var lhsTuples = lhs.Enumerate();
        var rhsTuples = rhs.Enumerate();
        var tuples = from left in lhsTuples
                     join right in rhsTuples on keySelector(left.item) equals keySelector(right.item)
                     select (left, right);

        foreach (var (left, right) in tuples)
        {
            lhsMap.AddUnique(keySelector(left.item), left);
            rhsMap.AddUnique(keySelector(right.item), right);
        }

        return (lhsMap.Values.Select(t => t.Item1), rhsMap.Values.Select(t => t.Item1));
    }

    /// <summary>
    /// Returns all matching items between two lists. The items are returned as tuples with their occurrencies.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns>matching items with their occurrencies.</returns>
    public static IEnumerable<((T? item, int count) lhs, (T? item, int count) rhs)> MatchWithOccurrencies<T>(
        [DisallowNull] this IEnumerable<T?> lhs,
        [DisallowNull] IEnumerable<T?> rhs)
    {
        lhs.ThrowIfNull();
        rhs.ThrowIfNull();

        var enumeratedLhs = lhs.Select(l => NullableKey.New(l))
                               .Enumerate();

        var enumeratedRhs = rhs.Select(r => NullableKey.New(r))
                               .Enumerate();

        var tuples = enumeratedLhs
                    .Join(enumeratedRhs,
                     left => left.item,
                     right => right.item,
                     (left, right) => (left, right)).ToArray();

        
        var lhsMap = new MultiMap<NullableKey<T>, (T?, int)>();
        var rhsMap = new MultiMap<NullableKey<T>, (T?, int)>();

        foreach (var (left, right) in tuples)
        {
            lhsMap.AddUnique(left.item, (left.item.Value, left.counter));
            rhsMap.AddUnique(right.item, (right.item.Value, right.counter));
        }

        foreach (var pair in lhsMap)
        {
            var lhsTuples = lhsMap.GetValues(pair.Key);
            var rhsTuples = rhsMap.GetValues(pair.Key);

            var leftTuple = (pair.Key.Value, lhsTuples.Count());
            var rightTuple = (pair.Key.Value, rhsTuples.Count());

            yield return (leftTuple, rightTuple);
        }
    }

    /// <summary>
    /// Returns matching items of two lists with their occurrencies.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<((T item, int count) lhs, (T item, int count) rhs)> MatchOccurrencyTuples<T, TKey>(
        [DisallowNull] this IEnumerable<T> lhs,
        [DisallowNull] IEnumerable<T> rhs,
        [DisallowNull] Func<T, TKey> keySelector)
        where TKey : notnull
    {
        lhs.ThrowIfNull();
        rhs.ThrowIfNull();
        keySelector.ThrowIfNull();

        var lhsMap = new MultiMap<TKey, (T, int)>();
        var rhsMap = new MultiMap<TKey, (T, int)>();

        var lhsTuples = lhs.Enumerate();
        var rhsTuples = rhs.Enumerate();

        var tuples = from left in lhsTuples
                     join right in rhsTuples on keySelector(left.item) equals keySelector(right.item)
                     select (left, right);

        foreach (var (left, right) in tuples)
        {
            var key = keySelector(left.item);

            lhsMap.AddUnique(key, left);
            rhsMap.AddUnique(key, right);
        }

        (T item, int occurency) getTuple(IEnumerable<(T item, int counter)> tuples)
        {
            var item = default(T);
            var occurency = 0;
            foreach(var tuple in tuples.OnFirst(x => item = x.item))
            {
                occurency++;
            }

            return (item!, occurency);
        }

        foreach(var key in lhsMap.GetKeys())
        {
            var lhsValues = lhsMap.GetValues(key);
            var rhsValues = rhsMap.GetValues(key);

            yield return (getTuple(lhsValues), getTuple(rhsValues));
        }
    }

    /// <summary>
    /// Returns the greatest item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="comparer">Returns the value to compare.</param>
    /// <returns></returns>
    public static T MaxBy<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, T, int> comparer)
    {
        items.ThrowIfNull();
        comparer.ThrowIfNull();

        return items.Aggregate((a, b) => comparer(a, b) == 1 ? a : b);
    }

    /// <summary>
    /// Returns the min item selected by the selector.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSelector"></typeparam>
    /// <param name="items"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static T? MinBy<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, T, int> comparer)
    {
        items.ThrowIfNull();
        comparer.ThrowIfNull();

        T? min = default;

        foreach (var item in items.OnFirstTakeOne(x => min = x))
        {
            if (-1 == comparer(item, min!))
                min = item;
        }
        return min;
    }

    /// <summary>
    /// Returns the min and max value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static MinMax<T>? MinMax<T>([DisallowNull] this IEnumerable<T> items)
        where T : IComparable<T>
    {
        items.ThrowIfNull();
        T? min = default;
        T? max = default;

        foreach (var item in items.OnFirstTakeOne(i =>
        {
            min = i;
            max = i;
        }))
        {
            if (-1 == item.CompareTo(min))
            {
                min = item;
                continue;
            }

            if (1 == item.CompareTo(max))
                max = item;
        }

        if (null == min || null == max) return null;

        return new MinMax<T>(min, max);

    }

    /// <summary>
    /// Returns the min and max item selected by the selector.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSelector"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static MinMax<T>? MinMax<T, TSelector>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, TSelector> selector)
        where T : notnull
        where TSelector : IComparable
    {
        KeyValuePair<TSelector, T> min = default;
        KeyValuePair<TSelector, T> max = default;

        foreach (var item in items.ThrowIfNull())
        {
            var selectorValue = selector(item);
            if (null == selectorValue) continue;

            if (min.Key is null)
            {
                min = Pair.New(selectorValue, item);
                max = min;

                continue;
            }

            if (-1 == selectorValue.CompareTo(min.Key))
            {
                min = new KeyValuePair<TSelector, T>(selectorValue, item);
                continue;
            }

            if (max.Key is null || 1 == selectorValue.CompareTo(max.Key))
                max = new KeyValuePair<TSelector, T>(selectorValue, item);
        }

        return min.Value is T minValue && max.Value is T maxValue ? new MinMax<T>(minValue, maxValue) : null;
    }

    /// <summary>
    /// returns the elements that occure most frequently.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="items"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static (IEnumerable<T> items, int count) MostFrequent<T, TKey>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, TKey> keySelector)
    {
        var grouped = items.ThrowIfNull()
                           .GroupBy(keySelector);

        var mostFrequent = new List<IGrouping<TKey, T>>();
        var itemCount = 0;

        foreach (var grp in grouped.OnFirst(g =>
        {
            mostFrequent.Add(g);
            itemCount = mostFrequent.Count;
        }).Skip(1))
        {
            var count = grp.Count();
            if (itemCount > count) continue;

            if (count > itemCount) mostFrequent.Clear();

            mostFrequent.Add(grp);
            itemCount = count;
        }

        if (null == mostFrequent) return (Enumerable.Empty<T>(), 0);

        return (mostFrequent.Select(g => g.First()), itemCount);
    }

    /// <summary>
    /// gets the item on a certain index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="index"></param>
    /// <returns>An optional.</returns>
    public static Opt<T> Nth<T>([DisallowNull] this IEnumerable<T> items, int index)
    {
        if (0 > index) return Opt.None<T>();

        var pos = 0;
        foreach (var item in items.ThrowIfNull())
        {
            if (pos > index) break;

            if (index == pos)
                return Opt.Some(item);

            pos++;
        }
        return Opt.None<T>();
    }


    /// <summary>
    /// Returns all items within the range of indices.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="range">range of indices. Indices from end are not supported</param>
    /// <returns>all items within the range of indices</returns>
    public static IEnumerable<T> Nths<T>([DisallowNull] this IEnumerable<T> items, System.Range range)
    {
        int i = 0;
        foreach (var item in items.ThrowIfNull())
        {
            if (range.IsInRange(i)) yield return item;

            i++;
        }
    }

    /// <summary>
    /// gets items on the certain indexes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="indexes"></param>
    /// <returns></returns>
    public static IEnumerable<T> Nths<T>([DisallowNull] this IEnumerable<T> items, params int[] indexes)
    {
        var pos = 0;
        foreach (var item in items.ThrowIfNull())
        {
            if (indexes.Contains(pos))
                yield return item;

            pos++;
        }
    }

    public static IEnumerable<TResult> OfTypes<T, TResult>([DisallowNull] this IEnumerable<T> items, params Type[] types)
    {
        var resultType = typeof(TResult);
        if (!types.All(t => resultType.IsAssignableFrom(t))) yield break;

        foreach (var item in items.ThrowIfNull())
        {
            if (null == item) continue;

            var itemType = item.GetType();
            if (!types.Any(t => t.Equals(itemType) || t.IsAssignableFrom(itemType))) continue;
            if (item is TResult result) yield return result;
        }
    }


    /// <summary>
    /// Calls action on all adjacent elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action">Contains the previous and the current item.</param>
    /// <returns></returns>
    public static IEnumerable<T> OnAdjacentElements<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action<T, T> action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) yield break;

        yield return it.Current;

        var prevItem = it.Current;
        while (it.MoveNext())
        {
            action(prevItem, it.Current);
            yield return it.Current;

            prevItem = it.Current;
        }
    }

    /// <summary>
    /// Calls action on each element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnEach<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        foreach (var item in items.ThrowIfNull())
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Calls action if items is empty;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnEmpty<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext())
        {
            action();
            yield break;
        }
        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Calls action on first item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnFirst<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) yield break;
        action();
        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Calls action on first item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnFirst<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) yield break;

        action(it.Current);
        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Calls action on first item. The IEnumerable result starts from the 2nd element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnFirstTakeOne<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) yield break;

        action(it.Current);

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Calls action on last item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnLast<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (it.MoveNext())
        {
            while (true)
            {
                yield return it.Current;
                if (!it.MoveNext())
                {
                    action();
                    yield break;
                }
            }
        }
    }

    /// <summary>
    /// Calls action on last item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnLast<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (it.MoveNext())
        {
            while (true)
            {
                var last = it.Current;
                yield return last;
                if (!it.MoveNext())
                {
                    action(last);
                    yield break;
                }
            }
        }
    }

    /// <summary>
    /// Calls action when reached an index during iteration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="index"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> OnNth<T>([DisallowNull] this IEnumerable<T> items, int index, [DisallowNull] Action<T> action)
    {
        action.ThrowIfNull();

        var counter = 0;
        foreach (var item in items.ThrowIfNull())
        {
            if (index == counter)
                action(item);

            yield return item;
            counter++;
        }
    }

    /// <summary>
    /// Partitions items into two lists. If predicate is true the item is added to matching otherwise to notMatching.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate">Discriminator of the two lists.</param>
    /// <returns></returns>
    public static (IEnumerable<T> matching, IEnumerable<T> notMatching) Partition<T>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        (IEnumerable<T> matching, IEnumerable<T> notMatching) = (Enumerable.Empty<T>(), Enumerable.Empty<T>());
        foreach (var item in items.ThrowIfNull())
        {
            if (predicate(item))
            {
                matching = matching.Append(item);
                continue;
            }
            notMatching = notMatching.Append(item);
        }

        return (matching, notMatching);
    }

    /// <summary>
    /// Partitions items into two lists. If predicate is true the item is added to matching otherwise notMatching.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <param name="match">projection to TResult.</param>
    /// <param name="noMatch">projection to TResult.</param>
    /// <returns></returns>
    public static (IEnumerable<TResult> matching, IEnumerable<TResult> notMatching) Partition<T, TResult>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, bool> predicate,
        [DisallowNull] Func<T, TResult> match,
        [DisallowNull] Func<T, TResult> noMatch)
    {
        predicate.ThrowIfNull();
        match.ThrowIfNull();
        noMatch.ThrowIfNull();

        (IEnumerable<TResult> matching, IEnumerable<TResult> notMatching) = (Enumerable.Empty<TResult>(), Enumerable.Empty<TResult>());
        foreach (var item in items.ThrowIfNull())
        {
            if (predicate(item))
            {
                matching = matching.Append(match(item));
                continue;
            }
            notMatching = notMatching.Append(noMatch(item));
        }

        return (matching, notMatching);
    }

    /// <summary>
    /// Creates permutations of a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">items must be sorted</param>
    /// <param name="length">This is the permutation size.</param>
    /// <param name="containsRepetitions">If true, it contains repetitions.</param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Permutations<T>(
        [DisallowNull] this IEnumerable<T> items,
        int length,
        bool containsRepetitions = true)
    {
        items.ThrowIfNull();

        if (length == 1) return items.Select(t => new T[] { t });

        if (containsRepetitions)
        {
            return Permutations(items, length - 1, containsRepetitions)
                        .SelectMany(t => items,
                                   (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        return Permutations(items, length - 1)
                    .SelectMany(t => items.Where(o => !t.Contains(o)),
                               (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    /// <summary>
    /// Inserts an item at the beginning only if the list is not empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> PrependIfNotEmpty<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T> factory)
    {
        factory.ThrowIfNull();

        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) yield break;

        yield return factory();
        yield return it.Current;

        while (it.MoveNext()) yield return it.Current;
    }

    /// <summary>
    /// Returns a random subset of elems.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elems"></param>
    /// <param name="numberOfSubSetElements">the size of the subset.</param>
    /// <param name="random">random number generator. If it is null a number generator is created.</param>
    /// <returns></returns>
    public static IEnumerable<T> RandomSubset<T>(
        [DisallowNull] this IEnumerable<T> elems,
        int numberOfSubSetElements,
        Random? random = null)
    {
        elems.ThrowIfNull();

        if (0 >= numberOfSubSetElements) return Enumerable.Empty<T>();

        if (null == random) random = new Random();

        var itemCount = elems.Count();
        if (numberOfSubSetElements > itemCount) 
            numberOfSubSetElements = itemCount;

        var indexes = Enumerable.Range(0, itemCount).ToList();
        var shuffledIndexes = indexes.Shuffle(random);

        return elems.Nths(shuffledIndexes
                    .Take(numberOfSubSetElements)
                    .ToArray());
    }

    /// <summary>
    /// Removes the last element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> RemoveTail<T>([DisallowNull] this IEnumerable<T> items)
    {
        var it = items.ThrowIfNull()
                      .GetEnumerator();

        while (true)
        {
            if (!it.MoveNext()) yield break;
            var prev = it.Current;

            if (!it.MoveNext()) yield break;

            yield return prev;
            yield return it.Current;
        }
    }

    /// <summary>
    /// Replaces the items from a list at specified indexes with the specified values of replaceTuples.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="replaceTuples"></param>
    /// <returns></returns>
    public static IEnumerable<T> Replace<T>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] IEnumerable<(T item, int index)> replaceTuples)
    {
        items.ThrowIfNull();
        replaceTuples.ThrowIfNull();

        return Replace(items, replaceTuples, item => item);
    }

    /// <summary>
    /// Replaces the items from a list with the values from project.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="project">Gets each item with index.</param>
    /// <returns></returns>
    public static IEnumerable<TResult> Replace<T, TResult>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, int, TResult> project)
    {
        items.ThrowIfNull();
        project.ThrowIfNull();

        foreach (var (item, counter) in items.Enumerate())
        {
            yield return project(item, counter);
        }
    }

    /// <summary>
    /// Replaces the items from a list at specified indexes with the specified values of replaceTuples.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="replaceTuples">A list of tuples which include the replacement items and indexes.</param>
    /// <param name="project">Projects each item to TResult.</param>
    /// <returns>A list of TResult items.</returns>
    public static IEnumerable<TResult> Replace<T, TResult>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] IEnumerable<(T item, int index)> replaceTuples,
        [DisallowNull] Func<T, TResult> project)
    {
        items.ThrowIfNull();
        replaceTuples.ThrowIfNull();
        project.ThrowIfNull();

        var orderedTuples = replaceTuples.Where(tuple => 0 <= tuple.index)
                                         .OrderBy(tuple => tuple.index)
                                         .ToArray();

        if (0 == orderedTuples.Length) yield break;

        var tupleIndex = 0;
        var tupleIndexMax = orderedTuples.Length - 1;
        var replaceIndexMax = orderedTuples[tupleIndexMax].index;

        var (replaceItem, replaceIndex) = orderedTuples[tupleIndex];

        foreach (var (item, i) in items.Enumerate())
        {
            if (i < replaceIndexMax && tupleIndex < tupleIndexMax)
            {
                if (i > replaceIndex) ++tupleIndex;

                if (i > replaceIndex)
                    (replaceItem, replaceIndex) = orderedTuples[tupleIndex];
            }

            if (i == replaceIndex) yield return project(replaceItem);
            else yield return project(item);
        }
    }

    /// <summary>
    /// Returns items only if all projections do have a value (are Opt.Some).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="project"></param>
    /// <returns>Returns an empty list if not all projections are Opt.Some.</returns>
    public static IEnumerable<TResult> SelectAll<T, TResult>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, Opt<TResult>> project)
    {
        project.ThrowIfNull();

        var results = Enumerable.Empty<TResult>();
        var notAllItems = false;

        foreach (var item in items.ThrowIfNull())
        {
            var opt = project(item);
            if (opt.IsNone)
            {
                notAllItems = true;
                break;
            }
            results = results.Append(opt.ValueOrThrow());
        }

        return notAllItems ? Enumerable.Empty<TResult>() : results;
    }

    /// <summary>
    /// Returns only the Error values.
    /// </summary>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<TError> SelectError<TOk, TError>([DisallowNull] this IEnumerable<Result<TOk, TError>> items)
    {
        return items.ThrowIfNull()
                    .Where(item => item.IsError)
                    .Select(result => result.Error);
    }


    /// <summary>
    /// Filters all items that are null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> SelectNotNull<T>([DisallowNull] this IEnumerable<T?> items)
    {
        foreach (var item in items.ThrowIfNull())
        {
            if (item is T result) yield return result;
        }
    }

    /// <summary>
    /// Returns only the Ok values.
    /// </summary>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<TOk> SelectOk<TOk, TError>([DisallowNull] this IEnumerable<Result<TOk, TError>> items)
    {
        return items.ThrowIfNull()
                    .Where(item => item.IsOk)
                    .Select(result => result.Ok!);
    }

    /// <summary>
    /// Returns all Some values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> SelectSome<T>([DisallowNull] this IEnumerable<Opt<T>> items)
    {
        return items.ThrowIfNull()
                    .Where(item => item.IsSome)
                    .Select(opt => opt.ValueOrThrow());
    }

    /// <summary>
    /// Returns Some if exactly one item exists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Opt<T> SingleAsOpt<T>([DisallowNull] this IEnumerable<T> items)
    {
        var it = items.ThrowIfNull()
                      .GetEnumerator();

        if (!it.MoveNext()) return Opt.None<T>();

        var first = it.Current;

        if (it.MoveNext()) return Opt.None<T>();

        return Opt.Maybe(first);
    }

    /// <summary>
    /// Returns Some if exactly one item exists with this predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Opt<T> SingleAsOpt<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        items.ThrowIfNull();
        predicate.ThrowIfNull();

        var found = Opt.None<T>();
        foreach (var item in items)
        {
            if (predicate(item))
            {
                if (found.IsSome) return Opt.None<T>();
                found = Opt.Some(item);
            }
        }
        return found;
    }

    /// <summary>
    /// Creates a list of enumerables of size length.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="length">this is the chop size.</param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Slice<T>([DisallowNull] this IEnumerable<T> items, int length)
    {
        items.ThrowIfNull();
        var sliced = Enumerable.Empty<T>();

        var itemCounter = 0;
        foreach (var item in items)
        {
            sliced = sliced.Append(item);
            itemCounter++;
            if (itemCounter == length)
            {
                itemCounter = 0;
                yield return sliced;

                sliced = Enumerable.Empty<T>();
            }
        }

        if (0 < itemCounter) yield return sliced;
    }

    /// <summary>
    /// Creates a list of enumerables. For each predicate an enumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicates"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Slice<T>(
        [DisallowNull] this IEnumerable<T> items,
        params Func<T, bool>[] predicates)
    {
        items.ThrowIfNull();
        if (0 == predicates.Length) return Enumerable.Empty<IEnumerable<T>>();

        var splittedItems = new Dictionary<int, IEnumerable<T>>();

        for (var i = 0; i < predicates.Length; i++)
            splittedItems.Add(i, Enumerable.Empty<T>());

        foreach (var item in items)
        {
            for (var i = 0; i < predicates.Length; i++)
                if (predicates[i](item))
                    splittedItems[i] = splittedItems[i].Append(item);
        }

        return splittedItems.Select(x => x.Value);
    }

    /// <summary>
    /// Returns the object with the smallest selector value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector">Returns the value to compare.</param>
    /// <returns></returns>
    public static T Smallest<T>([DisallowNull] this IEnumerable<T> items, Func<T, IComparable> selector)
    {
        selector.ThrowIfNull();

        return items.Aggregate((a, b) => selector(a).CompareTo(selector(b)) == -1 ? a : b);
    }

    /// <summary>
    /// Returns the object with the smallest selector value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSelector"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector">Returns the value to compare.</param>
    /// <returns></returns>
    public static T Smallest<T, TSelector>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, TSelector> selector)
        where TSelector : IComparable<TSelector>
    {
        selector.ThrowIfNull();

        return items.Aggregate((a, b) => selector(a).CompareTo(selector(b)) == -1 ? a : b);
    }

    /// <summary>
    /// This method implements the Fisher-Yates Shuffle.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public static IEnumerable<T> Shuffle<T>([DisallowNull] this IEnumerable<T> items, Random? random = null)
    {
        if (null == random) random = new Random();

        return items.ThrowIfEmpty()
                    .ToArray()
                    .Shuffle(random);
    }

    /// <summary>
    /// Throw an ArgumentNullException when an element is null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [return: NotNull]
    public static IEnumerable<T> ThrowIfElementNull<T>([DisallowNull] this IEnumerable<T?> items)
    {
        foreach(var item in items.ThrowIfNull())
        {
            yield return item.ThrowIfNull(); ;
        }
    }

    /// <summary>
    /// Throws an ArgumentNullException if items is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEmpty<T>([DisallowNull] this IEnumerable<T> items, [CallerArgumentExpression("items")] string name = "")
    {
        return ThrowIfEmpty(items, () => new ArgumentNullException(name));
    }

    /// <summary>
    /// Throws an Exception if items is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="exceptionFactory"></param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEmpty<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<Exception> exceptionFactory)
    {
        exceptionFactory.ThrowIfNull();

        if (!items.Any())
        {
            var exception = exceptionFactory() ?? throw new ArgumentException("factory returned null");
            throw exception;
        }
        return items;
    }

    /// <summary>
    /// Makes the enumerable interruptible. This can be used for nested foreach loops.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public static IEnumerable<T> ToBreakable<T>([DisallowNull] this IEnumerable<T> items, ref ObservableValue<bool> stop)
    {
        return new BreakableEnumerable<T>(items.ThrowIfNull(), ref stop);
    }

    /// <summary>
    /// Splits a stream into two streams and returns it as a DualOrdinalStreams. Matching items are added to the right stream.
    /// If isExhaustive is false, all items are added to the left stream.
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate">This is the split criteria.</param>
    /// <param name="project">A projection from TLeft to TRight.</param>
    /// <param name="isExhaustive">If true then matching items are not added to the left stream.</param>
    /// <returns></returns>
    public static DualOrdinalStreams<TLeft, TRight> ToDualOrdinalStreams<TLeft, TRight>(
        [DisallowNull] this IEnumerable<TLeft> items,
        [DisallowNull] Func<TLeft, bool> predicate,
        [DisallowNull] Func<TLeft, TRight> project,
        bool isExhaustive)
    {
        items.ThrowIfNull();
        predicate.ThrowIfNull();
        project.ThrowIfNull();

        var tuple = new DualOrdinalStreams<TLeft, TRight>();

        foreach (var (item, counter) in items.Enumerate())
        {
            if (predicate(item))
            {
                var ordinalRight = new Ordinal<TRight> { Position = counter, Value = project(item) };

                tuple.Right = tuple.Right.Append(ordinalRight);

                if (isExhaustive) continue;
            }

            var ordinalLeft = new Ordinal<TLeft> { Position = counter, Value = item };
            tuple.Left = tuple.Left.Append(ordinalLeft);
        }

        return tuple;
    }

    /// <summary>
    /// Returns an empty enumerable if items is null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> ToEmptyIfNull<T>([DisallowNull] this IEnumerable<T>? items)
    {
        return items ?? Enumerable.Empty<T>();
    }

    public static HashSet<T> ToHashSet<T>([DisallowNull] this IEnumerable<T> items)
    {
        return new HashSet<T>(items.ThrowIfNull());
    }

    public static IMultiMap<TKey, T> ToMap<T, TKey>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, TKey> keySelector)
        where TKey : notnull
    {
        items.ThrowIfNull();
        keySelector.ThrowIfNull();

        return ToMultiMap(items, keySelector, x => x);
    }

    public static IMultiMap<TKey, TValue> ToMultiMap<T, TKey, TValue>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, TKey> keySelector, 
        [DisallowNull] Func<T, TValue> valueSelector)
        where TKey : notnull
    {
        items.ThrowIfNull();
        keySelector.ThrowIfNull();

        var dictionary = new MultiMap<TKey, TValue>();
        foreach (var item in items)
            dictionary.Add(keySelector(item), valueSelector(item));

        return dictionary;
    }

    public static IEnumerable<KeyValuePair<TLhs, IEnumerable<TRhs>>> ToOne2Many<TSource, TLhs, TRhs>(
        [DisallowNull] this IEnumerable<TSource> source,
        [DisallowNull] Func<TSource, TLhs> lhsSelector,
        [DisallowNull] Func<TSource, TRhs> rhsSelector)
        where TLhs : notnull
    {
        source.ThrowIfNull();
        lhsSelector.ThrowIfNull();
        rhsSelector.ThrowIfNull();

        var one2Many = new MultiMap<TLhs, TRhs>();
        foreach (var sourceElem in source)
        {
            var lhsElem = lhsSelector(sourceElem);
            var rhsElem = rhsSelector(sourceElem);
            if (!one2Many.Contains(lhsElem, rhsElem))
                one2Many.Add(lhsElem, rhsElem);
        }

        return one2Many.GetKeyValues();
    }

    /// <summary>
    /// Normalizes a one to many collection to one to one.
    /// 
    /// A -> [1, 2, 3]
    /// B -> [4, 5]
    /// 
    /// creates tuples:
    /// 
    /// (A,1), (A,2), (A,3), (B,4), (B5)
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TLhs"></typeparam>
    /// <typeparam name="TRhs"></typeparam>
    /// <param name="source"></param>
    /// <param name="lhsSelector"></param>
    /// <param name="rhsSelector"></param>
    /// <returns></returns>
    public static IEnumerable<(TLhs, TRhs)> ToOne2One<TSource, TLhs, TRhs>(
        [DisallowNull] this IEnumerable<TSource> source,
        [DisallowNull] Func<TSource, TLhs> lhsSelector,
        [DisallowNull] Func<TSource, IEnumerable<TRhs>> rhsSelector)
    {
        source.ThrowIfNull();
        lhsSelector.ThrowIfNull();
        rhsSelector.ThrowIfNull();

        foreach (var sourceElem in source)
        {
            var lhsElem = lhsSelector(sourceElem);
            foreach (var rhsElem in rhsSelector(sourceElem))
            {
                yield return (lhsElem, rhsElem);
            }
        }
    }

    /// <summary>
    /// Creates optional items from items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public static IEnumerable<Opt<TResult>> ToOptionals<T, TResult>(
        [DisallowNull] this IEnumerable<T> items,
        [DisallowNull] Func<T, Opt<TResult>> project)
    {
        items.ThrowIfNull();
        project.ThrowIfNull();

        return items.Select(project);
    }

    public static IEnumerable<Ordinal<T>> ToOrdinals<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        items.ThrowIfNull();
        predicate.ThrowIfNull();

        return items.Enumerate()
                    .Where(tuple => predicate(tuple.item))
                    .Select(tuple => new Ordinal<T> { Position = tuple.counter, Value = tuple.item });
    }

    /// <summary>
    /// creates a string from the items separated by separator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string ToReadableString<T>([DisallowNull] this IEnumerable<T> items, string separator = ",")
    {
        items.ThrowIfNull();
        var sb = new StringBuilder();
        foreach (var item in items.AfterEveryElement(() => sb.Append(separator)))
        {
            sb.Append(item);
        }
        return sb.ToString();
    }

    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>([DisallowNull] this IEnumerable<T> items)
    {
        items.ThrowIfNull();
        return new ReadOnlyCollection<T>(items);
    }

    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>([DisallowNull] this IEnumerable<T> items, int count)
    {
        items.ThrowIfNull();
        return new ReadOnlyCollection<T>(items, count);
    }

    public static string[] ToStringArray<T>([DisallowNull] this IEnumerable<T> enumerable)
    {
        return enumerable.ThrowIfNull()
                         .Select(item => item.ToStringOrEmpty())
                         .ToArray();
    }

    /// <summary>
    /// Returns true if all predicates are true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="elems"></param>
    /// <param name="predicates"></param>
    /// <returns></returns>
    public static bool TryWhere<T>([DisallowNull] this IEnumerable<T> items, out IEnumerable<T> elems, params Func<T, bool>[] predicates)
    {
        items.ThrowIfNull();
        elems = Enumerable.Empty<T>();

        var allFulfilled = new bool[predicates.Length];
        foreach (var item in items)
        {
            var index = 0;

            var fulfilled = false;
            foreach (var predicate in predicates)
            {
                if (predicate(item))
                {
                    allFulfilled[index] = true;
                    fulfilled = true;
                }

                index++;
            }

            if (fulfilled)
                elems.Append(item);
        }

        return allFulfilled.All(x => x == true);
    }

    /// <summary>
    /// Returns true if all elements fulfilling the predicate. Stops immediately if predicate returns false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <param name="elems"></param>
    /// <returns></returns>
    public static bool TryWhereAll<T>(
        [DisallowNull] this IEnumerable<T> items, 
        [DisallowNull] Func<T, bool> predicate,
        out IEnumerable<T> elems)
    {
        items.ThrowIfNull();
        predicate.ThrowIfNull();

        elems = Enumerable.Empty<T>();

        foreach (var item in items)
        {
            if (!predicate(item))
            {
                return false;
            }

            elems.Append(item);
        }

        return true;
    }

    /// <summary>
    /// Checks if all items fulfill the predicate. If there is an item not matching the predicate an exception is thrown.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereAll<T>([DisallowNull] this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull();

        foreach (var item in items.ThrowIfNull())
        {
            if (!predicate(item))
                throw new ArgumentOutOfRangeException($"{item}");

            yield return item;
        }
    }

    /// <summary>
    /// Returns all elements which are not null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereNotNull<T>([DisallowNull] this IEnumerable<T> items) => items.Where(item => null != item);

    /// <summary>
    /// Returns all optional values that are some.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereSome<T>([DisallowNull] this IEnumerable<Opt<T>> items)
    {
        return items.Where(item => item.IsSome).Select(opt => opt.ValueOrThrow());
    }

    /// <summary>
    /// Works like Zip with comparer. Maps only maching items.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="comparer"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> Zip<T1, T2, TResult>(
        [DisallowNull] this IEnumerable<T1> first,
        [DisallowNull] IEnumerable<T2> second,
        [DisallowNull] Func<T1, T2, bool> comparer,
        [DisallowNull] Func<T1, T2, TResult> resultSelector)
    {
        second.ThrowIfNull();
        comparer.ThrowIfNull();
        resultSelector.ThrowIfNull();

        return from firstItem in first
               from secondItem in second
               where comparer(firstItem, secondItem)
               select resultSelector(firstItem, secondItem);
    }
}

public interface IElseIf<T>
{
    IEnumerable<T> Else();
    IEnumerable<T> Else([DisallowNull] Action<T> action);
    IElseIf<T> ElseIf([DisallowNull] Func<T, bool> condition, [DisallowNull] Action<T> action);
    void EndIf();
}

public interface IElse<T, TResult>
{
    IEnumerable<TResult> Else([DisallowNull] Func<T, TResult> map);
}

