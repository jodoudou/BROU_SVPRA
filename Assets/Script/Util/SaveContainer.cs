using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Container to hold Unity class because they can't be serialize.
/// </summary>
public interface SaveAr
{
	GameObject Restore();
}

[Serializable]
public class SaveObject3D: SaveAr
{
	public List<SMesh> meshes { get; set; }
	public string mtlFileName { get; set; }

	public SaveObject3D(MtlImporter i)
	{
		meshes = new List<SMesh>();
		foreach (KeyValuePair<string, List<GameObject>> pair in i.meshes)
		{
			foreach (GameObject g in pair.Value)
			{
				meshes.Add(new SMesh(g.GetComponent<MeshFilter>().mesh, pair.Key));
			}
		}
		mtlFileName = i.fileName;
	}

	public GameObject Restore()
	{
		GameObject go = new GameObject();
		Dictionary<string, List<GameObject>> mtlMeshes = new Dictionary<string, List<GameObject>>();
		foreach (SMesh m in meshes)
		{
			GameObject child = m.Restore();
			child.transform.SetParent(go.transform);
			if (mtlMeshes.ContainsKey(m.name))
			{
				mtlMeshes[m.name].Add(child);
			}
			else
			{
				List<GameObject> l = new List<GameObject>();
				l.Add(child);
				mtlMeshes.Add(m.name, l);
			}
		}
		go.AddComponent<MtlImporter>().fileName = mtlFileName;
		go.GetComponent<MtlImporter>().meshes = mtlMeshes;
		return go;
	}
}

[Serializable]
public class SaveText: SaveAr
{
	public string text { get; set; }

	public SaveText(string s)
	{
		text = s;
	}

	public GameObject Restore()
	{
		GameObject go = new GameObject();
		TextMesh mesh = go.AddComponent<TextMesh>();
		mesh.text = text;
		mesh.font = ToolBox.Instance.defaultFont;
		mesh.anchor = TextAnchor.LowerCenter;
		mesh.color = Color.black;
		go.GetComponent<Renderer>().material = ToolBox.Instance.defaultMaterialFont;
		return go;
	}
}

[Serializable]
public class SaveImage: SaveAr
{
	public byte[] texture { get; set; }
	public SVector3 scale { get; set; }

	public SaveImage(Texture t, Transform tr)
	{
		texture = t != null ? ((Texture2D)t).EncodeToPNG() : null;
		scale = new SVector3(tr.localScale);
	}

	public GameObject Restore()
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
		GameObject.Destroy(go.GetComponent<MeshCollider>());
		Texture2D image = new Texture2D(2, 2);
		image.LoadImage(texture);
		go.GetComponent<MeshRenderer>().material.mainTexture = image;
		go.transform.localScale = scale.Restore();
		return go;
	}
}

[Serializable]
public class SVector3
{
	public float x { get; set; }
	public float y { get; set; }
	public float z { get; set; }

	public SVector3(Vector3 v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3 Restore()
	{
		return new Vector3(x, y, z);
	}
}

[Serializable]
public class SVector2
{
	public float x { get; set; }
	public float y { get; set; }

	public SVector2(Vector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public Vector2 Restore()
	{
		return new Vector2(x, y);
	}
}

[Serializable]
public class SMesh
{
	public string name { get; set; }
	public SVector3[] vertices { get; set; }
	public SVector2[] uv { get; set; }
	public SVector3[] normals { get; set; }
	public int[] triangles { get; set; }

	public SMesh(Mesh m, string _name)
	{
		vertices = new SVector3[m.vertices.Length];
		transfer(m.vertices, vertices);
		uv = new SVector2[m.uv.Length];
		transfer(m.uv, uv);
		normals = new SVector3[m.normals.Length];
		transfer(m.normals, normals);
		triangles = m.triangles;
		name = _name;
	}

	private void transfer(Vector3[] v, SVector3[] s)
	{
		for (int i = 0; i < v.Length; i++)
		{
			s[i] = new SVector3(v[i]);
		}
	}

	private void transfer(Vector2[] v, SVector2[] s)
	{
		for (int i = 0; i < v.Length; i++)
		{
			s[i] = new SVector2(v[i]);
		}
	}

	public GameObject Restore()
	{
		GameObject go = new GameObject(name);
		Mesh mesh = new Mesh();
		mesh.vertices = Restore(vertices);
		mesh.uv = Restore(uv);
		mesh.normals = Restore(normals);
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.Optimize();
		go.AddComponent<MeshFilter>().mesh = mesh;
		go.AddComponent<MeshRenderer>();
		return go;
	}

	private Vector3[] Restore(SVector3[] s)
	{
		Vector3[] v = new Vector3[s.Length];
		for (int i = 0; i < v.Length; i++)
		{
			v[i] = s[i].Restore();
		}
		return v;
	}

	private Vector2[] Restore(SVector2[] s)
	{
		Vector2[] v = new Vector2[s.Length];
		for (int i = 0; i < v.Length; i++)
		{
			v[i] = s[i].Restore();
		}
		return v;
	}
}