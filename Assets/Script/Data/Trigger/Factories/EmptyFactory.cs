using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class EmptyFactory: TriggerFactory
{
	/// <summary>
	/// The method for creating a trigger
	/// </summary>
	/// <param name="_trigger">The xml element containing the trigger description (Root node = Trigger)</param>
	/// <returns>The newly created trigger.</returns>
	public override Trigger CreateTrigger(XElement _trigger)
	{
		String id = _trigger.Attribute("id").Value;
		EmptyTrigger t = new EmptyTrigger(id);

		t.preTriggerObjectIds = new List<string>();
		t.postTriggerObjectIds = new List<string>();

		if (_trigger.Element("PreTriggerObject") != null)
			foreach (XElement objectIdElement in _trigger.Element("PreTriggerObject").Elements())
			{
				t.preTriggerObjectIds.Add(objectIdElement.Value);
			}

		if (_trigger.Element("PostTriggerObject") != null)
			foreach (XElement objectIdElement in _trigger.Element("PostTriggerObject").Elements())
			{
				t.postTriggerObjectIds.Add(objectIdElement.Value);
			}

		return t;
	}
}