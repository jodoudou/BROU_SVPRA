using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Xml.Linq;
using System.Linq;

/// <summary>
/// Navigation in a google map with the static map api
/// </summary>
public class GoogleMapNavigation: MonoBehaviour, Observer
{
	private const string API_SERVER = "https://maps.googleapis.com/maps/api/staticmap?center=";
	private const int SCALE = 1;
	private const int MAX_ZOOM = 19;
	private const int MIN_ZOOM = 15;
	private const float MOVE_DISTANCE = 0.0003f;//33 meter

	[SerializeField]
	private Material mapContainer = null;
	[SerializeField]
	private Transform cursor = null;
	[SerializeField]
	private CustomLineRenderer lineRenderer = null;
	[SerializeField]
	private LineRenderer directLineRenderer = null;

	private int zoom = 19;
	/// <summary>
	/// Use to make our mercator projection to transform gps position in pixel and vice versa
	/// </summary>
	public MercatorProjection projection { get; private set; }
	/// <summary>
	/// Use for projection
	/// </summary>
	public Vector3 screenMapCenter { get; private set; }
	/// <summary>
	/// GPS position of the center of the map
	/// </summary>
	public GeoCoordinate mapCenterGPS { get; private set; }
	/// <summary>
	/// The gps position of the phone.
	/// </summary>
	public GeoCoordinate userGPSLocation { get; private set; }
	/// <summary>
	/// Position of the phone in the game
	/// </summary>
	public Vector3 userPosition { get; private set; }
	/// <summary>
	/// Size of the map image
	/// Is not the screen size because Google static map is maximum 640x640
	/// </summary>
	private Vector2 mapImageSize;
	/// <summary>
	/// The limit of the cursor for changing the map.
	/// </summary>
	private Vector2 mapLimit;
	/// <summary>
	/// ratio of the map pixel compare to the screen pixel
	/// </summary>
	public float mapExpandRatio { get; private set; }
	/// <summary>
	/// is downloading a new map
	/// </summary>
	public bool isDownloadingMap { get; private set; }
	/// <summary>
	/// is the first update of the class
	/// </summary>
	private bool isFirstUpdate = true;
	public RouteNavigation routeNavigation;
	private ToolBox toolBox;

	private void Awake()
	{
		isDownloadingMap = false;
	}

	private void Start()
	{
		routeNavigation = new RouteNavigation(lineRenderer, directLineRenderer, this);
		toolBox = ToolBox.Instance;
		directLineRenderer.enabled = true;
		toolBox.loadingScreen.enable(true);
		userGPSLocation = new GeoCoordinate();
		projection = new MercatorProjection(zoom);
		mapImageSize = MapSize(Screen.width, Screen.height);
		mapExpandRatio = Screen.width / mapImageSize.x;
		mapLimit = new Vector2(Screen.width / 4, Screen.height / 4);
		toolBox.sensorManager.gps.AddObserver(this);
		toolBox.sensorManager.orientation.AddObserver(this);
		toolBox.download.AddObserver(this);
	}

	private void OnDestroy()
	{
		if (toolBox != null)
		{
			toolBox.download.RemoveObserver(routeNavigation);
			toolBox.download.RemoveObserver(this);
			toolBox.sensorManager.gps.RemoveObserver(this);
			toolBox.sensorManager.orientation.RemoveObserver(this);
		}
	}

	/// <summary>
	/// call by the gps and compass to navigate.
	/// </summary>
	/// <param name="subject"></param>
	public void Notify(BaseSubject subject)
	{
		if (subject is GPS)
		{
			GPS gps = (GPS)subject;
			userGPSLocation = gps.currentPosition;
			UpdateGoogleMapNavigation();
		}
		else if (subject is Orientation)
		{
			Orientation o = (Orientation)subject;
			cursor.rotation = Quaternion.Euler(new Vector3(cursor.rotation.eulerAngles.x, cursor.rotation.eulerAngles.y, -o.azimut));
		}
		else if (subject is Download)
		{
			Download download = (Download)subject;
			if (download.theDownload.type == DownloadType.Map)
			{
				UpdateNewMap(download.lastDownload);
			}
		}
	}

