using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSpawner : MonoBehaviour
{
    public GameObject partPrefab;
    [Range(1, 10)] public int length;
    public float partDist;
    public bool isHorizontal;

    private const float slack = 0.5f;
    private GameObject plug;

    void Start()
    {
        plug = transform.Find("Plug").gameObject;
        Spawn();
    }

    void Update()
    {
        // prevent pulling plug past wire length
        OVRGrabbable grabbable = plug.GetComponent<OVRGrabbable>();
        float dist = (plug.transform.position - transform.position).magnitude;

        if (grabbable.isGrabbed && dist > (1 + slack) * length)
            grabbable.grabbedBy.ForceRelease(grabbable);
    }

    private void Spawn()
    {
        int count = (int)(length / partDist);

        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            Quaternion rotation;
            if (isHorizontal)
            {
                // robot start room wire
                position = new Vector3(transform.position.x, transform.position.y, transform.position.z - partDist * (i + 1));
                rotation = Quaternion.Euler(90, 0, 0);
            }
            else
            {
                // human start room wires
                position = new Vector3(transform.position.x, transform.position.y - partDist * (i + 1), transform.position.z);
                rotation = Quaternion.identity;
            }

            GameObject capsule = Instantiate(partPrefab, position, rotation, transform);
            capsule.name = (i).ToString();

            if (i == 0)
            {
                Destroy(capsule.GetComponent<FixedJoint>());
                capsule.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                capsule.GetComponent<FixedJoint>().connectedBody = transform.Find((i - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        // attach plug
        plug.GetComponent<FixedJoint>().connectedBody = transform.Find((count - 1).ToString()).GetComponent<Rigidbody>();
    }
}