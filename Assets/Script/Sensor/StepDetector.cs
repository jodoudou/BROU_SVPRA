using UnityEngine;
using System.Collections;

/// <summary>
/// Detect if the user take a step
/// http://answers.unity3d.com/questions/160106/implementing-a-psuedo-pedometer.html
/// </summary>
public class StepDetector: SubjectMono
{
#if UNITY_EDITOR
	/// <summary>
	/// to perform a step in the editor
	/// </summary>
	public bool performStep = false;
#endif
	public int step = 0;
	private float loLim = 0.005f; // level to fall to the low state 
	private float hiLim = 0.1f; // level to go to high state (and detect step) 
	private bool stateH = false; // comparator state
	private float fHigh = 10.0f; // noise filter control - reduces frequencies above fHigh private 
	private float curAcc = 0; // noise filter 
	private float fLow = 0.1f; // average gravity filter control - time constant about 1/fLow 
	private float avgAcc; // average gravity filter
	private bool isEnable = false;

	private void Start()
	{
		avgAcc = Input.acceleration.magnitude; // initialize avg filter
	}

	private void OnLevelWasLoaded(int level)
	{
		isEnable = level == 2;
	}

	private void FixedUpdate()
	{
		if (isEnable)
		{
			// filter input.acceleration using Lerp 
			curAcc = Mathf.Lerp(curAcc, Input.acceleration.magnitude, Time.deltaTime * fHigh);
			avgAcc = Mathf.Lerp(avgAcc, Input.acceleration.magnitude, Time.deltaTime * fLow);
			var delta = curAcc - avgAcc; // gets the acceleration pulses 
			if (!stateH)
			{ // if state == low... 
				if (delta > hiLim)
				{ // only goes high if input > hiLim 
					stateH = true;
					step++;
					NotifyAllObservers();
				}
			}
			else
			{
				if (delta < loLim)
				{ // only goes low if input < loLim 
					stateH = false;
				}
			}
		}

#if UNITY_EDITOR
		if (performStep)
		{
			step++;
			NotifyAllObservers();
			performStep = false;
		}
#endif
	}
}
