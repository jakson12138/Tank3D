using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Tank : MonoBehaviour
{
    private AI ai;
    private bool isWin = false;

    //炮塔炮管轮子履带
    public Transform turret;
    public Transform gun;
    private Transform wheels;
    private Transform tracks;
    //炮塔旋转速度
    private float turretRotSpeed = 2f;
    //炮塔炮管目标角度
    private float turretRotTarget = 0;
    private float turretRollTarget = 0;
    //炮管的旋转范围
    private float maxRoll = 10f;
    private float minRoll = -4f;


    //轮轴
    public List<AxleInfo> axleInfos;
    //马力/最大马力
    private float motor = 0;
    public float maxMotorTorque;
    //制动/最大制动
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    //转向角/最大转向角
    private float steering = 0;
    public float maxSteeringAngle;


    //马达音源
    public AudioSource motorAudioSource;
    //马达音效
    public AudioClip motorClip;

    public GameObject bullet;
    public float lastShootTime = 0;
    private float shootInterval = 0.5f;

    public enum CtrlType
    {
        none,
        computer,
        player
    }
    public CtrlType ctrlType = CtrlType.player;

    private float maxHp = 100;
    public float hp = 100;

    public GameObject destroyEffect;

    public Texture2D centerSight;
    public Texture2D tankSight;

    public Texture2D hpBarBg;
    public Texture2D hpBar;

    private void OnGUI()
    {
        if(ctrlType != CtrlType.player)
        {
            return;
        }
        DrawSight();
        DrawHp();
        DrawKillUI();
    }

    public void DrawHp()
    {
        Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15, hpBarBg.width, hpBarBg.height);
        GUI.DrawTexture(bgRect, hpBarBg);

        float width = hp * 102 / maxHp;
        Rect hpRect = new Rect(bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
        GUI.DrawTexture(hpRect, hpBar);

        string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
        Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
        GUI.Label(textRect, text);
    }

    public void DrawSight()
    {
        Vector3 explodePoint = CalExplodePoint();
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);

        Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2, Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);

        Rect centerRect = new Rect(Screen.width / 2 - centerSight.width / 2, Screen.height / 2 - centerSight.height / 2, centerSight.width, centerSight.height);
        GUI.DrawTexture(centerRect, centerSight);
    }

    public void BeAttacked(float att, GameObject attackTank)
    {
        if(hp <= 0)
        {
            return;
        }
        if(hp > 0)
        {
            hp -= att;
            if (ai != null)
            {
                ai.OnAttacked(attackTank);
            }
        }
        if (hp <= 0)
        {
            GameObject destroyObj = (GameObject)Instantiate(destroyEffect);
            destroyObj.transform.SetParent(transform, false);
            destroyObj.transform.localPosition = Vector3.zero;
            ctrlType = CtrlType.none;

            if (attackTank != null)
            {
                Tank tankCmp = attackTank.GetComponent<Tank>();
                if (tankCmp != null && tankCmp.ctrlType == CtrlType.player)
                {
                    tankCmp.StartDrawKill();
                }
            }

            isWin = Battle.instance.IsWin(attackTank);
        }
    }

    public Texture2D killUI;
    private float killUIStartTime = float.MinValue;

    public void StartDrawKill()
    {
        killUIStartTime = Time.time;
    }

    private void DrawKillUI()
    {
        if(Time.time - killUIStartTime < 1f)
        {
            Rect rect = new Rect(Screen.width / 2 - killUI.width / 2, 30, killUI.width, killUI.height);
            GUI.DrawTexture(rect, killUI);
        }
    }

    public void Shoot()
    {
        if(Time.time - lastShootTime < shootInterval)
        {
            return;
        }
        if(bullet == null)
        {
            return;
        }
        Vector3 pos = gun.position + gun.forward * 5;
        GameObject bulletObj = (GameObject)Instantiate(bullet, pos, gun.rotation);
        Bullet bulletCmp = bulletObj.GetComponent<Bullet>();
        if(bulletCmp != null)
        {
            bulletCmp.attackTank = this.gameObject;
        }
        lastShootTime = Time.time;

        //BeAttacked(30);
    }

    public void TargetSignPos()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;

        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);

        if(Physics.Raycast(ray,out raycastHit, 400.0f))
        {
            hitPoint = raycastHit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }

        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;

        //Transform targetCube = GameObject.Find("TargetCube").transform;
        //targetCube.position = hitPoint;
    }

    public Vector3 CalExplodePoint()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;

        Vector3 pos = gun.position + gun.forward * 5;
        Ray ray = new Ray(pos, gun.forward);
        if(Physics.Raycast(ray,out hit, 400f))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }

        return hitPoint;
    }

    //玩家控制
    public void PlayerCtrl()
    {

        if(ctrlType != CtrlType.player)
        {
            return;
        }

        //马力和转向角
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        //制动
        brakeTorque = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)  //前进时，按下“下”键
                brakeTorque = maxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)  //后退时，按下“上”键
                brakeTorque = maxBrakeTorque;
            continue;
        }
        //炮塔炮管角度
        TargetSignPos();

        if (Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    public void ComputerCtrl()
    {
        if(ctrlType != CtrlType.computer)
        {
            return;
        }
        Vector3 rot = ai.GetTurretTarget();
        turretRotTarget = rot.y;
        turretRollTarget = rot.x;
        steering = ai.GetSteering();
        motor = ai.GetMotor();
        brakeTorque = ai.GetBrakeTorque();

        if (ai.IsShoot())
            Shoot();
    }

    public void NoneCtrl()
    {
        if(ctrlType != CtrlType.none)
        {
            return;
        }
        motor = 0;
        steering = 0;
        brakeTorque = maxBrakeTorque / 2;
    }

    //开始时执行
    void Start()
    {
        if(ctrlType == CtrlType.computer)
        {
            ai = gameObject.AddComponent<AI>();
            ai.tank = this;
        }

        //获取炮塔
        turret = transform.Find("turret");
        //获取炮管
        gun = turret.Find("gun");
        //获取轮子
        wheels = transform.Find("wheels");
        //获取履带
        tracks = transform.Find("tracks");
        //马达音源
        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;
    }

    //每帧执行一次
    void Update()
    {

        //玩家控制操控
        PlayerCtrl();
        ComputerCtrl();
        NoneCtrl();
        //遍历车轴
        foreach (AxleInfo axleInfo in axleInfos)
        {
            //转向
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            //马力
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            //制动
            if (true)
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
            //转动轮子履带
            if (axleInfos[1] != null && axleInfo == axleInfos[1])
            {
                WheelsRotation(axleInfos[1].leftWheel);
                TrackMove();
            }
        }

        //炮塔炮管旋转
        TurretRotation();
        TurretRoll();
        //马达音效
        MotorSound();
    }

    //炮塔旋转
    public void TurretRotation()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;

        //归一化角度
        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;

        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    //炮管旋转
    public void TurretRoll()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;
        //获取角度
        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;
        //世界坐标系角度计算
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;
        //本地坐标系角度限制
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;

        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }

    //轮子旋转
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        //获取旋转信息
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        //旋转每个轮子
        foreach (Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }


    //履带滚动
    public void TrackMove()
    {
        if (tracks == null)
            return;

        float offset = 0;
        if (wheels.GetChild(0) != null)
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;

        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
    }

    //马达音效
    void MotorSound()
    {
        if (motor != 0 && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorClip;
            motorAudioSource.Play();
        }
        else if (motor == 0)
        {
            motorAudioSource.Pause();
        }
    }
}