	/// <summary>
	/// Update the map 
	/// if too close from the side of the screen,it will download a new map.
	/// </summary>
	private void UpdateGoogleMapNavigation()
	{
		if (!isDownloadingMap && (mapCenterGPS == null || NeedNewMap()))
		{
			isDownloadingMap = true;
			mapCenterGPS = userGPSLocation;
			toolBox.download.EnqueueStaticMap(mapCenterGPS, zoom, SCALE, mapImageSize.x, mapImageSize.y);
		}

		PlaceCursor();
	}

	/// <summary>
	/// place the cursor of the user position on the map
	/// </summary>
	private void PlaceCursor()
	{
		float x = (projection.GetXFromLongitude(userGPSLocation.Longitude) - screenMapCenter.x) * mapExpandRatio;
		float y = (projection.GetYFromLatitude(userGPSLocation.Latitude) - screenMapCenter.y) * mapExpandRatio;
		userPosition = new Vector3(x, y, cursor.position.z);
		cursor.position = userPosition;
		routeNavigation.UpdateRouteNavigation();
	}

	/// <summary>
	/// check if too close from the side of the screen
	/// </summary>
	/// <returns></returns>
	private bool NeedNewMap()
	{
		if (Math.Abs(userPosition.x) > mapLimit.x || Math.Abs(userPosition.y) > mapLimit.y)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Update a new google map.
	/// </summary>
	/// <returns></returns>
	private void UpdateNewMap(WWW www)
	{
		Texture2D map = www.texture;

		mapContainer.mainTexture = map;

		//We update the screenMapCentre and shiftedCentre for the new map
		float x = projection.GetXFromLongitude((float)mapCenterGPS.Longitude);
		float y = projection.GetYFromLatitude((float)mapCenterGPS.Latitude);

		// this is the centre point where we have to centre our map on the screen. 
		screenMapCenter = new Vector3(x, y, 0);
		isDownloadingMap = false;

		PlaceCursor();
		if (isFirstUpdate)
		{
			toolBox.download.EnqueueDirection(userGPSLocation, routeNavigation.goal.LatLng);
			routeNavigation.DownLoadGoal();
			isFirstUpdate = false;
		}
		routeNavigation.UpdateLine();
		toolBox.loadingScreen.enable(false);
	}

	/// <summary>
	/// Zoom in the Google maps
	/// </summary>
	public void ZoomIn()
	{
		if (zoom < MAX_ZOOM)
		{
			zoom++;
			Zoom();
		}
	}

	/// <summary>
	/// Zoom out the Google maps
	/// </summary>
	public void ZoomOut()
	{
		if (zoom > MIN_ZOOM)
		{
			zoom--;
			Zoom();
		}
	}

	private void Zoom()
	{
		projection = new MercatorProjection(zoom);
		toolBox.download.EnqueueStaticMap(mapCenterGPS, zoom, SCALE, mapImageSize.x, mapImageSize.y);
	}

	/// <summary>
	/// Change between Google direction and a direct line between the user and the goal
	/// </summary>
	public void EnableDirection()
	{
		lineRenderer.enabled = !lineRenderer.enabled;
		directLineRenderer.enabled = !directLineRenderer.enabled;
	}

	/// <summary>
	/// decide what size the map need to be for the application
	/// static google map size is maximum 640x640 pixel 
	/// </summary>
	/// <param name="screenWidth">width of the screen in pixel</param>
	/// <param name="screenHeight">height of the screen in pixel</param>
	/// <returns>size of the map</returns>
	private Vector2 MapSize(float screenWidth, float screenHeight)
	{
		int maxSize = 640;
		float width = maxSize;
		float height = maxSize;

		if (screenHeight < screenWidth)
		{
			height = width / screenWidth * screenHeight;
		}
		else if (screenHeight > screenWidth)
		{
			width = height / screenHeight * screenWidth;
		}

		return new Vector2((int)width, (int)height);
	}
}