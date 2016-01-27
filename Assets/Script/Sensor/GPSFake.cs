using UnityEngine;
using System.Collections;

/// <summary>
/// not a real GPS, use to debug in the editor
/// </summary>
public class GPSFake: GPS
{
	private float time = 0;
	public Vector2 speed = new Vector2();
	public Vector2 fakePosition;
	public bool update = true;

	private void Start()
	{
		currentPosition = new GeoCoordinate(46.215235f, 5.241015f, 0);//46.21499f, 5.241483f, 0);
		fakePosition = currentPosition.ToVector();
	}

	void Update()
	{
		if (update)
		{
			currentPosition.SetCoordinate(fakePosition);
			time += Time.deltaTime;
			if (time > 0.1f)
			{
				time = 0;
				fakePosition.x += speed.x / 500000;
				fakePosition.y += speed.y / 500000;

				NotifyAllObservers();
			}
		}
		else
		{
			fakePosition = currentPosition.ToVector();
		}
	}
}
