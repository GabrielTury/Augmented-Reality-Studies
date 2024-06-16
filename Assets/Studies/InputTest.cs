using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    // Start is called before the first frame update

    //public GameObject cube;

    public GameObject ball;

    public Transform racketTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            Debug.Log("LeftHand Trigger Pressed");
            Instantiate(ball, racketTransform.position, Quaternion.identity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    //public void ThumbsTest()
    //{
    //    Instantiate(cube, Camera.main.transform.position + Vector3.up*2, Quaternion.identity);
    //    
    //}
}
