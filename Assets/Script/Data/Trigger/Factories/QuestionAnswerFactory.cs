using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


//Example
//<Trigger id="example">
//	<QuestionAnswer>
//		<Question>a question?</Question>
//		<Answer>the right answer</Answer>
//	</QuestionAnswer>
//	<PreTriggerObject>
//		<ObjectId>732</ObjectId>
//	</PreTriggerObject>
//	<PostTriggerObject>
//		<ObjectId>733</ObjectId>
//	</PostTriggerObject>
//</Trigger>


/// <summary>
/// Builde a new QuestionAnswer Trigger.
/// </summary>
public class QuestionAnswerFactory: TriggerFactory
{
	/// <summary>
	/// The method for creating a trigger
	/// </summary>
	/// <param name="_trigger">The xml element containing the trigger description (Root node = Trigger)</param>
	/// <returns>The newly created trigger.</returns>
	public override Trigger CreateTrigger(XElement _trigger)
	{
		String id = _trigger.Attribute("id").Value;
		String question = _trigger.Descendants("Question").First().Value;
		String answer = _trigger.Descendants("Answer").First().Value;

		QuestionAnswerTrigger t = new QuestionAnswerTrigger(id, question, answer);

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

