using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public Tank tank;

    private Path path = new Path();

    public enum Status
    {
        Patrol,
        Attack,
    }
    private Status status = Status.Patrol;

    private GameObject target;
    private float sightDistance = 30f;
    private float lastSearchTime = 0;
    private float searchTargetInterval = 3;

    private float lastUpdateWaypointTime = float.MinValue;
    private float updateWaypointInterval = 10;

    void InitWaypoint()
    {
        GameObject obj = GameObject.Find("WaypointContainer");
        if(obj && obj.transform.GetChild(0) != null)
        {
            Vector3 targetPos = obj.transform.GetChild(0).position;
            path.InitByNavMeshPath(transform.position, targetPos);
        }
    }

    /*
    private void OnDrawGizmos()
    {
        path.DrawWaypoints();
    }
    */

    private void Start()
    {
        InitWaypoint();
    }

    public void ChangeStatus(Status status)
    {
        if (status == Status.Patrol)
            PatrolStart();
        else
            AttackStart();
    }
	
    void TargetUpdate()
    {
        float interval = Time.time - lastSearchTime;
        if (interval < searchTargetInterval)
            return;
        lastSearchTime = Time.time;
        if (target != null)
            HasTarget();
        else
            NoTarget();
    }

    void HasTarget()
    {
        Tank targetTank = target.GetComponent<Tank>();
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        if(targetTank.ctrlType == Tank.CtrlType.none)
        {
            Debug.Log("目标死亡，丢失目标");
            target = null;
        }
        else if(Vector3.Distance(pos,targetPos) > sightDistance)
        {
            Debug.Log("距离过远，丢失目标");
            target = null;
        }
    }

    void NoTarget()
    {
        float minHp = float.MaxValue;

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tank");
        for(int i = 0;i < targets.Length; i++)
        {
            Tank tank = targets[i].GetComponent<Tank>();
            if (tank == null)
                continue;
            if (targets[i] == gameObject)
                continue;
            if (Battle.instance.IsSameCamp(gameObject, targets[i]))
                continue;
            if (tank.ctrlType == Tank.CtrlType.none)
                continue;

            Vector3 pos = transform.position;
            Vector3 targetPos = targets[i].transform.position;
            if(Vector3.Distance(pos,targetPos) > sightDistance)
            {
                continue;
            }
            if (minHp > tank.hp)
            {
                target = tank.gameObject;
                minHp = tank.hp;
            }
        }
        if(target != null)
        {
            Debug.Log("获取目标 " + target.name);
        }
    }

    public void OnAttacked(GameObject attackTank)
    {
        if (Battle.instance.IsSameCamp(gameObject, attackTank))
            return;
        target = attackTank;
    }

	// Update is called once per frame
	void Update () {
        if (tank.ctrlType != Tank.CtrlType.computer)
            return;

        TargetUpdate();

        //行走
        if (path.IsReach(transform))
        {
            path.NextWaypoint();
        }

        if (status == Status.Patrol)
            PatrolUpdate();
        else if (status == Status.Attack)
            AttackUpdate();
	}

    public Vector3 GetTurretTarget()
    {
        if(target == null)
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }
        else
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
    }

    public float GetSteering()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        if (itp.x > path.deviation / 5)
            return tank.maxSteeringAngle;
        else if (itp.x < -path.deviation / 5)
            return -tank.maxSteeringAngle;
        else
            return 0;
    }

    public float GetMotor()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        float x = itp.x;
        float z = itp.z;
        float r = 6;

        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.maxMotorTorque;
        else
            return tank.maxMotorTorque;
    }

    public float GetBrakeTorque()
    {
        if (path.isFinish)
            return tank.maxMotorTorque;
        else
            return 0;
    }

    public bool IsShoot()
    {
        if (target == null)
            return false;
        float turretRoll = tank.turret.eulerAngles.y;
        float angle = turretRoll - GetTurretTarget().y;
        if (angle < 0) angle += 30;
        if (angle < 30 || angle > 330)
            return true;
        else
            return false;
    }

    void PatrolStart()
    {

    }

    void AttackStart()
    {
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    void PatrolUpdate()
    {
        if (target != null)
            ChangeStatus(Status.Attack);

        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval)
            return;
        lastUpdateWaypointTime = Time.time;

        if(path.waypoints == null || path.isFinish)
        {
            GameObject obj = GameObject.Find("WaypointContainer");
            {
                int count = obj.transform.childCount;
                if (count == 0) return;
                int index = Random.Range(0, count);
                Vector3 targetPos = obj.transform.GetChild(index).position;
                path.InitByNavMeshPath(transform.position, targetPos);
            }
        }
    }

    void AttackUpdate()
    {
        if(target == null)
        {
            ChangeStatus(Status.Patrol);
        }

        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval)
            return;
        lastUpdateWaypointTime = Time.time;
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }
}
