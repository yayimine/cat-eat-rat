using Unity.VisualScripting;
using UnityEngine;

public class WaterBulletScript : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletDespawnTime;

    private Rigidbody rb;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
        Invoke(nameof(Despawn),bulletDespawnTime);
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Rat" && collision.gameObject.tag != "Display2" && collision.gameObject.tag!="Cat")
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
