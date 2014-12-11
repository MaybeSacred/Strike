using System;
using UnityEngine;
using System.Collections.Generic;
public class Clusterer
{
	public Clusterer ()
	{
		
	}
	private static readonly float stoppingThreshold = .25f;
	private Vector3[] clusterCenters;
	private static readonly int maxIterations = 100;
	private int k;
	/**
     * @see func.FunctionApproximater#estimate(shared.DataSet)
     */
	public List<List<AttackableObject>> Estimate(List<AttackableObject> inObjects) {
		List<Vector3> set = new List<Vector3>(inObjects.Count);
		k = Mathf.CeilToInt((float)inObjects.Count/7);
		for(int i = 0; i < inObjects.Count; i++)
		{
			set.Add(inObjects[i].GetPosition());
		}
		int[] assignments = new int[set.Count];
		float lastSSE = float.PositiveInfinity;
		float currentSSE = float.PositiveInfinity;
		do
		{
			lastSSE = currentSSE;
			clusterCenters = new Vector3[k];
			// random initial centers
			for (int i = 0; i < clusterCenters.Length; i++) {
				/*int pick;
				do {
					pick = UnityEngine.Random.Range(0, set.Count);
				} while (assignments[pick] != 0);
				assignments[pick] = 1;*/
				clusterCenters[i] = set[i];
			}
			bool changed = false;
			int iterations = 0;
			// the main loop
			do {
				changed = false;
				// make the assignments
				for (int i = 0; i < set.Count; i++) {
					// find the closest center
					int closest = 0;
					double closestDistance = Vector3.Magnitude(set[i] - clusterCenters[0]);
					for (int j = 1; j < k; j++) {
						double distance = Vector3.Magnitude(set[i] - clusterCenters[j]);
						if (distance < closestDistance) {
							closestDistance = distance;
							closest = j;
						}
					}
					if (assignments[i] != closest) {
						changed = true;
					}
					assignments[i] = closest;
				}
				if (changed) {
					float[] assignmentCount = new float[k];
					// make the new clusters
					for (int i = 0; i < k; i++) {
						clusterCenters[i] = Vector3.zero;
					}
					for (int i = 0; i < set.Count; i++) {
						clusterCenters[assignments[i]] += set[i];
						assignmentCount[assignments[i]] ++;    
					}
					for (int i = 0; i < k; i++) {
						clusterCenters[i] *= (1/(assignmentCount[i] > 0? assignmentCount[i]:1));
					}
				}
				iterations++;
			} while (changed && iterations < maxIterations);
			currentSSE = 0;
			for(int i = 0; i < set.Count; i++)
			{
				currentSSE += (clusterCenters[assignments[i]] - set[i]).sqrMagnitude;
			}
			currentSSE /= set.Count;
			k++;
		}while(k < set.Count && lastSSE > currentSSE + stoppingThreshold);
		k--;
		List<List<AttackableObject>> outList = new List<List<AttackableObject>>();
		for(int i = 0; i < k; i++)
		{
			outList.Add(new List<AttackableObject>());
		}
		for(int i = 0; i < assignments.Length; i++)
		{
			outList[assignments[i]].Add(inObjects[i]);
		}
		return outList;
	}
	
	/**
     * Get the cluster centers
     * @return the cluster centers
     */
	public Vector3[] getClusterCenters() {
		return clusterCenters;
	}
}

