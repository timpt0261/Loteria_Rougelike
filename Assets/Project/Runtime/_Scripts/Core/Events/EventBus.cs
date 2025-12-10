using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EventBus
{

	private static readonly Dictionary<Type, List<Delegate>> subscribers = new();


	public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
	{

		Type t = typeof(T);
		if (!subscribers.TryGetValue(t, out List<Delegate> list))
		{
			list = new();
			subscribers[t] = list;
		}
		list.Add(handler);

	}
	public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
	{
		Type t = typeof(T);
		if (subscribers.TryGetValue(t, out List<Delegate> list))
		{
			list.Remove(handler);
			if (list.Count == 0) subscribers.Remove(t);
		}

	}


	public static void Raise<T>(T ev) where T : IGameEvent
	{
		Type t = typeof(T);
		if (subscribers.TryGetValue(t, out List<Delegate> list))
		{
			Delegate[] copy = list.ToArray();
			foreach (var d in copy)
			{
				((Action<T>)d)(ev);
			}
		}
	}

}
