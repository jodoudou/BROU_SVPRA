using UnityEngine;
using System.Collections;

public class NextStep: SubjectMono
{
	/// <summary>
	/// To go to the next trigger event
	/// </summary>
	public void Next()
	{
		NotifyAllObservers();
	}

	public void Enable(bool enable)
	{
		ToolBox.Instance.data.arObjToEnable.Clear();
		gameObject.SetActive(enable);
	}
}
