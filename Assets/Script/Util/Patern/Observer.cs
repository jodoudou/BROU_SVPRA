using UnityEngine;
using System.Collections;

public interface Observer
{
	void Notify(BaseSubject subject);
}

