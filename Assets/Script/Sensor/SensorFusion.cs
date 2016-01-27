using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// http://www.codeproject.com/Articles/729759/Android-Sensor-Fusion-Tutorial
/// </summary>
public class SensorFusion: Orientation
{
	// angular speeds from gyro
	private Vector3 gyro;

	// rotation matrix from gyro data
	private float[] gyroMatrix = new float[9];

	// orientation angles from gyro matrix
	private Vector3 gyroOrientation = new Vector3(0, 0, 0);

	// magnetic field vector
	private Vector3 magnet;

	// accelerometer vector
	private Vector3 accel;

	// orientation angles from accel and magnet
	private Vector3 accMagOrientation;

	// final orientation angles from sensor fusion
	private Vector3 fusedOrientation;

	// accelerometer and magnetometer based rotation matrix
	private float[] rotationMatrix = new float[9];

	private const float NS2S = 1.0f / 1000000000.0f;
	private bool initState = true;

	private const float TIME_CONSTANT = 0.03f;
	private const float FILTER_COEFFICIENT = 0.98f;

	private void Start()
	{
		gyroMatrix[0] = 1.0f;
		gyroMatrix[1] = 0.0f;
		gyroMatrix[2] = 0.0f;
		gyroMatrix[3] = 0.0f;
		gyroMatrix[4] = 1.0f;
		gyroMatrix[5] = 0.0f;
		gyroMatrix[6] = 0.0f;
		gyroMatrix[7] = 0.0f;
		gyroMatrix[8] = 1.0f;
	}

	private void Update()
	{
		if (fullOrientationEnable)
		{
			if (Input.accelerationEventCount > 0)
			{
				Vector3 inAccel = Input.acceleration * 10;
				accel = new Vector3(-inAccel.y, inAccel.x, -inAccel.z);
				magnet = Input.compass.rawVector;
				CalculateAccMagOrientation();
			}
			UpdateAzimut();
			GyroFunction();
		}
		else if (isCompassEnable)
		{
			azimut = Input.compass.magneticHeading;
			NotifyAllObservers();
		}
	}

	public override void EnableFullOrientation(bool enable)
	{
		base.EnableFullOrientation(enable);
		Input.gyro.enabled = enable;
		if (enable)
		{
#if LOG
			Debug.Log("Start sensor fusion");
#endif
			StartCoroutine(UpdateFusedOrientation());
		}
	}

	private IEnumerator UpdateFusedOrientation()
	{
		yield return new WaitForSeconds(1);
		while (fullOrientationEnable)
		{
			yield return new WaitForSeconds(TIME_CONSTANT);
			CalculateFusedOrientation();

			roll = fusedOrientation.y * Mathf.Rad2Deg;
			pitch = (fusedOrientation.z * Mathf.Rad2Deg) + 90;
			NotifyAllObservers();
		}
	}

	private void UpdateAzimut()
	{
			azimut = Mathf.LerpAngle(azimut, Input.compass.magneticHeading, 0.25f);
	}

