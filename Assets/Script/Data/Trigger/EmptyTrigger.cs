using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class EmptyTrigger: Trigger
{
	public EmptyTrigger(string _id)
		: base(_id)
	{
	}

	protected override void ExecuteTrigger()
	{
		ExecutePostTriggerObject();
	}
}
