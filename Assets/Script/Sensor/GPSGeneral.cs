using UnityEngine;
using System.Collections;

/// <summary>
/// class that work on all device with a GPS
/// </summary>
public class GPSGeneral: GPS
{
	private const float GPS_ACCURACY = 0.5f;
	private const float GPS_UPDATE_DISTANCE = 1f;
	private double lastUpdateTime = 0;

	public void Start()
	{
		currentPosition = new GeoCoordinate(46.2153f, 5.2415f, 0);
		StartCoroutine(InitGPS());
	}

	/// <summary>
	/// Init the GPS
	/// </summary>
	/// <returns></returns>
	private IEnumerator InitGPS()
	{
		if (Input.location.isEnabledByUser)
		{
#if LOG
			Debug.Log("Starting GPS location");
#endif
			Input.location.Start(GPS_ACCURACY, GPS_UPDATE_DISTANCE);
			while (Input.location.status == LocationServiceStatus.Initializing)
			{
				yield return new WaitForSeconds(1);
			}
			if (Input.location.status != LocationServiceStatus.Running)
			{
#if LOG
				Debug.Log("GPS problem:\nStatus: " + Input.location.status.ToString());
#endif
			}
		}
		else
		{
#if LOG
			Debug.Log("Localization not allow by user");
#endif
		}
	}

	private void Update()
	{
		NotifyAllObservers();
		MaxWaitForUpdate();
		if (Input.location.isEnabledByUser)
		{
			if (Input.location.status == LocationServiceStatus.Running)
			{
				if ((currentPosition == null || Input.location.lastData.timestamp != lastUpdateTime))
				{
					ResetWait();
					lastUpdateTime = Input.location.lastData.timestamp;
					currentPosition = new GeoCoordinate(Input.location.lastData.latitude, Input.location.lastData.longitude, 0, Input.location.lastData.verticalAccuracy);
					NotifyAllObservers();
				}
			}
			else if (Input.location.status != LocationServiceStatus.Initializing)
			{
				StartCoroutine(InitGPS());
			}
		}
	}
}