	private void CalculateFusedOrientation()
	{
		float oneMinusCoeff = 1.0f - FILTER_COEFFICIENT;

		/*
		 * Fix for 179° <--> -179° transition problem: Check whether one of
		 * the two orientation angles (gyro or accMag) is negative while the
		 * other one is positive. If so, add 360° (2 * Mathf.PI) to the
		 * negative value, perform the sensor fusion, and remove the 360°
		 * from the result if it is greater than 180°. This stabilizes the
		 * output in positive-to-negative-transition cases.
		 */

		float halfPi = -0.5f * Mathf.PI;
		float twoPi = 2.0f * Mathf.PI;

		// azimuth
		//if (gyroOrientation[0] < halfPi && accMagOrientation[0] > 0.0)
		//{
		//	fusedOrientation[0] = (FILTER_COEFFICIENT * (gyroOrientation[0] + twoPi) + oneMinusCoeff * accMagOrientation[0]);
		//	fusedOrientation[0] -= (fusedOrientation[0] > Mathf.PI) ? twoPi : 0;
		//}
		//else if (accMagOrientation[0] < halfPi && gyroOrientation[0] > 0.0)
		//{
		//	fusedOrientation[0] = (FILTER_COEFFICIENT * gyroOrientation[0] + oneMinusCoeff * (accMagOrientation[0] + twoPi));
		//	fusedOrientation[0] -= (fusedOrientation[0] > Mathf.PI) ? twoPi : 0;
		//}
		//else
		//{
		//	fusedOrientation[0] = FILTER_COEFFICIENT * gyroOrientation[0] + oneMinusCoeff * accMagOrientation[0];
		//}

		// roll
		if (gyroOrientation[1] < halfPi && accMagOrientation[1] > 0.0)
		{
			fusedOrientation[1] = (FILTER_COEFFICIENT * (gyroOrientation[1] + twoPi) + oneMinusCoeff * accMagOrientation[1]);
			fusedOrientation[1] -= (fusedOrientation[1] > Mathf.PI) ? twoPi : 0;
		}
		else if (accMagOrientation[1] < halfPi && gyroOrientation[1] > 0.0)
		{
			fusedOrientation[1] = (FILTER_COEFFICIENT * gyroOrientation[1] + oneMinusCoeff * (accMagOrientation[1] + twoPi));
			fusedOrientation[1] -= (fusedOrientation[1] > Mathf.PI) ? twoPi : 0;
		}
		else
		{
			fusedOrientation[1] = FILTER_COEFFICIENT * gyroOrientation[1] + oneMinusCoeff * accMagOrientation[1];
		}

		// pitch
		if (gyroOrientation[2] < halfPi && accMagOrientation[2] > 0.0)
		{
			fusedOrientation[2] = (FILTER_COEFFICIENT * (gyroOrientation[2] + twoPi) + oneMinusCoeff * accMagOrientation[2]);
			fusedOrientation[2] -= (fusedOrientation[2] > Mathf.PI) ? twoPi : 0;
		}
		else if (accMagOrientation[2] < halfPi && gyroOrientation[2] > 0.0)
		{
			fusedOrientation[2] = (FILTER_COEFFICIENT * gyroOrientation[2] + oneMinusCoeff * (accMagOrientation[2] + twoPi));
			fusedOrientation[2] -= (fusedOrientation[2] > Mathf.PI) ? twoPi : 0;
		}
		else
		{
			fusedOrientation[2] = FILTER_COEFFICIENT * gyroOrientation[2] + oneMinusCoeff * accMagOrientation[2];
		}

		// overwrite gyro matrix and orientation with fused orientation
		// to comensate gyro drift
		gyroMatrix = GetRotationMatrixFromOrientation(fusedOrientation);
		gyroOrientation = fusedOrientation;
	}

	/// <summary>
	/// calculates orientation angles from accelerometer and magnetometer output
	/// </summary>
	private void CalculateAccMagOrientation()
	{
		if (GetRotationMatrix(out rotationMatrix, accel, magnet))
		{
			GetOrientation(rotationMatrix, out accMagOrientation);
		}
	}

	/// <summary>
	/// This function performs the integration of the gyroscope data.
	/// It writes the gyroscope based orientation into gyroOrientation.
	/// </summary>
	/// <param name="g"></param>
	private void GyroFunction()
	{
		// don't start until first accelerometer/magnetometer orientation has
		// been acquired
		if (accMagOrientation != null)
		{
			// initialisation of the gyroscope based rotation matrix
			if (initState)
			{
				float[] initMatrix = new float[9];
				initMatrix = GetRotationMatrixFromOrientation(accMagOrientation);
				Vector3 test = new Vector3();
				GetOrientation(initMatrix, out test);
				gyroMatrix = MatrixMultiplication(gyroMatrix, initMatrix);
				initState = false;
			}

			// copy the new gyro values into the gyro array
			// convert the raw gyro data into a rotation vector
			float[] deltaVector;

			float dT = Time.deltaTime;//(Time.unscaledTime - timestamp) * NS2S;
			Vector3 input = Input.gyro.rotationRate;
			gyro = new Vector3(input.y, -input.x, input.z);
			GetRotationVectorFromGyro(gyro, out deltaVector, dT / 2.0f);

			// convert rotation vector into rotation matrix
			float[] deltaMatrix;
			GetRotationMatrixFromVector(out deltaMatrix, deltaVector);

			// apply the new rotation interval on the gyroscope based rotation
			// matrix
			gyroMatrix = MatrixMultiplication(gyroMatrix, deltaMatrix);

			// get the gyroscope based orientation from the rotation matrix
			GetOrientation(gyroMatrix, out gyroOrientation);
		}
		// measurement done, save current time for next interval
		//timestamp = Time.unscaledTime;
	}

