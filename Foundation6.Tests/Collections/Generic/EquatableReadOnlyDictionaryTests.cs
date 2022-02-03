﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Collections.Generic
{
    [TestFixture]
    public class EquatableReadOnlyDictionaryTests
    {
        [Test]
        public void Equals_Should_Return_False_When_KeysAreDifferent()
        {
            var keyValues1 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            var keyValues2 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "four", 3 }
            };

            var sut1 = new EquatableReadOnlyDictionary<string, object>(keyValues1);
            var sut2 = new EquatableReadOnlyDictionary<string, object>(keyValues2);

            Assert.AreNotEqual(sut1, sut2);
            Assert.AreNotEqual(sut1.GetHashCode(), sut2.GetHashCode());
        }

        [Test]
        public void Equals_Should_Return_False_When_KeysAndValuesAreDifferent()
        {
            var keyValues1 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            var keyValues2 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "four", 4 }
            };

            var sut1 = new EquatableReadOnlyDictionary<string, object>(keyValues1);
            var sut2 = new EquatableReadOnlyDictionary<string, object>(keyValues2);

            Assert.AreNotEqual(sut1, sut2);
            Assert.AreNotEqual(sut1.GetHashCode(), sut2.GetHashCode());
        }

        [Test]
        public void Equals_Should_Return_False_When_ValuesAreDifferent()
        {
            var keyValues1 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            var keyValues2 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 4 }
            };

            var sut1 = new EquatableReadOnlyDictionary<string, object>(keyValues1);
            var sut2 = new EquatableReadOnlyDictionary<string, object>(keyValues2);

            Assert.AreNotEqual(sut1, sut2);
            Assert.AreNotEqual(sut1.GetHashCode(), sut2.GetHashCode());
        }

        [Test]
        public void Equals_Should_Return_True_When_KeysAndValuesAreSame()
        {
            var keyValues1 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            var keyValues2 = new Dictionary<string, object>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };

            var sut1 = new EquatableReadOnlyDictionary<string, object>(keyValues1);
            var sut2 = new EquatableReadOnlyDictionary<string, object>(keyValues2);

            Assert.AreEqual(sut1, sut2);
            Assert.AreEqual(sut1.GetHashCode(), sut2.GetHashCode());
        }
    }
}
