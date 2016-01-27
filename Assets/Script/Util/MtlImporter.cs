using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

/// <summary>
/// add the texture to a game object from a .mtl file
/// the .mlt and texture need to be downloaded
/// <see cref="Data.cs"/>
/// </summary>
[Serializable]
public class MtlImporter: MonoBehaviour
{
	/// <summary>
	/// the game object of the mesh, the name of the material mesh
	/// </summary>
	public Dictionary<string, List<GameObject>> meshes { get; set; }
	//public Dictionary<GameObject, string> meshes { get; set; }
	/// <summary>
	/// file name of the .mtl
	/// </summary>
	public string fileName { get; set; }
	/// <summary>
	/// name of a texture, file name of the texture
	/// </summary>
	private Dictionary<string, string> matNameFile = new Dictionary<string, string>();
	/// <summary>
	/// name of a texture, tint of a texture
	/// </summary>
	private Dictionary<string, Color> matNameColor = new Dictionary<string, Color>();
	/// <summary>
	/// shader for non transparent texture
	/// </summary>
	private Shader normal;
	/// <summary>
	/// shader for transparent texture
	/// </summary>
	private Shader transparent;

	private Data data;

	/// <summary>
	/// add the texture to the game object and his child
	/// </summary>
	public void AddTexture()
	{
		normal = Shader.Find("Diffuse");
		transparent = Shader.Find("Transparent/Diffuse");
		data = ToolBox.Instance.data;

		if (fileName != null && data.mtl.ContainsKey(fileName))
		{
			ReadMtl(data.mtl[fileName]);
			foreach (KeyValuePair<string, string> mat in matNameFile)
			{
				if (meshes.ContainsKey(mat.Key))
				{
					Material material;
					if (matNameColor[mat.Key].a < 1)
					{
						material = new Material(transparent);
					}
					else
					{
						material = new Material(normal);
					}
					material.color = matNameColor[mat.Key];
					if (data.textures.ContainsKey(mat.Value))
					{
						material.mainTexture = data.textures[mat.Value];
					}
					foreach (GameObject g in meshes[mat.Key])
					{
						g.GetComponent<Renderer>().material = material;
					}
				}
			}
		}
	}

	/// <summary>
	/// read the .mtl and set all the variable
	/// </summary>
	/// <param name="mtl">the .mtl file</param>
	private void ReadMtl(string mtl)
	{
		using (StringReader reader = new StringReader(mtl))
		{
			string line = reader.ReadLine();
			string[] splitLine;
			string mtlName = "";
			float alpha = 1;
			string fileName = "";

			while (line != null)
			{
				if (line != "")
				{
					if (line.StartsWith("\t"))
					{
						line = line.Substring(1);
					}
					line.Replace("  ", " ");
					splitLine = line.Split(' ');

					if (splitLine.Length >= 2)
					{
						switch (splitLine[0])
						{
						case "newmtl":
							if (mtlName != "")
							{
								matNameFile.Add(mtlName, fileName);
								matNameColor.Add(mtlName, new Color(1, 1, 1, alpha));
							}
							fileName = "";
							mtlName = splitLine[1];
							break;
						case "map_Kd":
							fileName = splitLine[1];
							break;
						case "d":
						case "tr":
							if (!float.TryParse(splitLine[1], out alpha))
							{
								alpha = 1;
							}
							break;
						}
					}
				}
				line = reader.ReadLine();
			}
			matNameFile.Add(mtlName, fileName);
			matNameColor.Add(mtlName, new Color(1, 1, 1, alpha));
		}
	}

	public static List<string> GetTexturesFileName(string mtlFile)
	{
		List<string> textures = new List<string>();

		string[] splitFile = mtlFile.Split(null);

		for (int i = 0; i < splitFile.Length; i++)
		{
			if (splitFile[i] == "map_Kd")
			{
				i++;
				if (i < splitFile.Length)
				{
					textures.Add(splitFile[i]);
				}
			}
		}

		return textures;
	}
}