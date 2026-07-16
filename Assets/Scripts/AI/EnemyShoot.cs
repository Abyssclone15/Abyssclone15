using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemyshoot : MonoBehaviour
{
    public GameObject enemyBullet;

    public Transform  spawnBulletPoint;

    private Transform playerPosition;

    public float bulletVelocity = 100;

    public float bulletInvoke = 3;

    public float bulletDestroy = 3;

    // Start is called before the first frame update
    void Start()
    {
        playerPosition = FindObjectOfType<PlayerScript>().transform;

        Invoke("ShootPlayer", bulletInvoke);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShootPlayer()
    {
        Vector3 playerDirection = (playerPosition.position - transform.position).normalized;

        GameObject newBullet;

        newBullet = Instantiate(enemyBullet, spawnBulletPoint.position, spawnBulletPoint.rotation);

        newBullet.GetComponent<Rigidbody>().AddForce(playerDirection*bulletVelocity*10f, ForceMode.Force);

        Destroy(newBullet, bulletDestroy);
        
        Invoke("ShootPlayer", bulletInvoke);
    }

}
