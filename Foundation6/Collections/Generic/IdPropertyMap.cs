﻿using Foundation.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Foundation.Collections.Generic
{
    public class IdPropertyMap<TId>
        : IdPropertyMap<TId, EntityChangedEvent<TId>>
        , IIdPropertyMap<TId>
        where TId : notnull
    {
        public IdPropertyMap(
            KeyValue<string, TId> identifier,
            char pathSeparator = '/')
            : base(identifier, pathSeparator)
        {
        }

        public IdPropertyMap(
            KeyValue<string, TId> identifier,
            SortedDictionary<string, object> dictionary,
            char pathSeparator = '/')
            : base(identifier, dictionary, pathSeparator)
        {
        }

        protected override EntityChangedEvent<TId> CreateChangedEvent(string propertyName, object? value, PropertyChangedState state)
        {
            return new EntityChangedEvent<TId>(Identifier, new PropertyChangedEvent(propertyName, value, state));
        }
    }

   

    public abstract class IdPropertyMap<TObjectType, TId, TEvent>
        : IdPropertyMap<TId, TEvent>
        , IIdPropertyMap<TObjectType, TId>
        , IEquatable<IIdPropertyMap<TObjectType, TId>>
        where TId : notnull
        where TEvent: IIndexedIdentifiable<string, TId>, IPropertyChangedContainer, ITypedObject<TObjectType>
    {
        public IdPropertyMap(
            TObjectType objectType,
            KeyValue<string, TId> identifier,
            char pathSeparator = '/')
            : base(identifier, pathSeparator)
        {
            ObjectType = objectType.ThrowIfNull();
        }

        public IdPropertyMap(
            TObjectType objectType,
            KeyValue<string, TId> identifier,
            SortedDictionary<string, object> dictionary,
            char pathSeparator = '/')
            : base(identifier, dictionary, pathSeparator)
        {
            ObjectType = objectType.ThrowIfNull();
        }

        public override bool Equals(object? obj) => Equals(obj as IIdPropertyMap<TObjectType, TId>);

        public bool Equals(IIdPropertyMap<TObjectType, TId>? other)
        {
            return null != other 
                && base.Equals(other) 
                && ObjectType!.Equals(other.ObjectType);
        }

        public override int GetHashCode() => System.HashCode.Combine(ObjectType, Identifier);

        public override void HandleEvent(TEvent @event)
        {
            @event.ThrowIfNull();

            if (!ObjectType!.Equals(@event.ObjectType)) return;

            base.HandleEvent(@event);
        }

        public TObjectType ObjectType { get; }

        public override string ToString() => $"{ObjectType}, {Identifier}";
    }

    public abstract class IdPropertyMap<TId, TEvent>
        : PropertyMap<TEvent>
        , IIdPropertyMap<TId>
        where TId : notnull
        where TEvent : IIndexedIdentifiable<string, TId>, IPropertyChangedContainer
    {
        public IdPropertyMap(
            KeyValue<string, TId> identifier,
            char pathSeparator = '/')
            : this(identifier, new SortedDictionary<string, object>(), pathSeparator)
        {
        }

        public IdPropertyMap(
            KeyValue<string, TId> identifier,
            SortedDictionary<string, object> dictionary,
            char pathSeparator = '/')
            : base(dictionary, pathSeparator)
        {
            Identifier = identifier.ThrowIfEmpty();
        }

        public override object this[string key]
        {
            get
            {
                if (Identifier.Key == key)
                    return Identifier.Value;

                return base[key];
            }
            set
            {
                if (Identifier.Key == key) return;

                var exists = ContainsKey(key);
                base[key] = value;

                var state = exists ? PropertyChangedState.Replaced : PropertyChangedState.Added;

                var @event = CreateChangedEvent(key, value, state);
                AddEvent(@event);
            }
        }

        public override void HandleEvent(TEvent @event)
        {
            @event.ThrowIfNull();

            if (!Identifier.Equals(@event.Identifier)) return;

            switch (@event.PropertyChanged.ChangedState)
            {
                case PropertyChangedState.Added: Add(@event.PropertyChanged.Name, @event.PropertyChanged.Value); break;
                case PropertyChangedState.Removed: Remove(@event.PropertyChanged.Name); break;
                case PropertyChangedState.Replaced: this[@event.PropertyChanged.Name] = @event.PropertyChanged.Value!; break;
            };
        }

        public TId Id => Identifier.Value;

        public KeyValue<string, TId> Identifier { get; }

        public override string ToString() => $"{Identifier}";

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            if (Identifier.Key == key)
            {
                value = Identifier.Value;
                return true;
            }

            return base.TryGetValue(key, out value);
        }
    }
}
