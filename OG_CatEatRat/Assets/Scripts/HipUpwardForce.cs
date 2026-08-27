using UnityEngine;

public class HipUpwardForce : MonoBehaviour
{
    private Rigidbody rb;
    public float force;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(Vector3.up * force);
    }
}
