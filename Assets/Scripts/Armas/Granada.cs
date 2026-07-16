using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granada : MonoBehaviour
{
    public float delay = 1;

    float countdown;

    public float radius = 5;

    public float explosionForce = 70;

    bool exploded = false;

    public GameObject explosionEffect;

    // Start is called before the first frame update
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;

        if (countdown <= 0 && !exploded)
        {
            Explode();
            exploded = true;
        }
    }

    void Explode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (var rangedObjects in colliders)
        {
            Rigidbody rb = rangedObjects.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply explosion force
                rb.AddExplosionForce(explosionForce*10f, transform.position, radius);
            }
        }
        Destroy(gameObject);
    }
}