	/// <summary>
	/// It calculates a rotation vector from the gyroscope angular speed values.
	/// </summary>
	/// <param name="gyroValues"></param>
	/// <param name="deltaRotationVector"></param>
	/// <param name="timeFactor"></param>
	/// <Source>http://developer.android.com/reference/android/hardware/SensorEvent.html#values</Source>
	private void GetRotationVectorFromGyro(Vector3 gyroValues, out float[] deltaRotationVector, float timeFactor)
	{
		float[] normValues = new float[3];
		deltaRotationVector = new float[4];

		// Calculate the angular speed of the sample
		float omegaMagnitude = Mathf.Sqrt(gyroValues[0] * gyroValues[0] + gyroValues[1] * gyroValues[1] + gyroValues[2] * gyroValues[2]);

		// Normalize the rotation vector if it's big enough to get the axis
		if (omegaMagnitude > Mathf.Epsilon)
		{
			normValues[0] = gyroValues[0] / omegaMagnitude;
			normValues[1] = gyroValues[1] / omegaMagnitude;
			normValues[2] = gyroValues[2] / omegaMagnitude;
		}

		// Integrate around this axis with the angular speed by the timestep
		// in order to get a delta rotation from this sample over the timestep
		// We will convert this axis-angle representation of the delta rotation
		// into a quaternion before turning it into the rotation matrix.
		float thetaOverTwo = omegaMagnitude * timeFactor;
		float sinThetaOverTwo = Mathf.Sin(thetaOverTwo);
		float cosThetaOverTwo = Mathf.Cos(thetaOverTwo);
		deltaRotationVector[0] = sinThetaOverTwo * normValues[0];
		deltaRotationVector[1] = sinThetaOverTwo * normValues[1];
		deltaRotationVector[2] = sinThetaOverTwo * normValues[2];
		deltaRotationVector[3] = cosThetaOverTwo;
	}

	private float[] GetRotationMatrixFromOrientation(Vector3 o)
	{
		float[] xM = new float[9];
		float[] yM = new float[9];
		float[] zM = new float[9];

		float sinX = Mathf.Sin(o[1]);
		float cosX = Mathf.Cos(o[1]);
		float sinY = Mathf.Sin(o[2]);
		float cosY = Mathf.Cos(o[2]);
		float sinZ = Mathf.Sin(o[0]);
		float cosZ = Mathf.Cos(o[0]);

		// rotation about x-axis (pitch)
		xM[0] = 1.0f;
		xM[1] = 0.0f;
		xM[2] = 0.0f;
		xM[3] = 0.0f;
		xM[4] = cosX;
		xM[5] = sinX;
		xM[6] = 0.0f;
		xM[7] = -sinX;
		xM[8] = cosX;

		// rotation about y-axis (roll)
		yM[0] = cosY;
		yM[1] = 0.0f;
		yM[2] = sinY;
		yM[3] = 0.0f;
		yM[4] = 1.0f;
		yM[5] = 0.0f;
		yM[6] = -sinY;
		yM[7] = 0.0f;
		yM[8] = cosY;

		// rotation about z-axis (azimuth)
		zM[0] = cosZ;
		zM[1] = sinZ;
		zM[2] = 0.0f;
		zM[3] = -sinZ;
		zM[4] = cosZ;
		zM[5] = 0.0f;
		zM[6] = 0.0f;
		zM[7] = 0.0f;
		zM[8] = 1.0f;

		// rotation order is y, x, z (roll, pitch, azimuth)
		float[] resultMatrix = MatrixMultiplication(xM, yM);
		resultMatrix = MatrixMultiplication(zM, resultMatrix);
		return resultMatrix;
	}

