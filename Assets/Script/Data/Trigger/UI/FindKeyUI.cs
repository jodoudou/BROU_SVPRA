using UnityEngine;
using UnityEngine.UI;

public class FindKeyUI: MonoBehaviour, Observer
{
	[SerializeField]
	private GameObject extend = null;
	[SerializeField]
	private GameObject shrink = null;
	[SerializeField]
	private GameObject keyIsFind = null;
	[SerializeField]
	private Text hint = null;

	private FindKeyTrigger trigger;

	private void Start()
	{
		Enable(false);
	}

	public void Init(FindKeyTrigger _trigger)
	{
		trigger = _trigger;
		enabled = false;
		hint.text = trigger.hint;
		ToolBox.Instance.sensorManager.gps.AddObserver(this);
		Enable(true);

	}

	public void Enable(bool b)
	{
		gameObject.SetActive(b);
	}

	private void KeyIsFind()
	{
		extend.SetActive(false);
		shrink.SetActive(false);
		keyIsFind.SetActive(true);
	}

	public void OkKeyFind()
	{
		trigger.KeyIsFind();
		Enable(false);
	}

	public void Notify(BaseSubject subject)
	{
		if (subject is GPS)
		{
			double distance = trigger.keyPosition.GetMeterDistanceTo(((GPS)subject).currentPosition);
			if (distance <= trigger.toleranceRadius)
			{
				KeyIsFind();
			}
		}
	}
}
