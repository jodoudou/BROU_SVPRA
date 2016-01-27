using UnityEngine;
using System.Collections;

/// <summary>
/// Contain all the sensor of the device and choose wich to use
/// </summary>
public class SensorManager: MonoBehaviour
{
	public GPS gps { get; private set; }
	public Orientation orientation { get; private set; }
	public StepDetector stepDetector { get; private set; }

	private void Awake()
	{
#if UNITY_EDITOR
		orientation = gameObject.AddComponent<Orientation>();
#else
		orientation = gameObject.AddComponent<SensorFusion>();
#endif
		stepDetector = gameObject.AddComponent<StepDetector>();

#if UNITY_EDITOR
		gps = gameObject.AddComponent<GPSFake>();
#elif UNITY_ANDROID
				gps = gameObject.AddComponent<GPSAndroid>();
#else
				gps = gameObject.AddComponent<GPSGeneral>();
#endif

	}
}