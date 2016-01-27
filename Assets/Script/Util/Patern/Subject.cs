using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Subject: BaseSubject
{
	protected List<Observer> observers = new List<Observer>();

	private void OnLevelWasLoaded(int level)
	{
		for (int i = 0; i < observers.Count; i++)
		{
			if (observers[i] == null || observers[i].Equals(null))
			{
				observers.RemoveAt(i);
			}
		}
	}

	public void AddObserver(Observer observer)
	{
		if (!observers.Contains(observer))
		{
			observers.Add(observer);
		}
	}

	public void RemoveObserver(Observer observer)
	{
		if (observers.Contains(observer))
		{
			observers.Remove(observer);
		}
	}

	protected void NotifyAllObservers()
	{
		for (int i = 0; i < observers.Count; i++)
		{
			if (observers[i] == null || observers[i].Equals(null))
			{
				observers.RemoveAt(i);
				i--;
			}
			else
			{
				observers[i].Notify(this);
			}
		}
	}
}

public interface BaseSubject
{
	void AddObserver(Observer observer);
	void RemoveObserver(Observer observer);
}

