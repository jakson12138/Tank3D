using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float speed = 30f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;

    public GameObject attackTank;

	// Use this for initialization
	void Start () {
        instantiateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {

        transform.position += transform.forward * speed * Time.deltaTime;

		if(Time.time - instantiateTime > maxLiftTime)
        {
            Destroy(gameObject);
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == attackTank)
            return;

        Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);

        Tank tank = collision.gameObject.GetComponent<Tank>();
        if(tank != null)
        {
            float att = GetAtt();
            tank.BeAttacked(att,attackTank);
        }
    }

    private float GetAtt()
    {
        float att = 100 - (Time.time - instantiateTime) * 40;
        if (att < 1) att = 1;
        return att;
    }

}
