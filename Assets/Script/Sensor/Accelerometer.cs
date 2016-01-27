using UnityEngine;
using System.Collections;

public class Accelerometer: Orientation
{
	private void Update()
	{
		if (fullOrientationEnable && Input.accelerationEventCount > 0)
		{
			roll = -(90 * Input.acceleration.x);
			pitch = -(90 * Input.acceleration.z);
			azimut = Input.compass.magneticHeading;
			NotifyAllObservers();
		}
		else if (isCompassEnable)
		{
			azimut = Input.compass.magneticHeading;
			NotifyAllObservers();
		}
	}
}
