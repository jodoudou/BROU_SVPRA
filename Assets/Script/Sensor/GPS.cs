using UnityEngine;
using System.Collections;
using System;

public class GPS: SubjectMono, Observer
{
	/// <summary>
	/// 0.74 meters
	/// </summary>
	private const float LENGHT_STEP = 0.000007f;
	private const int MAX_WAIT = 30;
	public GeoCoordinate currentPosition { get; protected set; }
	private double timer = 0;

	private void Awake()
	{
		currentPosition = new GeoCoordinate();
        ///
		ToolBox.Instance.sensorManager.stepDetector.AddObserver(this);
	}

	public void Notify(BaseSubject subject)
	{

        //////////////// Peut être désactiver le StepDetector
        //////////////// Le "if" ici  et au dessus le ToolBox.Instance.sensorManager.stepDetector.AddObserver(this);
        //////////////// La classe StepDetector
        //////////////// SensorManager -> 2 lignes : 20 et 11

        if (subject is StepDetector)
		{
			Vector3 direction = Quaternion.Euler(0, 0, -ToolBox.Instance.sensorManager.orientation.azimut) * Vector3.up;
			currentPosition.SetCoordinate(currentPosition.ToVector()/*+ (direction.normalized * LENGHT_STEP)*/);
			NotifyAllObservers();
		}
	}

	protected void ResetWait()
	{
		timer = 0;
	}

	protected void MaxWaitForUpdate()
	{
		timer += Time.deltaTime;
		if (timer > MAX_WAIT)
		{
			ToolBox.Instance.notification.ShowError(Notification.ERROR_GPS);
			ResetWait();
		}
	}
}
