using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This trigger represent a choice give to the user. He will have to choose between different options.
/// </summary>
[Serializable]
public class MultipleChoiceTrigger: Trigger
{
	/// <summary>
	/// Represente a choice give to the user
	/// </summary>
	[Serializable]
	public struct Choice
	{
		/// <summary>
		/// Value of that choice : the text display
		/// </summary>
		public String Value { get; set; }
		/// <summary>
		/// Name of a fonction of whatever we want.
		/// </summary>
		public String Action { get; set; }
		/// <summary>
		/// The waypoint that is unlocked when making to choice.
		/// </summary>
		public String Waypoint { get; set; }
		/// <summary>
		/// An object to display after the user made this choice.
		/// </summary>
		public String Object { get; set; }
	}

	/// <summary>
	/// The "question" aka title of the choice.
	/// </summary>
	public String question { get; private set; }
	/// <summary>
	/// The list of choice indexed by their value.
	/// </summary>
	public Dictionary<String, Choice> choices { get; private set; }

	public Choice ChoiceMade { get; private set; }

	/// <summary>
	/// Build a new trigger
	/// </summary>
	/// <param name="_id">Id of the trigger</param>
	/// <param name="_question">The "question"</param>
	/// <param name="_choices">The list of all choices available</param>
	public MultipleChoiceTrigger(String _id, String _question, Dictionary<String, Choice> _choices)
		: base(_id)
	{
		this.question = _question;
		this.choices = _choices;
	}

	protected override void ExecuteTrigger()
	{
		GameToolBox.Instance.uiGameObject.multipleChoiceTrigger.GetComponent<MultipleChoicesUI>().Init(this);
	}

	public void Submit(string choice)
	{
		if (choices.ContainsKey(choice))
		{
			ChoiceMade = choices[choice];
			GameToolBox.Instance.uiGameObject.multipleChoiceTrigger.GetComponent<MultipleChoicesUI>().Enable(false);
			ExecutePostTriggerObject();
		}
	}
}





