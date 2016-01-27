//http://wiki.unity3d.com/index.php?title=ObjImporter
/* This version of ObjImporter first reads through the entire file, getting a count of how large
 * the final arrays will be, and then uses standard arrays for everything (as opposed to ArrayLists
 * or any other fancy things). 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

/// <summary>
/// Create a game object with a custom mesh from a text file
/// the game object can have multiple mesh on child
/// can only read .obj file
/// will also put the component MtlImporter to import the texture on the mesh
/// </summary>
public class ObjImporter
{
	public struct ObjStruct
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uv;
		public List<int[]> triangles;
		public int[] faceVerts;
		public int[] faceUVs;
		public List<Vector3[]> faceData;
		public string name;
		public string mtlFileName;
	}

	public class MeshStruct
	{
		public Vector3[] vertices { get; set; }
		public Vector2[] uv { get; set; }
		public Vector3[] normals { get; set; }
		public int[] triangles { get; set; }
		public string mtlName { get; set; }
		public MeshStruct(string name)
		{
			mtlName = name;
		}
	}

	/// <summary>
	/// Mesh on unity have a maximum number of faces
	/// </summary>
	private const int MESH_MAX_FACES = 65000;
	/// <summary>
	/// Structure of the obj file
	/// </summary>
	private ObjStruct objStruct;
	private List<MeshStruct> allMesh = new List<MeshStruct>();
	/// <summary>
	/// the file text
	/// </summary>
	private string fileText;
	/// <summary>
	/// Name of the mtl file
	/// </summary>
	private string mtlFileName;
	private bool canFinish = false;

	public IEnumerator StartImportation(string file)
	{
		fileText = file;
		allMesh.Clear();
		objStruct = new ObjStruct();
		mtlFileName = "";
		canFinish = false;

		try
		{
			InitMestStruct();
			PopulateMeshStruct();
			CreateMeshStruct();
			canFinish = true;
		}
		catch (Exception e)
		{
#if LOG
			Debug.Log(e.ToString());
#endif
		}
		yield return 0;
	}

	/// <summary>
	/// read the text file and init the variable of the mesh.
	/// </summary>
	private void InitMestStruct()
	{
		List<int> triangles = new List<int>();
		List<int> face = new List<int>();
		triangles.Add(0);
		face.Add(0);
		int vertices = 0;
		int vt = 0;
		int vn = 0;
		fileText = fileText.Replace("  ", " ");

		//if the file only have \n as end line, it slows the StreamReader really badly when you have 1 000 000 lines to read.
		if (!fileText.Contains("\r\n"))
		{
			if (fileText.Contains("\n"))
			{
				fileText = fileText.Replace("\n", "\r\n");
			}
			else if (fileText.Contains("\r"))
			{
				fileText = fileText.Replace("\r", "\r\n");
			}
		}


		using (StringReader reader = new StringReader(fileText))
		{
			int i = 0;
			string currentText = reader.ReadLine();
			char[] splitIdentifier = { ' ' };
			string[] brokenString;
			while (currentText != null)
			{
				if (currentText.StartsWith("f ") || currentText.StartsWith("v ") || currentText.StartsWith("vt ")
					|| currentText.StartsWith("vn ") || currentText.StartsWith("usemtl "))
				{
					currentText = currentText.Trim();                           //Trim the current line
					brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
					switch (brokenString[0])
					{
					case "v":
						vertices++;
						break;
					case "vt":
						vt++;
						break;
					case "vn":
						vn++;
						break;
					case "f":
						face[i] = face[i] + brokenString.Length - 1;
						triangles[i] = triangles[i] + 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertice, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
						break;
					case "usemtl":
						triangles.Add(0);
						face.Add(0);
						allMesh.Add(new MeshStruct(brokenString[1]));
						i++;
						break;
					}
				}
				currentText = reader.ReadLine();
			}
		}
		objStruct.vertices = new Vector3[vertices];
		objStruct.uv = new Vector2[vt];
		objStruct.normals = new Vector3[vn];
		objStruct.triangles = new List<int[]>();
		objStruct.faceData = new List<Vector3[]>();

		int k = 0;
		for (int j = 0; j < face.Count; j++)
		{
			if (face[j] > 0)
			{
				if (face[j] <= MESH_MAX_FACES)
				{
					objStruct.triangles.Add(new int[triangles[j]]);
					objStruct.faceData.Add(new Vector3[face[j]]);
					k++;
				}
				else
				{
					throw new Exception("Mesh too large");
				}

			}
		}

	}

	/// <summary>
	/// set all the variable for the mesh.
	/// </summary>
	private void PopulateMeshStruct()
	{
		fileText = fileText.Replace("#QNAN", "0");
		using (StringReader reader = new StringReader(fileText))
		{
			string currentText = reader.ReadLine();

			char[] splitIdentifier = { ' ' };
			char[] splitIdentifier2 = { '/' };
			string[] brokenString;
			string[] brokenBrokenString;
			int f = 0;
			int f2 = 0;
			int v = 0;
			int vn = 0;
			int vt = 0;
			int vt1 = 0;
			int vt2 = 0;
			int group = -1;
			while (currentText != null)
			{
				if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
					!currentText.StartsWith("vn ") && !currentText.StartsWith("usemtl ") && !currentText.StartsWith("usemap ") &&
					!currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
					!currentText.StartsWith("vc "))// && !currentText.StartsWith("o ") && !currentText.StartsWith("g "))
				{
					currentText = reader.ReadLine();
				}
				else
				{
					currentText = currentText.Trim();
					brokenString = currentText.Split(splitIdentifier, 50);
					switch (brokenString[0])
					{
					//case "g":
					//case "o":

					//	break;
					case "usemtl":
						group++;
						f = 0;
						f2 = 0;
						//if (!childsNameMtlName.ContainsKey(childsName[group]))
						//{
						//	childsNameMtlName.Add(childsName[group], brokenString[1]);
						//}
						break;
					case "usemap":
						break;
					case "mtllib":
						mtlFileName = brokenString[1];
						DownloadMtl(mtlFileName);
						break;
					case "v":
						objStruct.vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
												 System.Convert.ToSingle(brokenString[3]));
						v++;
						break;
					case "vt":
						objStruct.uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
						vt++;
						break;
					case "vt1":
						objStruct.uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
						vt1++;
						break;
					case "vt2":
						objStruct.uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
						vt2++;
						break;
					case "vn":
						objStruct.normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
												System.Convert.ToSingle(brokenString[3]));
						vn++;
						break;
					case "vc":
						break;
					case "f":
						if (group == -1)
						{
							allMesh.Add(new MeshStruct("Polygone1"));
							group = 0;
						}
						int j = 1;
						List<int> intArray = new List<int>();
						while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
						{
							Vector3 temp = new Vector3();
							brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
							temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
							if (brokenBrokenString.Length >= 3)
							{
								if (brokenBrokenString[1] != "")
								{
									temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
								}
								temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
							}
							else if (brokenBrokenString.Length >= 2)
							{
								temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
							}

							j++;

							objStruct.faceData[group][f2] = temp;
							intArray.Add(f2);
							f2++;
						}
						j = 1;
						while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
						{
							objStruct.triangles[group][f] = intArray[0];
							f++;
							objStruct.triangles[group][f] = intArray[j];
							f++;
							objStruct.triangles[group][f] = intArray[j + 1];
							f++;

							j++;
						}
						break;
					}
					currentText = reader.ReadLine();
				}
			}
		}
	}

	private void CreateMeshStruct()
	{
		for (int i = 0; i < objStruct.faceData.Count; i++)//foreach (Vector3[] f in objStruct.faceData)
		{
			Vector3[] f = objStruct.faceData[i];

			allMesh[i].vertices = new Vector3[f.Length];
			allMesh[i].uv = new Vector2[f.Length];
			allMesh[i].normals = new Vector3[f.Length];
			allMesh[i].triangles = objStruct.triangles[i];

			int j = 0;
			/* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
			 * for the appropriate Unity mesh array.
			 */
			foreach (Vector3 v in f)
			{
				allMesh[i].vertices[j] = objStruct.vertices[(int)v.x - 1];

				if (v.y >= 1)
				{
					allMesh[i].uv[j] = objStruct.uv[(int)v.y - 1];
				}

				if (v.z >= 1)
				{
					allMesh[i].normals[j] = objStruct.normals[(int)v.z - 1];
				}
				j++;
			}
		}
	}

	public GameObject FinishMesh()
	{
		if (canFinish)
		{
			try
			{
				Dictionary<string, List<GameObject>> mtlNameMeshes = new Dictionary<string, List<GameObject>>();
				GameObject obj = new GameObject("imported");
				List<GameObject> childs = new List<GameObject>();
				int group = 0;
				foreach (MeshStruct m in allMesh)
				{
					Mesh mesh = new Mesh();

					mesh.vertices = m.vertices;
					mesh.uv = m.uv;
					mesh.normals = m.normals;
					mesh.triangles = m.triangles;

					mesh.RecalculateBounds();
					mesh.Optimize();

					childs.Add(new GameObject(m.mtlName));
					childs[group].AddComponent<MeshRenderer>();
					childs[group].AddComponent<MeshFilter>().mesh = mesh;
					childs[group].transform.SetParent(obj.transform);

					if (mtlNameMeshes.ContainsKey(m.mtlName))
					{
						mtlNameMeshes[m.mtlName].Add(childs[group]);
					}
					else
					{
						List<GameObject> l = new List<GameObject>();
						l.Add(childs[group]);
						mtlNameMeshes.Add(m.mtlName, l);
					}
					group++;
				}

				obj.AddComponent<MtlImporter>().meshes = mtlNameMeshes;
				obj.GetComponent<MtlImporter>().fileName = mtlFileName;

				return obj;
			}
			catch (Exception e)
			{
#if LOG
				Debug.Log(e.ToString());
#endif
				return null;
			}
		}
		return null;
	}

	private void DownloadMtl(string fileName)
	{
		Element ele = ToolBox.Instance.data.attraction.ContainFileName(fileName);
		if (ele != null)
		{
			ToolBox.Instance.download.EnqueueElement(ele, DownloadType.Element);
		}
	}
}