using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDebug: MonoBehaviour
{
	[SerializeField]
	private Text text = null;
	[SerializeField]
	private RectTransform back = null;

#if !LOG
	private void Awake()
	{
		GameObject.DestroyObject(this.gameObject);
	}
#endif

	private void Update()
	{
		GetComponent<Canvas>().sortingOrder = 10;
		back.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, text.preferredWidth + 10);
		back.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.preferredHeight + 10);
		text.text = "GPS : (" + ToolBox.Instance.sensorManager.gps.currentPosition.Latitude.ToString() + ", " + ToolBox.Instance.sensorManager.gps.currentPosition.Longitude.ToString() + ") +- " + ToolBox.Instance.sensorManager.gps.currentPosition.Accuracy.ToString();
		text.text += "\nObj downloading: " + (ToolBox.Instance.download.toDownload.Count + ToolBox.Instance.download.priorityToDownload.Count).ToString();
		text.text += "\nOrientation: (" + ToolBox.Instance.sensorManager.orientation.pitch + ", " + ToolBox.Instance.sensorManager.orientation.azimut + ", " + ToolBox.Instance.sensorManager.orientation.roll + ")";
		text.text += "\nSteps: " + ToolBox.Instance.sensorManager.stepDetector.step;
	}
}