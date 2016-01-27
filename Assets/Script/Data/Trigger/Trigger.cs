using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// Base class for triggers
/// </summary>
[Serializable]
public abstract class Trigger: Subject, Observer
{
	/// <summary>
	/// Id of that trigger
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// The list of pretrigger object ids
	/// </summary>
	public List<string> preTriggerObjectIds { get; set; }
	/// <summary>
	/// The list of posttrigger object ids
	/// </summary>
	public List<string> postTriggerObjectIds { get; set; }

	/// <summary>
	/// The list of waypoint where the user can go after this trigger, in the case of Auth = List
	/// </summary>
	public List<string> nextWaypointList { get; set; }

	private bool inPre = false;
	private bool inPost = false;

	public Trigger(string _id)
	{
		id = _id;
	}

	public void Execute()
	{
		GameToolBox.Instance.nextStep.AddObserver(this);
		ExecutePreTriggerObject();
	}

	private void ExecutePreTriggerObject()
	{
		if (preTriggerObjectIds.Count > 0)
		{
			inPre = true;
			GameToolBox.Instance.nextStep.Enable(true);
			ToolBox.Instance.data.arObjToEnable = preTriggerObjectIds;
			GameToolBox.Instance.gameManager.ShowArObject();
			ToolBox.Instance.notification.ShowNotif(Notification.GO_AR);
		}
		else
		{
			ExecuteTrigger();
		}
	}

	protected void ExecutePostTriggerObject()
	{
		if (postTriggerObjectIds.Count > 0)
		{
			inPost = true;
			GameToolBox.Instance.nextStep.Enable(true);
			ToolBox.Instance.data.arObjToEnable = postTriggerObjectIds;
			GameToolBox.Instance.gameManager.ShowArObject();
			ToolBox.Instance.notification.ShowNotif(Notification.GO_AR);
		}
		else
		{
			NotifyAllObservers();
		}
	}

	protected abstract void ExecuteTrigger();

	public void Notify(BaseSubject subject)
	{
		if (subject is NextStep)
		{
			if (inPre)
			{
				inPre = false;
				ExecuteTrigger();
			}
			else if (inPost)
			{
				inPost = false;
				GameToolBox.Instance.nextStep.RemoveObserver(this);
				NotifyAllObservers();
			}
		}
	}
}