using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using System.Security.Cryptography;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine.Experimental.GlobalIllumination;
using System;
using JetBrains.Annotations;
//using UnityEditor.Rendering;

public class CatMovement : MonoBehaviour
{
    //[Header("Componenets")]
    private Rigidbody rb;
    public Animator animator;


    [Header("GroundMovements")]
    //ForawrdMovement
    private bool isMoving;
    public float forceAmount;
    public float leapForce;
    public int jumpAmount;
    public float midAirMoveForce;


    [Header("Rotations")]
    //Horizontal Rotation
    public float horizonRotateSpeed;
    private float rotateDirection;

    //Verticle Rotation
    public float verticRotateSpeed;
    private float tiltAmount;
    private float horizontalRotateAmount;
    public float tiltMax;

    private Vector2 catRotate;

    [Header("Jump Stats")]
    public float jumpForce; 
    public float jumpForward;

    [Header("Jump & Ground Check")]
    public Vector3 GroundCheckPosition;
    public float GroundCheckRadius;
    public LayerMask GroundLayer;

    //Capsule Ground Check
    private Collider[] colliders;
    public Vector3 startVector;
    public Vector3 endVector;
    public float groundCapRad;
    public float colliderDelay;
    private bool animLand;

    //Capsule Land Anim Check
    private Collider[] animGroundCollide;
    public Vector3 animStartVector;
    public Vector3 animEndVector;
    public float animCapsuleRadius;

    [Header("Particles")]
    public ParticleSystem StunStars;
    public ParticleSystem SlamDust;
    public ParticleSystem SlamForce;
    public ParticleSystem[] CatShedPart;
    //public ParticleSystem CatFart;

    [Header("Attacks")]
    private bool isGrounded;


    [Header("SlamAttack")]
    //Slam Attack---------------------
    public float slamStartDelay;
    public float slamForce;
    public float slamObjForce;
    public float slamLandDelay;
    private bool hasSlammedObj;

    //Can Attack Collider
    private Collider[] attackCollider;
    private Collider[] attackCollider2;
    public Vector3 attackCheckPos1;
    public Vector3 attackCheckPos2;
    public Vector3 attackCheckPos3;
    public Vector3 attackCheckPos4;

    public float attackCheckRadius;
    public float slamUpForce;
    public bool isSlaming = false;

    //Slam Attack Collider
    public Vector3 slamAttackCheckpos;
    public Vector3 squareSlamPos;
    public Vector3 slamAttSquareDistance;
    public float slammAttSphereRadius;
    private Collider[] slammedCollider;

    //Slam to Death Capsule
    public Vector3 slamDeathCap1;
    public Vector3 slamDeathCap2;
    public float slamDeathCapR;
    //public float chicken;

    //Capsule Collider
    public float capTopOffset;
    public float capBottomOffset;
    private Vector3 capsuleTop;
    private Vector3 capsuleBottom;
    public float capRadius; 
    public LayerMask slamLayers;

    //Slam Cpsule Collider
    


    [Header("LungeAttack")]
    //Lunge Attack-------------------------------
    public float lungeHight;
    public float lungeLength;
    public int lungeCD;
    private bool lungeisReady = true;
    private bool launchBuffer;
    private enum Lunge {None, hasJump, hasMove};
    private Lunge currentState;
    public float LungeCheckSec;


   [Header("ScreenShake")]
    private CinemachineImpulseSource impulseSource;
    public ScreenShakeProfile slamProfile;
   // public ScreenShakeProfile slamProfile2;


   [Header("RecieveAttack")]
   public float watergunStunTime = 1.0f;
   public float watergunUpForce;
   public float watergunForwardForce;
   
   [Header("Camera")]
   public GameObject CamTarget;

   [Header("Sound")]
   private AudioSource audioSource;
   public AudioClip[] catWalkSounds;
   public AudioClip catHiss;

