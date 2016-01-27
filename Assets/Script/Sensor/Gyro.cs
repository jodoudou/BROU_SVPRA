using UnityEngine;
using System.Collections;

public class Gyro: Orientation
{
	private void Update()
	{
		if (fullOrientationEnable)
		{
			roll = -(90 * Input.gyro.gravity.x);
			pitch = -(90 * Input.gyro.gravity.z);
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
