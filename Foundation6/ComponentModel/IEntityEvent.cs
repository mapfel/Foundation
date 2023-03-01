﻿namespace Foundation.ComponentModel;

public interface IEntityEvent<TEventId, TEntityId> : IEvent<TEventId>
    where TEntityId : notnull
{
    TEntityId EntityId { get; }
}

public interface IEntityEvent<TEventId, TEntityId, TObjectType>
    : IEntityEvent<TEventId, TEntityId>
    , ITypedObject<TObjectType>
    where TEntityId : notnull
{
}