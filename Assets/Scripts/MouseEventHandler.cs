using UnityEngine;
using System;
using System.Collections.Generic;

public class MouseEventHandler : MonoBehaviour {
	private List<Instance> instances;
	private List<ReinforcementInstance> reinforcementInstances;
	private ARFFDataHandler trainingArff, testingArff, trainingReinforcementArff, testingReinforcementArff;
	void Start () {
		instances = new List<Instance>();
		reinforcementInstances = new List<ReinforcementInstance>();
		trainingArff = new ARFFDataHandler("C:\\Users\\Jon\\Documents\\Stryke\\Assets\\Data\\", LearnerUtilities.dataFileName, true, true);
		//trainingArff.CreateARFFFile();
		testingArff = new ARFFDataHandler("C:\\Users\\Jon\\Documents\\Stryke\\Assets\\Data\\", LearnerUtilities.eventDataFileName, true, true);
		testingArff.CreateARFFFile(new Instance(System.Enum.GetNames(typeof(UnitNames)).Length));
		trainingReinforcementArff = new ARFFDataHandler("C:\\Users\\Jon\\Documents\\Stryke\\Assets\\Data\\", LearnerUtilities.reinforcementDataFileName, true, true);
		testingReinforcementArff = new ARFFDataHandler("C:\\Users\\Jon\\Documents\\Stryke\\Assets\\Data\\", LearnerUtilities.reinforcementEventDataFileName, true, true);
		testingReinforcementArff.CreateARFFFile(new ReinforcementInstance(System.Enum.GetNames(typeof(UnitNames)).Length));
	}
	void Update () {
		if(Input.GetKeyDown("u"))
		{
			trainingArff.CreateARFFFile(new Instance(System.Enum.GetNames(typeof(UnitNames)).Length));
			testingArff.CreateARFFFile(new Instance(System.Enum.GetNames(typeof(UnitNames)).Length));
			trainingReinforcementArff.CreateARFFFile(new ReinforcementInstance(System.Enum.GetNames(typeof(UnitNames)).Length));
			testingReinforcementArff.CreateARFFFile(new ReinforcementInstance(System.Enum.GetNames(typeof(UnitNames)).Length));
		}
		if(Input.GetKeyDown("t"))
		{
			LearnerUtilities.TrainCurrentClassifier(LearnerUtilities.dataFileName);
		}
		if(Input.GetKeyDown("y"))
		{
			WriteInstances();
		}
	}
	public void StartTestInstance(Instance instance)
	{
		testingArff.ClearInstances();
		testingArff.WriteInstanceData(instance);
		LearnerUtilities.BeginProductionClassification(LearnerUtilities.eventDataFileName);
	}
	public void StartTestInstanceReinforcement()
	{
		testingReinforcementArff.ClearInstances();
		ReinforcementInstance[] outInstances = new ReinforcementInstance[27];
		Array names = System.Enum.GetValues(typeof(UnitNames));
		for(int i = 0; i < outInstances.Length; i++)
		{
			outInstances[i] = (ReinforcementInstance)InGameController.CreateInstance((UnitNames)names.GetValue(i), true);
		}
		testingReinforcementArff.WriteInstanceData(outInstances);
		LearnerUtilities.BeginProductionClassification(LearnerUtilities.reinforcementEventDataFileName);
	}
	public UnitNames CheckTestInstanceClassification()
	{
		return LearnerUtilities.CheckProductionClassification();
	}
	public List<UnitNames> CheckTestInstanceClassificationRanked()
	{
		return LearnerUtilities.CheckProductionClassificationRanked();
	}

	public List<UnitNames> CheckTestInstanceClassificationReinforcement ()
	{
		return LearnerUtilities.CheckProductionClassificationReinforcement();
	}

	public void AddInstance(UnitNames classification)
	{
		Instance instance = InGameController.CreateInstance(classification, false);
		instances.Add(instance);
	}
	public void AddReinforcementInstance(ReinforcementInstance ri)
	{
		reinforcementInstances.Add(ri);
	}
	public void WriteInstances()
	{
		trainingArff.WriteInstanceData(instances.ToArray());
	}
	public void WriteReinforcementInstances()
	{
		trainingReinforcementArff.WriteInstanceData(reinforcementInstances.ToArray());
	}
}
