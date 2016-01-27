using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

//Example
//<Trigger id="example">
//	<FindKey>
//		<Hint>an hint</Hint>
//		<Radius>3</Radius>
//		<KeyObject>740</KeyObject>
//		<Latitude>46.215062</Latitude>
//		<Longitude>5.239631</Longitude>
//	</FindKey>
//	<PreTriggerObject>
//		<ObjectId>730</ObjectId>
//		<ObjectId>738</ObjectId>
//	</PreTriggerObject>
//	<PostTriggerObject>
//		<ObjectId>740</ObjectId>
//	</PostTriggerObject>
//</Trigger>

/// <summary>
/// Create a new find a key trigger
/// </summary>
class FindKeyFactory: TriggerFactory
{
	/// <summary>
	/// The method for creating a trigger
	/// </summary>
	/// <param name="_trigger">The xml element containing the trigger description (Root node = Trigger)</param>
	/// <returns>The newly created trigger.</returns>
	public override Trigger CreateTrigger(XElement _trigger)
	{
		FindKeyTrigger t = null;

		String id = _trigger.Attribute("id").Value;

		string hint = _trigger.Elements().First().Element("Hint") != null ? _trigger.Elements().First().Element("Hint").Value : "";
		string keyObject = _trigger.Elements().First().Element("KeyObject") != null ? _trigger.Elements().First().Element("KeyObject").Value : "";
		float radius = float.Parse(_trigger.Elements().First().Element("Radius").Value);
		GeoCoordinate keyPosition = new GeoCoordinate(float.Parse(_trigger.Elements().First().Element("Latitude").Value), float.Parse(_trigger.Elements().First().Element("Longitude").Value), 0);

		t = new FindKeyTrigger(id, radius, keyPosition, hint, keyObject);

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

