using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Componenets")]
    private Rigidbody rb;
    private Animator animator;

    [Header("InputActions")]
    public InputActionReference MoveForward;
    public InputActionReference Turn;
    public InputActionReference Launch;
    public InputActionReference Jump;
    public InputActionReference Slam;

    [Header("Movements")]
    private bool isMoving;
    public float forceAmount;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
          if(MoveForward.action.WasPressedThisFrame())
        {
            rb.AddForce(transform.forward * forceAmount);
        }
    }

    
}
