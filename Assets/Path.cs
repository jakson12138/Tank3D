using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path : MonoBehaviour {

    public Vector3[] waypoints;
    public int index = -1;
    public Vector3 waypoint;
    bool isLoop = false;
    public float deviation = 5;
    public bool isFinish = false;

    public void DrawWaypoints()
    {
        if (waypoint == null)
            return;
        Debug.Log("waypoints:" + waypoints);
        int length = waypoints.Length;
        for (int i = 0; i < length; i++)
            if (i == index)
                Gizmos.DrawSphere(waypoints[i], 1);
            else
                Gizmos.DrawCube(waypoints[i], Vector3.one);
    }

    public void InitByNavMeshPath(Vector3 pos,Vector3 targetPos)
    {
        waypoints = null;
        index = -1;
        NavMeshPath navPath = new NavMeshPath();
        bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);

        if (!hasFoundPath)
            return;
        int length = navPath.corners.Length;
        waypoints = new Vector3[length];
        for(int i = 0;i < length; i++)
        {
            waypoints[i] = navPath.corners[i];
        }

        index = 0;
        waypoint = waypoints[index];
        isFinish = false;
    }

    public bool IsReach(Transform trans)
    {
        Vector3 pos = trans.position;
        float distance = Vector3.Distance(waypoint, pos);
        return distance < deviation;
    }

    public void NextWaypoint()
    {
        if (index < 0)
            return;
        if (index < waypoints.Length - 1)
            index++;
        else
        {
            if (isLoop)
                index = 0;
            else isFinish = true;
        }
        waypoint = waypoints[index];
    }

    public void InitByObj(GameObject obj,bool isLoop = false)
    {
        int length = obj.transform.childCount;
        if(length == 0)
        {
            waypoints = null;
            index = -1;
            Debug.LogWarning("Path.InitByObj.Length == 0");
            return;
        }

        waypoints = new Vector3[length];
        for(int i = 0;i < length; i++)
        {
            Transform trans = obj.transform.GetChild(i);
            waypoints[i] = trans.position;
        }
        index = 0;
        waypoint = waypoints[index];
        this.isLoop = isLoop;
        isFinish = false;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
