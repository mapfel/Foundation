﻿namespace Foundation.Collections.Generic
{
    public static class HashChainFactory
    {
        public static IEnumerable<HashChainElement<T, THash>> Create<T, THash>(IEnumerable<T> elements, Func<T, THash> getHash)
            where T : notnull
            where THash : notnull
        {
            var prevHash = Option.None<THash>();
            foreach (var element in elements)
            {
                var hash = getHash(element);
                if (prevHash.IsNone)
                {
                    var firstElem = new HashChainElement<T, THash>(element, getHash, prevHash);
                    prevHash = Option.Some(firstElem.Hash);
                    yield return firstElem;
                    continue;
                }

                var elem = new HashChainElement<T, THash>(element, getHash, prevHash);
                yield return elem;

                prevHash = Option.Some(elem.Hash);
            }
        }
    }
}
