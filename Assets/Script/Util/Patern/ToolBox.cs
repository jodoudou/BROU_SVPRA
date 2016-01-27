using UnityEngine;
using System.Collections;

public class ToolBox: Singleton<ToolBox>
{
	public Download download = null;
	public SensorManager sensorManager = null;
	public LoadingScreen loadingScreen = null;
	public Data data = null;
	public Notification notification = null;
	public TestDevice testDevice = null;
	public Settings settings = null;
	public Load load = null;
	/// <summary>
	/// the default font for the text element
	/// </summary>
	public Font defaultFont = null;
	/// <summary>
	/// the material for the default font
	/// </summary>
	public Material defaultMaterialFont = null;
	public GameObject defaultMesh = null;

	protected ToolBox() { } // guarantee this will be always a singleton only - can't use the constructor!

	private void Awake()
	{
		data = new Data();
		settings = new Settings();
	}

	
}
