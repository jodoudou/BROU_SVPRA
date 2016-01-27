using UnityEngine;
using System.Collections;

/// <summary>
/// the game object will always look in the direction of another game object
/// </summary>
public class LookAt: MonoBehaviour
{
	public Transform lookAt { get; set; }
	private Vector3 lastPosition = Vector3.zero;

	void Update()
	{
		if (lookAt != null && lookAt.position != lastPosition)
		{
			float z = transform.rotation.eulerAngles.z;
			transform.LookAt(lookAt);
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z);
			lastPosition = lookAt.position;
		}
	}
}