	private float[] MatrixMultiplication(float[] A, float[] B)
	{
		float[] result = new float[9];

		result[0] = A[0] * B[0] + A[1] * B[3] + A[2] * B[6];
		result[1] = A[0] * B[1] + A[1] * B[4] + A[2] * B[7];
		result[2] = A[0] * B[2] + A[1] * B[5] + A[2] * B[8];

		result[3] = A[3] * B[0] + A[4] * B[3] + A[5] * B[6];
		result[4] = A[3] * B[1] + A[4] * B[4] + A[5] * B[7];
		result[5] = A[3] * B[2] + A[4] * B[5] + A[5] * B[8];

		result[6] = A[6] * B[0] + A[7] * B[3] + A[8] * B[6];
		result[7] = A[6] * B[1] + A[7] * B[4] + A[8] * B[7];
		result[8] = A[6] * B[2] + A[7] * B[5] + A[8] * B[8];

		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="R"></param>
	/// <param name="values"></param>
	/// <returns></returns>
	/// <Source>http://grepcode.com/file/repo1.maven.org/maven2/org.robolectric/android-all/5.0.0_r2-robolectric-0/android/hardware/SensorManager.java#SensorManager.getOrientation%28float%5B%5D%2Cfloat%5B%5D%29</Source>
	private Vector3 GetOrientation(float[] R, out Vector3 values)
	{
		/*
		 * 3x3 (length=9) case:
		 *   /  R[ 0]   R[ 1]   R[ 2]  \
		 *   |  R[ 3]   R[ 4]   R[ 5]  |
		 *   \  R[ 6]   R[ 7]   R[ 8]  /
		 *
		 */
		values = new Vector3();
		values[0] = Mathf.Atan2(R[1], R[4]);
		values[1] = Mathf.Asin(-R[7]);
		values[2] = Mathf.Atan2(-R[6], R[8]);

		return values;
	}

	/// <summary>
	/// </summary>
	/// <param name="R">out Rotation matrix</param>
	/// <param name="gravity"></param>
	/// <param name="geomagnetic"></param>
	/// <returns></returns>
	/// <Source>http://grepcode.com/file/repository.grepcode.com/java/ext/com.google.android/android/5.0.2_r1/android/hardware/SensorManager.java#SensorManager.getRotationMatrix%28float%5B%5D%2Cfloat%5B%5D%2Cfloat%5B%5D%2Cfloat%5B%5D%29</Source>
	private bool GetRotationMatrix(out float[] R, Vector3 gravity, Vector3 geomagnetic)
	{
		R = null;
		// TODO: move this to native code for efficiency
		float Ax = gravity[0];
		float Ay = gravity[1];
		float Az = gravity[2];
		float Ex = geomagnetic[0];
		float Ey = geomagnetic[1];
		float Ez = geomagnetic[2];
		float Hx = Ey * Az - Ez * Ay;
		float Hy = Ez * Ax - Ex * Az;
		float Hz = Ex * Ay - Ey * Ax;
		float normH = Mathf.Sqrt(Hx * Hx + Hy * Hy + Hz * Hz);
		if (normH < 0.1f)
		{
			// device is close to free fall (or in space?), or close to
			// magnetic north pole. Typical values are  > 100.
			return false;
		}
		float invH = 1.0f / normH;
		Hx *= invH;
		Hy *= invH;
		Hz *= invH;
		float invA = 1.0f / Mathf.Sqrt(Ax * Ax + Ay * Ay + Az * Az);
		Ax *= invA;
		Ay *= invA;
		Az *= invA;
		float Mx = Ay * Hz - Az * Hy;
		float My = Az * Hx - Ax * Hz;
		float Mz = Ax * Hy - Ay * Hx;
		R = new float[9];
		R[0] = Hx;
		R[1] = Hy;
		R[2] = Hz;
		R[3] = Mx;
		R[4] = My;
		R[5] = Mz;
		R[6] = Ax;
		R[7] = Ay;
		R[8] = Az;

		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="R"></param>
	/// <param name="rotationVector"></param>
	/// <Source>http://grepcode.com/file/repo1.maven.org/maven2/org.robolectric/android-all/5.0.0_r2-robolectric-0/android/hardware/SensorManager.java#SensorManager.getRotationMatrixFromVector%28float%5B%5D%2Cfloat%5B%5D%29</Source>
	private void GetRotationMatrixFromVector(out float[] R, float[] rotationVector)
	{
		float q0;
		float q1 = rotationVector[0];
		float q2 = rotationVector[1];
		float q3 = rotationVector[2];

		if (rotationVector.Length >= 4)
		{
			q0 = rotationVector[3];
		}
		else
		{
			q0 = 1 - q1 * q1 - q2 * q2 - q3 * q3;
			q0 = (q0 > 0) ? Mathf.Sqrt(q0) : 0;
		}

		float sq_q1 = 2 * q1 * q1;
		float sq_q2 = 2 * q2 * q2;
		float sq_q3 = 2 * q3 * q3;
		float q1_q2 = 2 * q1 * q2;
		float q3_q0 = 2 * q3 * q0;
		float q1_q3 = 2 * q1 * q3;
		float q2_q0 = 2 * q2 * q0;
		float q2_q3 = 2 * q2 * q3;
		float q1_q0 = 2 * q1 * q0;

		R = new float[9];
		R[0] = 1 - sq_q2 - sq_q3;
		R[1] = q1_q2 - q3_q0;
		R[2] = q1_q3 + q2_q0;

		R[3] = q1_q2 + q3_q0;
		R[4] = 1 - sq_q1 - sq_q3;
		R[5] = q2_q3 - q1_q0;

		R[6] = q1_q3 - q2_q0;
		R[7] = q2_q3 + q1_q0;
		R[8] = 1 - sq_q1 - sq_q2;
	}
}
