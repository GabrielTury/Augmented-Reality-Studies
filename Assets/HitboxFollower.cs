using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxFollower : MonoBehaviour
{
    HitboxSpawner targetToFollow;

    private Rigidbody rdb;

    private Vector3 velocity;

    [SerializeField]
    private float sensitivity;

    // Start is called before the first frame update

    private void Awake()
    {
        rdb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 destination = targetToFollow.transform.position;
        rdb.transform.rotation = transform.rotation;

        velocity = (destination - rdb.transform.position) * sensitivity;

        rdb.velocity = velocity;
        transform.rotation = targetToFollow.transform.rotation;
    }

    public void SetTarget(HitboxSpawner target)
    {
        targetToFollow = target;
    }
}
