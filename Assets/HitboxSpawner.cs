using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxSpawner : MonoBehaviour
{
    [SerializeField]
    private HitboxFollower hitboxFollower;
    void Start()
    {
        var follower = Instantiate(hitboxFollower);
        follower.transform.position = transform.position;

        follower.SetTarget(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
