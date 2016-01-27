#if UNITY_ANDROID
using UnityEngine;
using System.Collections;

/// <summary>
/// class that work only on an android device 
/// use an extern plugin GPS.jar
/// </summary>
public class GPSAndroid: GPS
{
	private const string JAVA_CLASS = ".GPS";
	private string bundleIdentifier = "com." + Application.companyName + "." + Application.productName;
	private AndroidJavaClass gpsPlugin;

	private void Start()
	{
		AndroidJNI.AttachCurrentThread();
		gpsPlugin = new AndroidJavaClass(bundleIdentifier + JAVA_CLASS);
	}

	private void Update()
	{
		MaxWaitForUpdate();
		if (gpsPlugin.CallStatic<bool>("GetIsUpdated"))
		{
			ResetWait();
			currentPosition.Latitude = gpsPlugin.CallStatic<float>("GetLatitude");
			currentPosition.Longitude = gpsPlugin.CallStatic<float>("GetLongitude");
			currentPosition.Accuracy = gpsPlugin.CallStatic<float>("GetAccuracy");
			NotifyAllObservers();
		}
	}
}
#endif