   //Audio Sources
   public AudioSource catJump;
   public AudioSource catSlam;
   public AudioSource catLand;
   //public AudioSource catFart;

   RatMovement ratScript;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //animator = GetComponentInChildren<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        audioSource = GetComponent<AudioSource>();
        //ratScript = GameObject.FindWithTag("Rat").GetComponent<RatMovement>();
        animLand = false;
        isGrounded = false;
        animator.SetBool("slamGetUp", true);

        currentState = Lunge.None;
        
    }
    // Update is called once per frame
    void Update()
    {
        //Debugs
        //Debug.Log("currentStae:" + currentState);
        //Cat Dash Movement
        //colliders = Physics.OverlapSphere(transform.position + GroundCheckPosition, GroundCheckRadius, GroundLayer);
        colliders = Physics.OverlapCapsule(transform.TransformPoint(startVector), transform.TransformPoint(endVector), groundCapRad, GroundLayer);
        attackCollider = Physics.OverlapCapsule(transform.TransformPoint(attackCheckPos1), transform.TransformPoint(attackCheckPos2), attackCheckRadius, GroundLayer);
        attackCollider2 = Physics.OverlapCapsule(transform.TransformPoint(attackCheckPos2), transform.TransformPoint(attackCheckPos4), attackCheckRadius, GroundLayer);
        //slammedCollider = Physics.OverlapSphere(transform.TransformPoint(slamAttackCheckpos), slammAttSphereRadius, GroundLayer);
        //slammedCollider = Physics.OverlapBox(transform.TransformPoint(squareSlamPos), transform.position + slamAttSquareDistance,transform.rotation);
        slammedCollider = Physics.OverlapCapsule(transform.TransformPoint(slamDeathCap1), transform.TransformPoint(slamDeathCap2), slamDeathCapR);
        animGroundCollide = Physics.OverlapCapsule(transform.TransformPoint(animStartVector), transform.TransformPoint(animEndVector), animCapsuleRadius, GroundLayer);
        Collider[] slamCollide = Physics.OverlapCapsule(capsuleTop, capsuleBottom, capRadius, slamLayers);

        foreach(Collider slammedObject in slammedCollider)
        {
            if(slammedObject.gameObject.tag == "Rat")
            {
                hasSlammedObj = true;
            }
            else
            
            {
                hasSlammedObj = false;
            }

        }  

        //On Ground Movement Check
        GroundMovementCheck();
        ClampTilt();
    }

    //Update Checks
    private IEnumerator LungeAttackCheck()
    {
       //foreach(ParticleSystem CatSheds in CatShedPart)
       //{
       //    //CatSheds.Play();
       //}
       //Debug.Log("StartSlamCoroutine");
        while(launchBuffer == true) 
        {
            if (currentState == Lunge.hasMove && InputManager.current.CatJump.action.triggered)
            {
                    currentState = Lunge.None;
                    isGrounded = false;
                    LaunchAttack();
                    Debug.Log("Move then Jump Launch");
                    yield break;
                
            }
            else if (currentState == Lunge.hasJump && InputManager.current.CatMove.action.triggered)
            {
                    currentState = Lunge.None;
                    isGrounded = false;
                    LaunchAttack();
                    Debug.Log("Jump then Move Launch");
                    yield break;
            }
            yield return null;
        }
        
        Debug.Log("StartMoveDetect");

        if (launchBuffer == false)
        {
            if (currentState == Lunge.hasJump)//After Buffer, Check What State.
            {
                rb.AddForce(transform.up*jumpForce, ForceMode.Impulse);
                rb.AddForce(transform.forward*jumpForward, ForceMode.Impulse);
                Debug.Log("Jump");
                animator.SetTrigger("Jump");
                Debug.Log("currentStae:" + currentState);
                catJump.Play();
                
                currentState = Lunge.None;
                yield break;
            }else if (currentState == Lunge.hasMove)
            {
                PlayRandomWalkSound();
                rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);
                rb.AddForce(transform.up * leapForce, ForceMode.Impulse);
                StartCoroutine(MovingForward());
                Debug.Log("currentStae:" + currentState);
                Debug.Log("Move");
                animator.SetTrigger("Moved");  
                currentState = Lunge.None;   
                yield break;
               // Debug.Log(currentState);              
            }
        }
        
    }

    //Ground Movements-----------------------------------------------------------------------
    private void GroundMovementCheck()
    {
        if (colliders.Length > 0)
        {
            jumpAmount = 1;
            //Set isGrounded to true
        
                if(isGrounded == false)
                {
                    GameManager.current.inAir = true;
                    //animator.SetTrigger("Land");
                    Debug.Log("SetLanded");
                    catLand.Play();
                }
                if(animLand == false && currentState == Lunge.None)
                {
                    animLand = true;
                    animator.SetTrigger("Land");
                }
                isGrounded = true;
                //isSlaming = true;
                

            //Jump
            if(InputManager.current.CatJump.action.WasPressedThisFrame() && currentState == Lunge.None)
            {
                Debug.Log("Start Jump");
                currentState = Lunge.hasJump;
                StartCoroutine(LungeCheckDelay());
                StartCoroutine(LungeAttackCheck());
                return;
            }else if(InputManager.current.CatMove.action.WasPressedThisFrame() && currentState == Lunge.None)
            {
                Debug.Log("Start Move");
                currentState = Lunge.hasMove;
                StartCoroutine(LungeCheckDelay());
                StartCoroutine(LungeAttackCheck());
            }
            else if(InputManager.current.CatMove.action.WasPressedThisFrame() && InputManager.current.CatJump.action.WasPerformedThisFrame())
            {
                LaunchAttack();
            }

           //if(InputManager.current.CatMove.action.WasPressedThisFrame())
           //{
           //    PlayRandomWalkSound();
           //    rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);
           //    rb.AddForce(transform.up * leapForce, ForceMode.Impulse);
           //}

        }else
        {
            if (isGrounded == true)
            {
                isGrounded = false;
            }
            if (jumpAmount > 0)
            {
                if(InputManager.current.CatMove.action.WasPressedThisFrame())
                {
                    rb.AddForce(transform.forward * midAirMoveForce, ForceMode.Impulse);
                    //CatFart.Play();
                    //catFart.Play();
                    jumpAmount -= 1;
                }
            }
        }
        
        if(attackCollider.Length < 1 && attackCollider2.Length < 1)
        {
            //While in air, set is Grounded to false
            isGrounded = false;
            GameManager.current.inAir = true;

            //Slam Attack
            if (InputManager.current.CatJump.action.triggered)
            {
                StartCoroutine(SlamAttack());
                
            }   
        }
        else
        {
            GameManager.current.inAir = false;
        }

        if (animGroundCollide.Length > 0)
        {
            if (animLand == false)
            {
                animLand = true;
                animator.SetTrigger("Land");
            }
            animLand = true;
        }
        else
        {
            animLand = false;
        }
    }

    //Launch----------------------------------------------------------------------------
    private IEnumerator LungeCheckDelay()
    {
        launchBuffer = true;
        yield return new WaitForSeconds(LungeCheckSec);
        launchBuffer = false;
    }

    public void LaunchAttack()
    {
        launchBuffer = false;

        //rb.linearVelocity = new Vector3(0,0,0);
        rb.AddForce(transform.forward * lungeLength, ForceMode.Impulse);
        rb.AddForce(transform.up * lungeHight, ForceMode.Impulse);
       // StartCoroutine(LungeCoolDown());
        Debug.Log("Launch");

        //animator.SetTrigger("Land");
        //isGrounded = false;
        animLand = false;
        animator.SetTrigger("Jump");
         currentState = Lunge.None;
    }


    //Slam------------------------------------------------------------------------------
    public IEnumerator SlamAttack()
    {
        rb.linearVelocity = new Vector3(0,0,0);
        rb.constraints = RigidbodyConstraints.FreezeAll;
        
        animator.SetBool("isSlam", true);
        animator.SetBool("slamGetUp", false);

        yield return new WaitForSeconds(slamStartDelay);

        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(-transform.up * slamForce, ForceMode.Impulse);
        InputManager.current.DisableCat();

        //Check if landed---------------
        ////Collider[] colliders = Physics.OverlapSphere(transform.position + GroundCheckPosition, GroundCheckRadius, GroundLayer);

        while(colliders.Length == 0)
        {
            Debug.Log("SlamStart");
            foreach(Collider slammed in slammedCollider)
            {
                if (slammed.gameObject.tag == "Rat")
                {
                    ratScript = slammed.gameObject.GetComponent<RatMovement>();
                    if(ratScript)
                    {
                        ratScript.StartCoroutine(ratScript.GotSlammed(slamForce));
                        hasSlammedObj = true;
                    }

                    Debug.Log("CatWin");
                    if(attackCollider.Length > 1 && attackCollider2.Length > 1)
                    {
                        catSlam.Play();
                    }
                }
            }

            // foreach(Collider slammed in attackCollider2)
            //{
            //    if (slammed.gameObject.tag == "Rat")
            //    {
            //        ratScript.StartCoroutine(ratScript.GotSlammed(slamForce));
            //        Debug.Log("CatWin");
            //    }
            //}
            isSlaming = true;
            yield return null;
    
        }
        Debug.Log("SlamStop");

        //Slam Collision Check------------------
        isSlaming = false;
        capsuleTop = new Vector3 (transform.position.x, transform.position.y + capTopOffset, transform.position.z);
        capsuleBottom = new Vector3 (transform.position.x, transform.position.y + capBottomOffset, transform.position.z);
        Collider[] slamCollide = Physics.OverlapCapsule(capsuleTop, capsuleBottom, capRadius, slamLayers);

            foreach(Collider slamTarget in slamCollide)
            {
               //if (slamTarget.gameObject.tag == "Rat" && hasSlammedObj == false)
               //{
                    //Run Stun
                    RatMovement ratPlayerScript = slamTarget.GetComponent<RatMovement>();
                    if(ratPlayerScript && hasSlammedObj == false)
                    {
                        ratPlayerScript.StartCoroutine(ratPlayerScript.SlamStunned());
                        ratPlayerScript.SlamShake(transform.position);
                    }
    
                    //Hit Up
                    Rigidbody slammedRb = slamTarget.attachedRigidbody;
                    if(slamTarget.gameObject.tag == "Rat")
                    {
                        slammedRb.AddForce(slamTarget.transform.up * slamUpForce, ForceMode.Impulse);
                    }else if(slammedRb)
                    {
                        slammedRb.AddForce(slamTarget.transform.up * slamObjForce, ForceMode.Impulse);
                    }
                   
               // }//
            }

        //Screen Shakee------------------------------------------
        //CameraManager.Coal.ScreenShakeFromProfile(slamProfile, impulseSource); 
        CameraManager.Coal.CameraShake(impulseSource);
        Debug.Log("in the Slammer");
        SlamDust.Play();
        SlamForce.Play();
        animator.SetBool("isSlam", false);
        isGrounded = true;
        

        //Delay for returning to normal
        yield return new WaitForSeconds(slamLandDelay);
        animator.SetBool("slamGetUp", true);
        if(hasSlammedObj == true)
        {
            animator.SetTrigger("CatWin");
        }
        
        InputManager.current.EnableCat();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    public IEnumerator MovingForward()
    {
        yield return new WaitForSeconds(colliderDelay);
        if(colliders.Length > 0) { yield break;};
        while(colliders.Length == 0)
        {
            //lower hearts???   
            yield return null;
        }
        RatShake();
    }

    public void RatShake()
    {
        if(ratScript)
        {
            ratScript.SlamShake(transform.position);
        }
    }

    //Attack Recieve--------------------------------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.tag == "Rat")
        //{
        //    Debug.Log("SlamonRat");
        //}
        //if (collision.gameObject.tag == "Rat" && isSlaming == true)
        //{
        //    GameManager.current.state = GameState.CatWin;
        //    Debug.Log("SlamOnRat");
        //}
        if (collision.gameObject.tag == "WaterBullet")
        {
            rb.AddForce(collision.gameObject.transform.forward * watergunForwardForce, ForceMode.Impulse);
            rb.AddForce(transform.up * watergunUpForce, ForceMode.Impulse);
            StartCoroutine(WatergunStun());
            Destroy(collision.gameObject);
        }
    }

    public IEnumerator WatergunStun()
    {
        InputManager.current.DisableCat();
        audioSource.PlayOneShot(catHiss);
        animator.SetBool("isStunned", true);
        GameManager.current.ShowWaterSplash();
        StunStars.Play();

        yield return new WaitForSeconds(watergunStunTime);

        StunStars.Stop();
        animator.SetBool("isStunned", false);
        InputManager.current.EnableCat();
    }

    public IEnumerator LungeCoolDown()
    {
        lungeisReady = false;
        
        yield return new WaitForSeconds(lungeCD);

        lungeisReady = true;

    }

    private void PlayRandomWalkSound()
    {
        Debug.Log("Playing cat walk");
        audioSource.PlayOneShot(catWalkSounds[UnityEngine.Random.Range(0,catWalkSounds.Length-1)]);
    }
    private void ClampTilt()
    {
        tiltAmount = Mathf.Clamp(tiltAmount,-tiltMax, tiltMax);
    }

    void OnDrawGizmos()
    {
        if (isGrounded == false)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        //Gizmos.DrawWireSphere(transform.position + GroundCheckPosition, GroundCheckRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(startVector), groundCapRad);
        Gizmos.DrawWireSphere(transform.TransformPoint(endVector), groundCapRad);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(attackCheckPos1), attackCheckRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(attackCheckPos2), attackCheckRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(attackCheckPos3), attackCheckRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(attackCheckPos4), attackCheckRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(capsuleTop, capRadius);
        Gizmos.DrawWireSphere(capsuleBottom, capRadius);
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.TransformPoint(slamDeathCap1), slamDeathCapR);
        Gizmos.DrawWireSphere(transform.TransformPoint(slamDeathCap2), slamDeathCapR);
        Gizmos.DrawWireSphere(transform.TransformPoint(slamAttackCheckpos), slammAttSphereRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.TransformPoint(animStartVector), animCapsuleRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(animEndVector), animCapsuleRadius);
        //if(hasSlammedObj == true)
        //{
        //    Gizmos.color = Color.purple;
        //}
        //else
        //{
        //    Gizmos.color = Color.red;
        //}
        //Gizmos.DrawWireCube(transform.TransformPoint(squareSlamPos), slamAttSquareDistance);
        //Gizmos.DrawWireSphere(transform.TransformPoint(slamDeathCap1), slamDeathCapR);
        //Gizmos.DrawWireSphere(transform.TransformPoint(slamDeathCap2), slamDeathCapR);
    
    }

    void FixedUpdate()
    {
        //Set Cat Rotaion
        //Cat Rotation Vertical
        catRotate = InputManager.current.CatTurn.action.ReadValue<Vector2>();

        horizontalRotateAmount += catRotate.x * horizonRotateSpeed;
        tiltAmount += catRotate.y * verticRotateSpeed;

        CamTarget.transform.eulerAngles = new Vector3(tiltAmount, horizontalRotateAmount, 0);
        transform.eulerAngles = new Vector3(0, horizontalRotateAmount, 0);
    }
}
