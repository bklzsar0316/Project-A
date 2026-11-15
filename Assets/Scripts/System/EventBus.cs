using System;
using System.Collections.Generic;

public static class EventBus 
{

    private static readonly Dictionary<Type,Delegate> Handlers= new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> onEvent) where T : struct
    {
        Type type = typeof(T);
        if (Handlers.TryGetValue(type, out Delegate existingHandler))
        {
            Handlers[type] = Delegate.Combine(existingHandler, onEvent);
        }
        else
        {
            Handlers[type] = onEvent;
        }
    }

    public static void UnSubscribe<T>(Action<T> onEvent) where T : struct
    {
        Type type = typeof(T);
        if (Handlers.TryGetValue(type, out Delegate existingHandler))
        {
            Delegate newHandler = Delegate.Remove(existingHandler, onEvent);
            if (newHandler == null)
            {
                Handlers.Remove(type);
            }
            else
            {
                Handlers[type] = newHandler;
            }
        }
    }

    public static void Publish<T>(T @EventData) where T : struct
    {
        Type type = typeof(T);
        if (Handlers.TryGetValue(type, out Delegate handler))
        {
            (handler as Action<T>)?.Invoke(EventData);   
        }
    }
}