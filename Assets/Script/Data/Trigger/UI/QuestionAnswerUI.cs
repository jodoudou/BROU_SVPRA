using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class QuestionAnswerUI: MonoBehaviour
{
	[SerializeField]
	private Text question = null;
	[SerializeField]
	private Button submit = null;
	[SerializeField]
	private InputField answer = null;

	private QuestionAnswerTrigger trigger;

	private void Start()
	{
		submit.onClick.AddListener(() => Submit());
		Enable(false);
	}

	public void Init(QuestionAnswerTrigger _trigger)
	{
		trigger = _trigger;
		question.text = trigger.question;
		answer.text = "";
		Enable(true);
	}

	public void Enable(bool b)
	{
		gameObject.SetActive(b);
	}

	private void Submit()
	{
		trigger.SubmitAnswer(answer.text);
	}


}
