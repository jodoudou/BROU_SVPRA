using UnityEngine;
using System.Collections;

public class ContinuousRotation: MonoBehaviour
{
	public Vector3 speed;

	private void Update()
	{
		transform.Rotate(speed * Time.deltaTime);
	}
}
