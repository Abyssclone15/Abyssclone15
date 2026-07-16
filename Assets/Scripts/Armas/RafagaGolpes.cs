using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RafagaGolpes : MonoBehaviour
{
    //Bergazos
    public Transform PunchSpawnPoint;

    public GameObject bullet;

    public float shotForce = 3500f;

    public float shotRate = 0.5f;

    private float shotRateTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time > shotRateTime && GameManager.Instance.gunAmmo > 0)
            {
                GameManager.Instance.gunAmmo--;

                GameObject newBullet;
                
                newBullet = Instantiate(bullet, PunchSpawnPoint.position, PunchSpawnPoint.rotation);

                newBullet.GetComponent<Rigidbody>().AddForce(PunchSpawnPoint.forward*shotForce);

                shotRateTime = Time.time + shotRate;

                Destroy(newBullet, 0.2f);
            }
        }

    }
}
