using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Create a new multiple choice trigger.
/// </summary>
class MultipleChoiceFactory: TriggerFactory
{
	/// <summary>
	/// The method for creating a trigger
	/// </summary>
	/// <param name="_trigger">The xml element containing the trigger description (Root node = Trigger)</param>
	/// <returns>The newly created trigger.</returns>
	public override Trigger CreateTrigger(XElement _trigger)
	{
		MultipleChoiceTrigger t = null;

		String id = _trigger.Attribute("id").Value;
		String question = _trigger.Descendants("Question").First().Value;

		Dictionary<String, MultipleChoiceTrigger.Choice> choices = new Dictionary<String, MultipleChoiceTrigger.Choice>();

		XElement choicesElement = _trigger.Descendants("Choices").First();


		foreach (XElement choiceElement in choicesElement.Elements())
		{
			MultipleChoiceTrigger.Choice choice = new MultipleChoiceTrigger.Choice();
			choice.Value = choiceElement.Value;
			choice.Action = choiceElement.Attribute("action") == null ? "" : choiceElement.Attribute("action").Value;
			choice.Waypoint = choiceElement.Attribute("waypoint") == null ? "" : choiceElement.Attribute("waypoint").Value;
			choice.Object = choiceElement.Attribute("object") == null ? "" : choiceElement.Attribute("object").Value;
			choices.Add(choice.Value, choice);
		}

		t = new MultipleChoiceTrigger(id, question, choices);

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

