using UnityEngine;
using System;


/// <summary>
/// Represent a question and answer trigger. The user must answer a question
/// </summary>
[Serializable]
public class QuestionAnswerTrigger: Trigger
{
	/// <summary>
	/// Question for the user
	/// </summary>
	public String question { get; private set; }
	/// <summary>
	/// The correct answer
	/// </summary>
	private String answer;

	/// <summary>
	/// Build a new trigger
	/// </summary>
	/// <param name="_id">Its id</param>
	/// <param name="_question">The question to ask</param>
	/// <param name="_answer">The answer to the question</param>
	public QuestionAnswerTrigger(String _id, String _question, String _answer)
		: base(_id)
	{
		this.question = _question;
		this.answer = _answer;
	}

	protected override void ExecuteTrigger()
	{
		GameToolBox.Instance.uiGameObject.questionAnswerTrigger.GetComponent<QuestionAnswerUI>().Init(this);
	}

	public void SubmitAnswer(string _answer)
	{
		if (answer.ToLower().Equals(_answer.ToLower()))
		{
			GameToolBox.Instance.uiGameObject.questionAnswerTrigger.GetComponent<QuestionAnswerUI>().Enable(false);
			ExecutePostTriggerObject();
		}
	}
}

