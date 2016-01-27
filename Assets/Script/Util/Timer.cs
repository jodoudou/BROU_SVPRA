using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class Timer
{
	private static Stopwatch watch;

	public static void StartTimer()
	{
		watch = Stopwatch.StartNew();
	}

	public static void StopTimer(string title)
	{
		watch.Stop();
		UnityEngine.Debug.Log(title + (watch.ElapsedMilliseconds / 1000f).ToString("f4"));
	}

	public static float GetElapsedSecond()
	{
		return watch.ElapsedMilliseconds / 1000f;
	}
}
