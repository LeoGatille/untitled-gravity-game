using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform reference;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = reference.position + new Vector3(0, 10, -5);
    }
}
