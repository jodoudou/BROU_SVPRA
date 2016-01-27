using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MultipleChoicesUI: MonoBehaviour
{
	[SerializeField]
	private GameObject choiceButtonParent = null;
	[SerializeField]
	private Text question = null;

	private List<Button> choiceButton = new List<Button>();

	private MultipleChoiceTrigger trigger;

	private void Start()
	{
		choiceButton = new List<Button>(choiceButtonParent.transform.GetComponentsInChildren<Button>());
		DisableAllButton();
		Enable(false);
	}

	private void DisableAllButton()
	{
		foreach (Button b in choiceButton)
		{
			b.gameObject.SetActive(false);
		}
	}

	public void Init(MultipleChoiceTrigger _trigger)
	{
		DisableAllButton();
		trigger = _trigger;
		question.text = trigger.question;

		int i = 0;
		foreach (MultipleChoiceTrigger.Choice choice in trigger.choices.Values)
		{
			choiceButton[i].GetComponentsInChildren<Text>(true)[0].text = choice.Value;
			choiceButton[i].gameObject.SetActive(true);
			i++;
		}
		Enable(true);
	}

	public void Enable(bool b)
	{
		gameObject.SetActive(b);
	}

	public void ChoiceButton_Click(Text value)
	{
		trigger.Submit(value.text);
	}
}
