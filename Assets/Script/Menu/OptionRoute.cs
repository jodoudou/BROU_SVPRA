using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionRoute: MonoBehaviour
{
	[SerializeField]
	private Text title = null;
	[SerializeField]
	private Text description = null;
	[SerializeField]
	private Text modif = null;
	[SerializeField]
	private Text distance = null;
	[SerializeField]
	private GameObject saveButton = null;

	public void Init(Element route)
	{
		title.text = route.title;
		description.text = route.description;
		modif.text = "Mise à jours le " + route.lastModificationDate;

		float lon;
		float lat;
		if (float.TryParse(route.longitude, out lon) && float.TryParse(route.latitude, out lat))
		{
			distance.text = ToolBox.Instance.sensorManager.gps.currentPosition.GetMeterDistanceTo(new GeoCoordinate(lat, lon, 0)).ToString("f0") + " mètres";
		}
		else
		{
			distance.text = "Distance inconnue";
		}
		saveButton.SetActive(!ToolBox.Instance.settings.savedRoute);
	}
}
