using UnityEngine;
using System.Collections;

public class Orientation: SubjectMono
{
#if UNITY_EDITOR
	public float roll;
	public float pitch;
	public float azimut;

	private void Update()
	{
		NotifyAllObservers();
	}
#else
	public float roll { get; protected set; }
	public float pitch { get; protected set; }
	public float azimut { get; protected set; }
#endif

	protected bool isCompassEnable = false;
	protected bool fullOrientationEnable = false;

	/// <summary>
	/// Only enable the compass when of the level 2/scene Game
	/// update the azimut
	/// </summary>
	/// <param name="level"></param>
	private void OnLevelWasLoaded(int level)
	{
		isCompassEnable = level == 2;
		Input.compass.enabled = isCompassEnable;
	}

	/// <summary>
	/// enable full orientation update
	/// update roll, pitch, azimut
	/// </summary>
	/// <param name="enable"></param>
	public virtual void EnableFullOrientation(bool enable)
	{
		fullOrientationEnable = enable;
	}
}


