﻿namespace Foundation.Collections.Generic;

public class CorrelateSorted<T, TIndex>
{
    private IEnumerator<T>[]? _enumerators;
    private readonly Func<T, TIndex> _indexSelector;

    public CorrelateSorted(Func<T, TIndex> indexSelector)
    {
        _indexSelector = indexSelector.ThrowIfNull();
    }

    private bool AreAllCurrentValuesEqual()
    {
        if (null == _enumerators) return false;

        var first = _indexSelector(_enumerators[0].Current);
        return _enumerators.Skip(1).All(e => _indexSelector(e.Current).EqualsNullable(first));
    }

    private bool EnumeratorsMoveNext()
    {
        if (null == _enumerators) return false;

        return _enumerators.All(e => e.MoveNext());
    }

    private IEnumerable<T> GetCurrentValues()
    {
        if (null == _enumerators) return Enumerable.Empty<T>();

        return _enumerators.Select(e => e.Current).ToArray();
    }

    private TIndex? GetMinIndexFromCurrentValues()
    {
        return GetCurrentValues().Min(_indexSelector);
    }

    private bool MoveEnumeratorsWithMinIndex(TIndex index)
    {
        if(null == _enumerators) return false;

        foreach(var enumerator in _enumerators)
        {
            if(null == enumerator.Current) return false;

            if(!_indexSelector(enumerator.Current).EqualsNullable(index)) continue;

            if(!enumerator.MoveNext()) return false;
        }

        return true;
    }

    /// <summary>
    /// Correlates streams that are sorted and the indices per stream are unique.
    /// It returns values from each stream with the same index.
    /// </summary>
    /// <param name="streams"></param>
    /// <returns></returns>
    public IEnumerable<IEnumerable<T>> UniqueIndexStreams(IEnumerable<IEnumerable<T>> streams)
    {
        _enumerators = streams.Select(s => s.GetEnumerator()).ToArray();
        if (0 == _enumerators.Length) yield break;
        if (1 == _enumerators.Length)
        {
            yield return streams.First();
            yield break;
        }

        if (!EnumeratorsMoveNext()) yield break;

        while (true)
        {
            if (AreAllCurrentValuesEqual())
            {
                yield return GetCurrentValues();

                if (!EnumeratorsMoveNext()) yield break;
            }
            else
            {
                var minIndex = GetMinIndexFromCurrentValues();
                if (null == minIndex) yield break;

                if(!MoveEnumeratorsWithMinIndex(minIndex)) yield break;
            }
        }
    }
}

