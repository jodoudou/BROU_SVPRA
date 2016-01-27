using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A 2D line
/// Need to be attach to a camera to work
/// </summary>
public class CustomLineRenderer: MonoBehaviour
{
	/// <summary>
	/// material of the line
	/// </summary>
	private Material mat;
	/// <summary>
	/// All the point for the line.
	/// </summary>
	public List<Vector2> points = new List<Vector2>();
	/// <summary>
	/// all the segment of the line.
	/// </summary>
	private List<Vector2[]> quads = new List<Vector2[]>();
	/// <summary>
	/// width of the line
	/// </summary>
	private float lineWidth = 0.01f;

	/// <summary>
	/// Set the number of point in the line
	/// </summary>
	/// <param name="nb">number of point</param>
	public void SetNbPoint(int nb)
	{
		if (nb >= points.Count)
		{
			for (int i = points.Count; i < nb - 1; i++)
			{
				points.Add(Vector2.zero);
				quads.Add(new Vector2[4]);
			}
			points.Add(Vector2.zero);
		}
		else
		{
			points.RemoveRange(nb, points.Count - nb);
			quads.RemoveRange(nb - 1, quads.Count - (nb - 1));
		}
	}

	/// <summary>
	/// Reset the line
	/// </summary>
	public void Reset()
	{
		points.Clear();
		quads.Clear();
	}

	/// <summary>
	/// Set a point from the line
	/// </summary>
	/// <param name="index">index of the point</param>
	/// <param name="point">new point</param>
	public void SetPoint(int index, Vector2 point)
	{
		if (points.Count > index)
		{
			points[index] = CreatePoint(point);
			UpdateQuad(index);
		}
	}

	/// <summary>
	/// Take the Unity coordinate point and tranform it to a point for the line
	/// </summary>
	/// <param name="point">Unity coordinate point</param>
	/// <returns>point for the line</returns>
	private Vector2 CreatePoint(Vector2 point)
	{
		Vector2 vec = new Vector2();
		vec.x = (point.x + (Screen.width / 2)) / Screen.width;
		vec.y = (point.y + (Screen.height / 2)) / Screen.height;
		return vec;
	}

	/// <summary>
	/// Update the segment before and after a point of the line
	/// </summary>
	/// <param name="index">the point that has been changed</param>
	private void UpdateQuad(int index)
	{
		if (index > 0)
		{
			CreateQuad(index - 1, points[index - 1], points[index]);
		}

		if (index < quads.Count - 1)
		{
			CreateQuad(index, points[index], points[index + 1]);
		}
	}

	/// <summary>
	/// Create a new segment of the line
	/// </summary>
	/// <param name="index">index of the segment</param>
	/// <param name="v1">an end of the segment</param>
	/// <param name="v2">the other end fo the segment</param>
	private void CreateQuad(int index, Vector2 v1, Vector2 v2)
	{
		float xDiff = Mathf.Abs(v1.x - v2.x);
		float yDiff = Mathf.Abs(v1.y - v2.y);
		float diff = xDiff + yDiff;
		float xAdd = lineWidth * (yDiff / diff);
		float yAdd = lineWidth * (xDiff / diff);
		if (v1.x < v2.x)
		{
			xAdd *= -1;
		}
		if (v1.y < v2.y)
		{
			yAdd *= -1;
		}
		quads[index][0] = new Vector2(v2.x - xAdd, v2.y + yAdd);
		quads[index][1] = new Vector2(v1.x - xAdd, v1.y + yAdd);
		quads[index][2] = new Vector2(v1.x + xAdd, v1.y - yAdd);
		quads[index][3] = new Vector2(v2.x + xAdd, v2.y - yAdd);
	}

	private void Start()
	{
		mat = new Material("Shader \"Lines/Colored Blended\" {" + "SubShader { Pass { " + "    Blend SrcAlpha OneMinusSrcAlpha " + "    ZWrite Off Cull Off Fog { Mode Off } " + "    BindChannels {" + "      Bind \"vertex\", vertex Bind \"color\", color }" + "} } }");
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.shader.hideFlags = HideFlags.HideAndDontSave;
	}

	/// <summary>
	/// Render the line to the screen
	/// </summary>
	private void OnPostRender()
	{
		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		for (int i = 0; i < quads.Count; i++)
		{
			GL.Begin(GL.QUADS);
			GL.Color(Color.red);
			GL.Vertex(quads[i][0]);
			GL.Vertex(quads[i][1]);
			GL.Vertex(quads[i][2]);
			GL.Vertex(quads[i][3]);
			GL.End();
		}
		GL.PopMatrix();
	}
}
