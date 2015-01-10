// //Created by Jon Tyson : jtyson3@gatech.edu
#define EXPORT_INSTANCE_COMMENTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
//using UnityEditor;
[System.Serializable]
public class ARFFDataHandler
{
	public string filename;
	public string dataPath;
	public bool autoOverwrite;
	public int exportBlockHeight;
	public int exportBlockWidth;
	public bool exportMoreReadable;
	/*Create sparse export function*/
	public ARFFDataHandler (string dPath, string fName, bool overwrite, bool exportReadable)
	{
		filename = fName;
		dataPath = dPath;
		autoOverwrite = overwrite;
		exportMoreReadable = exportReadable;
	}
	/// <summary>
	/// Creates header information for ARFF data file.
	/// </summary>
	/// <returns><c>true</c>, if ARFF file was created, <c>false</c> otherwise.</returns>
	/// <param name="filename">Filename.</param>
	public bool CreateARFFFile(Instance stringPrototype)
	{
		if(!autoOverwrite)
		{
			if(!File.Exists(dataPath + filename + ".arff"))
			{
				/*if(!EditorUtility.DisplayDialog("Warning", "Warning: File " + filename + " already exists\nOverwrite anyways?", "Overwrite", "Cancel"))
				{
					return false;
				}*/
			}
		}
		try
		{
			FileStream fs = File.Create(dataPath + filename + ".arff");
			BinaryWriter bw = new BinaryWriter(fs);
			string relation = "@relation \"Exported data block from Strike : " + DateTime.Now.ToString() + "\"\n"
				+ "%@attributes row, column\n";
			#if EXPORT_INSTANCE_COMMENTS
			relation += "%Pretty names for each instance are located after the first ARFF comment. Any additional comments should be placed after a second ARFF comment (%).\n";
			#endif
			string data = "@data\n";
			ASCIIEncoding asen = new ASCIIEncoding();
			byte[] ba = asen.GetBytes(relation);
			bw.Write(ba);
			ba = asen.GetBytes(stringPrototype.AttributeString());
			bw.Write(ba);
			ba = asen.GetBytes(data);
			bw.Write(ba);
			bw.Close();
			fs.Close();
		}
		catch (Exception e)
		{
			Debug.Log(e);
			return false;
		}
		return true;
	}
	/// <summary>
	/// Clears instances from ARFF data file
	/// </summary>
	/// <returns><c>true</c>, if instances were cleared successfully, <c>false</c> otherwise.</returns>
	/// <param name="filename">Filename.</param>
	public bool ClearInstances()
	{
		if(!File.Exists(dataPath + filename + ".arff"))
		{
			throw new FileNotFoundException(dataPath + filename + ".arff");
		}
		try
		{
			string tempFilename = dataPath + UnityEngine.Random.value.ToString() + ".arff";
			int lineNumber = 0;
			int linesRemoved = 0;
			using (var sr = new StreamReader(dataPath + filename + ".arff"))
			{
				using (var sw = new StreamWriter(tempFilename))
				{
					string line;
					while (!(line = sr.ReadLine()).Contains("@data"))
					{
						lineNumber++;
						sw.WriteLine(line);
					}
					sw.WriteLine("@data");
				}
			}
			File.Delete(dataPath + filename + ".arff");
			File.Move(tempFilename, dataPath + filename + ".arff");
		}
		catch (Exception e)
		{
			return false;
		}
		return true;
	}
	public bool WriteInstanceData(Instance inputInstance)
	{
		Instance[] ins = new Instance[]{inputInstance};
		return WriteInstanceData(ins);
	}
	public bool WriteInstanceData(Instance[] inputInstance)
	{
		if(!File.Exists(dataPath + filename + ".arff"))
		{
			CreateARFFFile(inputInstance[0]);
		}
		try
		{
			FileStream fs = File.OpenWrite(dataPath + filename + ".arff");
			fs.Seek(0, SeekOrigin.End);
			BinaryWriter bw = new BinaryWriter(fs);
			StringBuilder sb = new StringBuilder();
			for(int instance = 0; instance < inputInstance.Length; instance++)
			{
				//create function for this
				sb.Append(inputInstance[instance].CreateARFFDataString(exportMoreReadable));
			}
			ASCIIEncoding asen = new ASCIIEncoding();
			byte[] ba = asen.GetBytes(sb.ToString());
			bw.Write(ba);
			bw.Close();
			fs.Close();
		}
		catch (Exception e)
		{
			return false;
		}
		return true;
	}
}

