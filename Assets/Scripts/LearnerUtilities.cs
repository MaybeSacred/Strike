// //Created by Jon Tyson : jtyson3@gatech.edu

using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class LearnerUtilities
{
	private static String jrePath = "C:\\Program Files (x86)\\Java\\jre7\\bin\\javaw.exe";
	private static String wekaClassPath = "C:\\Program Files (x86)\\Weka-3-6\\weka.jar";
	private static String dataPath = Application.dataPath + @"\Data\";
	public static String dataFileName = "ProductionTrainingData";
	public static String reinforcementDataFileName = "ProductionTrainingReinforcementData";
	public static string reinforcementEventDataFileName = "ProductionEventReinforcementData";
	public static String eventDataFileName = "ProductionEventData";
	public static float learningRate;
	public static float learningMomentum;
	public static String currentClassifier = "weka.classifiers.trees.LADTree";
	private static Process exeProcess;
	private static float classificationThreshold = .075f;
	private static Process neuralNetTrainer;
	private static string logFile = "log.txt";
	public static void SetJREPath(String path)
	{
		LearnerUtilities.jrePath = path;
	}
	public static void SetWekaPath(String path)
	{
		LearnerUtilities.wekaClassPath = path;
	}
	public static void SetDataPath(String path)
	{
		LearnerUtilities.dataPath = path;
	}
	public static String GetJREPath()
	{
		return LearnerUtilities.jrePath;
	}
	public static String GetWekaPath()
	{
		return LearnerUtilities.wekaClassPath;
	}
	public static String GetDataPath()
	{
		return LearnerUtilities.dataPath;
	}
	public static void TrainCurrentClassifier(string file)
	{
		TrainLADTree(file);
	}
	/// <summary>
	/// Launch Weka with settings for Neural Net.
	/// </summary>
	private static void TrainBFTree(string file)
	{
		Process exeProcess = new Process();
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		//exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.meta.RandomCommittee -S 1 -I 10 -W weka.classifiers.trees.BFTree -- -S 1 -M 2 -N 5 -C 1.0 -P POSTPRUNED -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.trees.J48 -C 0.25 -M 2 -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.Start();
	}
	private static void TrainLADTree(string file)
	{
		Process exeProcess = new Process();
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.trees.LADTree -B 10 -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
		exeProcess.Start();
	}
	/// <summary>
	/// Launch Weka with settings for Neural Net.
	/// </summary>
	private static void TrainNeuralNet(string file)
	{
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			neuralNetTrainer = new Process();
			neuralNetTrainer.StartInfo.CreateNoWindow = false;
			neuralNetTrainer.StartInfo.UseShellExecute = false;
			neuralNetTrainer.StartInfo.FileName = jrePath;
			neuralNetTrainer.EnableRaisingEvents = true;
			neuralNetTrainer.Exited += new EventHandler(myProcess_Exited);
			neuralNetTrainer.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" weka.classifiers.functions.MultilayerPerceptron -t " + LearnerUtilities.dataPath + file + ".arff -k -d " + LearnerUtilities.dataPath + file + ".model";
			neuralNetTrainer.Start();
			WriteIDToFile(neuralNetTrainer.Id);
		}
	}
	private static void WriteIDToFile(int id)
	{
		try
		{
			FileStream fs = File.Create(dataPath + logFile);
			BinaryWriter bw = new BinaryWriter(fs);
			string data = id.ToString();
			ASCIIEncoding asen = new ASCIIEncoding();
			byte[] ba = asen.GetBytes(data);
			bw.Write(ba);
			bw.Close();
			fs.Close();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.Log(e);
		}
	}
	private static int ReadIDFromFile()
	{
		using (var sr = new StreamReader(dataPath + logFile))
		{
			string line = sr.ReadLine();
			return int.Parse(line);
		}
	}
	public static void TerminateNeuralTraining()
	{
		Process[] processes = Process.GetProcesses();
		int id = ReadIDFromFile();
		for(int i = 0; i < processes.Length; i++)
		{
			try
			{
				if(processes[i].Id == id)
				{
					processes[i].Kill();
				}
			}
			catch (InvalidOperationException e)
			{
				
			}
		}
	}
	private static void myProcess_Exited(object sender, System.EventArgs e)
	{
		UnityEngine.Debug.Log("Neural net ended");
	}
	
	public static void BeginProductionClassification(string file)
	{
		exeProcess = new Process();
		exeProcess.StartInfo.RedirectStandardError = true;
		exeProcess.StartInfo.RedirectStandardOutput = true;
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" " + LearnerUtilities.currentClassifier + " -l " + LearnerUtilities.dataPath + LearnerUtilities.dataFileName + ".model -T " + LearnerUtilities.dataPath + LearnerUtilities.eventDataFileName + ".arff -p 0 -distribution";
		exeProcess.Start();
	}
	public static void BeginProductionReinforcementClassification(string file)
	{
		exeProcess = new Process();
		exeProcess.StartInfo.RedirectStandardError = true;
		exeProcess.StartInfo.RedirectStandardOutput = true;
		exeProcess.StartInfo.CreateNoWindow = false;
		exeProcess.StartInfo.UseShellExecute = false;
		exeProcess.StartInfo.FileName = jrePath;
		exeProcess.StartInfo.Arguments = "-Dfile.encoding-Cp1252 -classpath \"" + LearnerUtilities.wekaClassPath + "\" " + LearnerUtilities.currentClassifier + " -l " + LearnerUtilities.dataPath + LearnerUtilities.reinforcementDataFileName + ".model -T " + LearnerUtilities.dataPath + LearnerUtilities.reinforcementEventDataFileName + ".arff -p 0 -distribution";
		exeProcess.Start();
	}
	public static UnitNames CheckProductionClassification()
	{
		if(exeProcess.HasExited)
		{
			return GetUnitNameFromWekaString(exeProcess.StandardOutput.ReadToEnd());
		}
		return UnitNames.Headquarters;
	}
	public static List<UnitNames> CheckProductionClassificationRanked()
	{
		if(exeProcess.HasExited)
		{
			return GetUnitNamesFromWekaString(exeProcess.StandardOutput.ReadToEnd());
		}
		return null;
	}

	public static List<UnitNames> CheckProductionClassificationReinforcement ()
	{
		if(exeProcess.HasExited)
		{
			return GetUnitNamesFromWekaString(exeProcess.StandardOutput.ReadToEnd());
		}
		return null;
	}

	private static UnitNames GetUnitNameFromWekaString(string input)
	{
		String[] split = input.Split(":".ToCharArray(), StringSplitOptions.None);
		split = split[2].Split(" ".ToCharArray(), StringSplitOptions.None);
		split[0] = split[0].Trim(" ".ToCharArray());
		return GetUnitNameFromString(split[0]);
	}
	private static List<UnitNames> GetUnitNamesFromWekaString(string input)
	{
		UnityEngine.Debug.Log(input);
		String[] split = input.Split(" ".ToCharArray(), StringSplitOptions.None);
		split = split[split.Length - 2].Split(new string[]{",", "*"}, StringSplitOptions.RemoveEmptyEntries);
		return ParseUnitNames(split);
	}
	private static List<UnitNames> ParseUnitNames(string[] inArray)
	{
		Array values = System.Enum.GetValues(typeof(UnitNames));
		List<UnitNames> likeliestValues = new List<UnitNames>();
		for(int i = 0; i < inArray.Length; i++)
		{
			if(float.Parse(inArray[i]) >= classificationThreshold)
			{
				likeliestValues.Add((UnitNames)values.GetValue(i));
			}
		}
		return likeliestValues;
	}
	private static UnitNames GetUnitNameFromString(string input)
	{
		foreach(string name in System.Enum.GetNames(typeof(UnitNames)))
		{
			if(name.Contains(input))
			{
				return (UnitNames)System.Enum.Parse(typeof(UnitNames), name, true);
			}
		}
		throw new Exception();
	}
}

