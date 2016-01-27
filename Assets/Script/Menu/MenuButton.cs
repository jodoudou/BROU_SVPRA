using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// A button for the menu
/// </summary>
public class MenuButton: MonoBehaviour
{
	public static event EventHandler ClickMenuButton;

	[HideInInspector]
	public string id { get; set; }

	public void Click()
	{
		if (ClickMenuButton != null)
		{
			ClickMenuButton(this, new EventArgs());
		}
	}
}
