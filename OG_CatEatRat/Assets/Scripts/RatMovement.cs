using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
//using UnityEditor.VersionControl;
//using NUnit.Framework;
//using System.Numerics;

public class RatMovement : MonoBehaviour
{
    //[Header("Componenets")]
    private Rigidbody rb;
    private Animator animator;

    [Header("Movements")]
    private bool isMoving;
    public float forwardForceAmmount;
    public ParticleSystem RightFootstep;
    public ParticleSystem LeftFootstep;
    public bool isRunning; 
    public float onSlopeForceMultiply;

    [Header("Rotaion")]
    //Rotation
    private Vector2 turnRotation;
    private float verticRotate;
    private float horizonRotate;
    public float rotateSpeed;
    public float verticTiltMax;
    public GameObject camTarget;


    [Header("Jumping")]
    public float gravityScale = 1.0f;
    private Collider[] groundCollider;
    public Vector3 GroundCheckPosition;
    public float GroundCheckRadius;
    public LayerMask GroundLayer;
    public float jumpForce;
    public float additionalJumpForce;
    public int additionalJumps;
    private int additionalJumpsLeft;
    private bool isGrounded = false;

    //Mid Air Drifting
    public float driftStartDelayTime;
    public float rbOriginLD;
    public float rbMidairLD;
    public float airMoveMultiplier;

    [Header("Dash")]
    public float dashForce;

    [Header("WaterGun")]
    public GameObject waterBullet;
    public float shootCD;
    private bool canShoot;
    public Vector3 distanceFromOrigin;
    public Transform camTrans;
    public float maxShootDistance = 1000.0f;
    public CinemachineCamera cinemaCam;
    public GameObject camTarget2;
    public Camera Cam;
    public LayerMask hitLayer;
    public float raySpawnDistance;
    public int waterAmmo = 5;
    public int maxAmmo;
    public float reloadTime;
    private bool isRecharging;
    public float lerpSpeed;

    //Water Gun Object
    private GameObject Watergun;

    [Header("AttackRecieve")]
    public float slamStunTime;
    public ParticleSystem stunStars;
    public float catWinDelay;
    public float slamUpForce;
    public Vector3 slamGCheckCapTop;
    public Vector3 slamGCheckCapBot;
    public float slamGCheckCapRadius;
    public float vignetteAmount; 
    private bool isSlammed;

    [Header("ItemPickup")]
    //private bool hasCheese = false;
    private bool inPickupRange = false;
    public Vector3 pickRangeOffset;
    public float pickRangeRadius;
    public LayerMask pickLayer;
    private GameObject cheeseObj;
    public Vector3 itemDropOffset;
    public GameObject pickupCheese;
    public float throwForce;
    [Range(0,1)]public float throwAngle;


    //Water Gun
    public GameObject pickupWatergun;
    
    //Debugging
    private Vector3 rayDebug;
    private Vector3 hitPosition;

    //[Header("Inventory")]
    private enum InventoryState {None, hasCheese, hasWaterGun};
    private InventoryState inventState;

    [Header("WinGame")]
    public float winGameDelay = 2.0f;

    [Header("Sounds")]
    private AudioSource audioSource;
    public AudioClip gunSquirt;
    public AudioClip itemPickup;
    public AudioClip fartSound;
    public AudioClip jumpSound;

    //Audio Sources
    public AudioSource gunSquirtS;
    public AudioSource itemPickupSC;
    public AudioSource itemDropupSC;
    public AudioSource itemPickupSW;
    public AudioSource itemDropupSW;
    public AudioSource fartSoundS;
    public AudioSource jumpSoundS;
    public AudioSource ratSlammedS;
    public AudioSource ratCheeseInHole;

    [Header("ScreenShake")]
    private CinemachineImpulseSource impulseSource;

    public Volume RatVolume;
    private Vignette RatVignette;
    private ChromaticAberration RatChroma;
    

    [Header("Particles")]
    public ParticleSystem fart;
    public ParticleSystem[] gotSlammedPart;
    public ParticleSystem[] Shed;
    public ParticleSystem[] winCheese;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        //complicated ahh way to access
        if (RatVolume.profile.TryGet<Vignette>(out var tmpVignette))
        {
            RatVignette = tmpVignette;
        }
        if (RatVolume.profile.TryGet<ChromaticAberration>(out var tmpAbbr))
        {
            RatChroma = tmpAbbr;
        }

        if (RatVolume == null)
    {
        Debug.LogError("RatVolume is not assigned in the Inspector.");
    }
    else if (RatVolume.profile == null)
    {
        Debug.LogError("RatVolume does not have a Volume Profile.");
    }
    else if (!RatVolume.profile.TryGet(out RatVignette))
    {
        Debug.LogError("The Volume Profile does not contain a Vignette override.");
    }

        cheeseObj = transform.Find("RatCheese").gameObject;
        cheeseObj.SetActive(false);
        Watergun = transform.Find("RatWaterGun").gameObject;
        Watergun.SetActive(false);
        isRecharging = false;


        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        inventState = InventoryState.None;
        waterAmmo = maxAmmo;
        GameManager.current.SetWaterGun(waterAmmo, maxAmmo);

        groundCollider = Physics.OverlapSphere(transform.position + GroundCheckPosition, GroundCheckRadius, GroundLayer);

        
        //Reset Ammo
        if (InputManager.current.RatMove.action != null)
        {
            Debug.Log("rat action assigned");
            
        }
    }


    // Update is called once per frame
    void Update()
    {
        groundCollider = Physics.OverlapSphere(transform.TransformPoint(GroundCheckPosition), GroundCheckRadius, GroundLayer);

          //Move Forward Stop
          if (InputManager.current.RatMove.action.WasReleasedThisFrame())
          {
             animator.SetBool("isRunning", false);
          }

          //Turning----------------------------------------------------

          turnRotation = InputManager.current.RatTurn.action.ReadValue<Vector2>();
          //Debug.Log(turnRotation.x);
          Clamp();
          //transform.Rotate(Vector3.up, turnRotation.x * rotateSpeed * Time.deltaTime);
          verticRotate += turnRotation.y * rotateSpeed;
          horizonRotate += turnRotation.x * rotateSpeed;
          //Debug.Log(transform.eulerAngles.y);
          camTarget.transform.rotation = Quaternion.Euler(-verticRotate, horizonRotate, 0f);
          transform.rotation = Quaternion.Euler(0f, horizonRotate, 0f);

        //Jump-------------------------------------------------------
        if (groundCollider.Length > 0)
        {
            if(isGrounded == false)
            {
                animator.SetTrigger("Land");
            }
            isGrounded = true;

            //Slope Force Multiplication

            foreach(Collider slopeCollides in groundCollider)
            {
                if(slopeCollides.gameObject.tag == "Slope")
                {
                    onSlopeForceMultiply = 4;
                }
                else
                {
                    onSlopeForceMultiply = 1;
                }
            }
            


            if (InputManager.current.RatJump.action.triggered)
            {
                StartCoroutine(MidAirDrift());
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                additionalJumpsLeft = additionalJumps;
                //audioSource.PlayOneShot(jumpSound);
                jumpSoundS.PlayOneShot(jumpSound);

                animator.SetTrigger("Jump");
            }
        } else if (additionalJumpsLeft > 0) //Double-Jump
        {
            if (InputManager.current.RatJump.action.triggered)
            {
                rb.AddForce(transform.up * additionalJumpForce, ForceMode.Impulse);
                additionalJumpsLeft--;
                fart.Play();
                //audioSource.PlayOneShot(fartSound);
                fartSoundS.PlayOneShot(fartSound);
            }
        }
        if (groundCollider.Length < 1)
        {
            onSlopeForceMultiply = 1;
            isGrounded = false;
        }

        //Pickup---------------------------------------------------------
        if(InputManager.current.RatPickUp.action.triggered)
        {
            ItemInteraction();
        }

        //Water Gun Blast-----------------------------------------------
        if (InputManager.current.RatBlast.action.WasPressedThisFrame())
        {
            if(inventState == InventoryState.hasWaterGun)
            {
                
                Debug.Log("WaterBlast");
                ShootWaterBlast();
            }

        }

        if(isGrounded && !LeftFootstep.isPlaying)
        {
            LeftFootstep.Play();
            RightFootstep.Play();
        } else if(!isGrounded && LeftFootstep.isPlaying)
        {
            LeftFootstep.Stop();
            RightFootstep.Stop();
        }
    
    }

    private void PlayWinParticle()
    {
        foreach(ParticleSystem winPart in winCheese)
        {
            winPart.Play();
        }
        
    }

    private void ItemInteraction()
    {

        Collider[] Pickup = Physics.OverlapSphere(transform.TransformPoint(pickRangeOffset), pickRangeRadius, pickLayer);
        Vector3 itemSpawnPos = transform.TransformPoint(itemDropOffset);

        if(Pickup.Length < 1)
        {
            ThrowItem();
        }else
        {
        //Object Pickup------------------------------------------------------------------------------------
            foreach (Collider PickedObj in Pickup)
            {
                //Cheese Pickup---------------------------------------
                if(PickedObj.gameObject.CompareTag("Cheese"))
                {
                    audioSource.PlayOneShot(itemPickup);
                    itemPickupSC.Play();
                    Debug.Log("PickupCheese");

                    if(inventState == InventoryState.None)
                    {
                        Destroy(PickedObj.gameObject);
                        cinemaCam.Target.TrackingTarget = camTarget.transform;
                        inventState = InventoryState.hasCheese;
                        cheeseObj.SetActive(true);
                    }  
                    else if(inventState == InventoryState.hasWaterGun)
                    {
                        animator.SetTrigger("Throw");
                        Destroy(PickedObj.gameObject);
                        cinemaCam.Target.TrackingTarget = camTarget.transform;
                        GameObject droppedGun = Instantiate(pickupWatergun, itemSpawnPos, transform.rotation);
                        droppedGun.GetComponent<Rigidbody>().AddForce(throwForce * Vector3.up, ForceMode.Impulse);
                        Watergun.SetActive(false);
                        cheeseObj.SetActive(true);
                        inventState = InventoryState.hasCheese;
                        itemDropupSW.Play();
                        GameManager.current.ShowWaterGun(false);  
                    }
                }
                else if(PickedObj.gameObject.CompareTag("Watergun"))//WaterGun--------------------------
                {
                    Debug.Log("PickupWatergun");
                    audioSource.PlayOneShot(itemPickup);
                    itemPickupSW.Play();

                    if(inventState == InventoryState.None)
                    {
                        Destroy(PickedObj.gameObject);
                        inventState = InventoryState.hasWaterGun;
                        cinemaCam.Target.TrackingTarget = camTarget2.transform;
                        Watergun.SetActive(true);     
                        GameManager.current.ShowWaterGun(true);           
                    }
                     else if(inventState == InventoryState.hasCheese)
                    {
                        animator.SetTrigger("Throw");
                        Destroy(PickedObj.gameObject);
                        GameObject pickedCheese = Instantiate(pickupCheese, itemSpawnPos, transform.rotation);
                        pickedCheese.GetComponent<Rigidbody>().AddForce((throwAngle * throwForce * Vector3.up) + (throwForce * (1-throwAngle) * Vector3.forward), ForceMode.Impulse);
                        cheeseObj.SetActive(false);
                        Watergun.SetActive(true);
                        cinemaCam.Target.TrackingTarget = camTarget2.transform;
                        inventState = InventoryState.hasWaterGun;
                        itemDropupSC.Play();
                        GameManager.current.ShowWaterGun(true);  
                    }
                }  
                else if(PickedObj.gameObject.CompareTag("WaterPlate") && isRecharging == false)
                {
                    Debug.Log("Activate Reload");
                    itemPickupSC.Play();
                    isRecharging = true;
                    StartCoroutine(Reload());
                }else if
                (PickedObj.gameObject.tag != "Cheese" || PickedObj.gameObject.tag != "Watergun" || PickedObj.gameObject.tag != "WaterPlate") 
                { 
                    ThrowItem(); 
                }


            }
        }
    }

    private void ThrowItem()
    {
        animator.SetTrigger("Throw");
        Vector3 itemSpawnPos = transform.TransformPoint(itemDropOffset);
        if(inventState == InventoryState.hasCheese)
        {
            inventState = InventoryState.None;
            GameObject pickedCheese = Instantiate(pickupCheese, itemSpawnPos, transform.rotation);
            pickedCheese.GetComponent<Rigidbody>().AddForce((throwAngle * throwForce * Vector3.up) + (throwForce * (1-throwAngle) * Vector3.forward), ForceMode.Impulse);
            cheeseObj.SetActive(false);
            itemDropupSC.Play();
        }
        else if(inventState == InventoryState.hasWaterGun)
        {
            inventState = InventoryState.None;
            GameObject droppedGun = Instantiate(pickupWatergun, itemSpawnPos, transform.rotation);
            droppedGun.GetComponent<Rigidbody>().AddForce(throwForce * Vector3.up, ForceMode.Impulse);
            Watergun.SetActive(false);
            cinemaCam.Target.TrackingTarget = camTarget.transform;
            GameManager.current.ShowWaterGun(false);
            itemDropupSW.Play();
        }
}


    public IEnumerator MidAirDrift()
    {
        yield return new WaitForSeconds(driftStartDelayTime);

        while(groundCollider.Length == 0)
        {
            rb.linearDamping = rbMidairLD;
            yield return null;
        }
        rb.linearDamping = rbOriginLD;

    }
    
    //Be Slammed -----------------------------------------------------------------------------------------------------------------------------------------------------
    public IEnumerator SlamStunned()
    {
        
        //if(GameManager.current.state != GameState.CatWin)
        //{
            Collider[] SlamGCollide = Physics.OverlapCapsule(transform.TransformPoint(slamGCheckCapTop), slamGCheckCapBot, slamGCheckCapRadius, GroundLayer);

            if (SlamGCollide.Length > 0)
            {
                InputManager.current.DisableRat();
                //rb.AddForce(transform.up * slamUpForce, ForceMode.Impulse);
                stunStars.Play();
                animator.SetBool("isStunned", true);
                CameraManager.Coal.CameraShake(impulseSource);


                if (RatVignette == null)
                {
                    Debug.LogError("Cannot activate vignette because RatVignette is null.");
                    yield break;
                }
                
                //debug
                RatVignette.active = true;
                RatVignette.intensity.overrideState = true;
                RatVignette.intensity.value = vignetteAmount;
            //yield return new WaitForSeconds(slamStunTime);

            
                float elapsed = 0f;
                while (elapsed < slamStunTime)
                {
                elapsed += Time.deltaTime;
                float completion = Mathf.Clamp01(elapsed / slamStunTime);

                RatVignette.intensity.value = Mathf.Lerp(vignetteAmount, 0f, completion);
                RatChroma.intensity.value = Mathf.Lerp(vignetteAmount, 0f, completion);

                yield return null;
                }

                RatVignette.intensity.value = 0f;
                RatChroma.intensity.value = 0f;

                Debug.Log("Stunned Stop");
                stunStars.Stop();
                animator.SetBool("isStunned", false);
                if (isSlammed == false)
                {
                InputManager.current.EnableRat();
                }
            }
        //}
    }

    public void SlamShake(Vector3 cat)
    {
        float dist = Vector3.Distance(transform.position, cat);
        float shakeAmount = 1/(dist+1);
        CameraManager.Coal.CameraShake(impulseSource, shakeAmount);
    }

    public IEnumerator GotSlammed(float SlamForce)
    {
        isSlammed = true;
        LeftFootstep.Stop();
        RightFootstep.Stop();
        Debug.Log("GotSlammed");
        animator.SetBool("Flatten", true);
        InputManager.current.DisableRat();
        ThrowItem();
        InputManager.current.DisableRat();
        rb.AddForce(-transform.up * 400, ForceMode.Impulse);
        rb.linearVelocity = new Vector3(0, -400, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        //rb.constraints = RigidbodyConstraints.FreezePositionX;
        //rb.constraints = RigidbodyConstraints.FreezePositionZ;
        ratSlammedS.Play();
        foreach(ParticleSystem Slammed in gotSlammedPart)
        {
            Slammed.Play();
        }

        yield return new WaitForSeconds(catWinDelay);

        GameManager.current.CatWin();


    }
    //------------------------------------------------------------------------------------------------------------------------------
    
    private void ShootWaterBlast()
    {
        //Old Shooting Machenism
        /*
        Vector3 blastEuler = new Vector3(verticRotate, transform.eulerAngles.y, 0.0f);
        Quaternion blastAim = Quaternion.Euler(blastEuler);
        Vector3 bulletSpawnPoint = transform.TransformPoint(distanceFromOrigin);
        */
        Debug.Log("BlastBullet");
        Debug.Log(waterAmmo);
        gunSquirtS.Play();

        Vector3 bulletSpawnPoint = transform.TransformPoint(distanceFromOrigin);

        //Creat the ray
        Ray shootray = Cam.ViewportPointToRay(new Vector3(0.5f,0.5f,0));  //Creates a ray that goes through this point of the camera
        RaycastHit hit;
        shootray.origin += shootray.direction * raySpawnDistance;
        rayDebug = shootray.origin;

        Vector3 aimPoint;
        Vector3 aimDirection;

        //Shoot Ray(New Machenism)
        if (waterAmmo > 0)
        {
            if (Physics.Raycast(shootray, out hit, Mathf.Infinity, hitLayer))
            {
                aimPoint = hit.point;
            }
            else
            {
                aimPoint = shootray.direction * maxShootDistance;
            }
            aimDirection = (aimPoint - bulletSpawnPoint).normalized;
            Instantiate(waterBullet, bulletSpawnPoint, Quaternion.LookRotation(aimDirection)); 
            Debug.DrawRay(shootray.origin, shootray.direction * 1000);
            waterAmmo -= 1;
            GameManager.current.SetWaterGun(maxAmmo, waterAmmo);
            audioSource.PlayOneShot(gunSquirt);
        }
        else if (waterAmmo <= 0)
        {
            waterAmmo = 0;
            audioSource.PlayOneShot(gunSquirt);
        }
            Debug.Log(waterAmmo);
    }

    //Relaod -----------------------------------------------------------------------------------------------------------------------------------------
    private IEnumerator Reload()
    {
       //Quaternion targetAngle = Quaternion.Euler(-90,0,0);
       //animator.SetBool("Refill", true);
       //Debug.Log("StartRefill");
       //while(Watergun.transform.rotation < targetAngle)
       //{
       //    Debug.Log("StartRefillANim");
       //    float currentAngle =  Mathf.Lerp(Watergun.transform.rotation.x, targetAngle, lerpSpeed);
       //    Watergun.transform.rotation = Quaternion.Euler(currentAngle, 0,0);
       //    yield return null;
       //}
       Watergun.transform.Rotate(-90,0,0);
        while(rb.linearVelocity.magnitude < 2 && waterAmmo < maxAmmo )
        {
            Debug.Log("Recharge");
            //Debug.Log(rb.linearVelocity.magnitude);
            waterAmmo += 1;
            yield return new WaitForSeconds(reloadTime);
            GameManager.current.SetWaterGun(maxAmmo, waterAmmo);

        }
        
        // while(Watergun.transform.rotation.y < -1)
        //{
        //    float currentAngle2 =  Mathf.Lerp(Watergun.transform.rotation.x, 0, lerpSpeed);
        //    Watergun.transform.rotation = Quaternion.Euler(currentAngle2, 0,0);
        //    yield return null;
        //}
//
        Watergun.transform.Rotate(90,0,0);
        animator.SetBool("Refill", false);
        isRecharging = false;
        Debug.Log("EndRefill");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "RatHole" && inventState == InventoryState.hasCheese)
        {
            GameManager.current.UpdateCheeses(1);
            inventState = InventoryState.None;
            cheeseObj.SetActive(false);
            PlayWinParticle();
            ratCheeseInHole.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cheese")
        {
            inPickupRange = false;
        }
    }

    void FixedUpdate()
    {
        
        //Move Forward----------------------------------------------
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        
        if (InputManager.current.RatMove.action.IsPressed())
        {
            if (groundCollider.Length > 0)
            {
                rb.AddForce(transform.forward * forwardForceAmmount);

                //audioSource.Play();
                animator.SetBool("isRunning", true);

                if (isRunning == false)
                {
                    audioSource.Play();
                   //foreach(ParticleSystem Sheds in Shed)
                   //{
                   //    Sheds.Play();
                   //}
 

                }
                isRunning = true;
            } else
            {
                rb.AddForce(transform.forward * airMoveMultiplier * forwardForceAmmount * onSlopeForceMultiply);
                //audioSource.Stop();

                animator.SetBool("isRunning", false);
                isRunning = false;
                audioSource.Stop();
               //foreach(ParticleSystem Sheds in Shed)
               //{
               //    Sheds.Stop();
               //}
            }
        }
        else
        {
            isRunning = false;
            audioSource.Stop();
           //foreach(ParticleSystem Sheds in Shed)
           //{
           //    Sheds.Stop();
           //}
        }

      //if (InputManager.current.RatMove.action.WasPressedThisDynamicUpdate())
      //{
      //    foreach(ParticleSystem Sheds in Shed)
      //    {
      //        Sheds.Play();
      //    }
      //    
      //}
      //if (InputManager.current.RatMove.action.WasReleasedThisDynamicUpdate())
      //{
      //    foreach(ParticleSystem Sheds in Shed)
      //    {
      //        Sheds.Stop();
      //    }
      //}
     

          //Turning----------------------------------------------------
        turnRotation = InputManager.current.RatTurn.action.ReadValue<Vector2>();
        //Debug.Log(turnRotation.x);
        Clamp();

        //transform.Rotate(Vector3.up, turnRotation.x * rotateSpeed * Time.deltaTime);
        verticRotate += turnRotation.y * rotateSpeed;
        horizonRotate += turnRotation.x * rotateSpeed;
        //Debug.Log(transform.eulerAngles.y);
        camTarget.transform.eulerAngles = new Vector3(-verticRotate, horizonRotate, 0);
        camTarget2.transform.eulerAngles = new Vector3(-verticRotate, horizonRotate, 0);
        transform.eulerAngles = new Vector3(0, horizonRotate, 0);
    }

    private void Clamp()
    {
        verticRotate = Mathf.Clamp(verticRotate,-verticTiltMax, verticTiltMax);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(GroundCheckPosition), GroundCheckRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(distanceFromOrigin), 0.1f);
        Gizmos.DrawWireSphere(rayDebug, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(hitPosition, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.TransformPoint(itemDropOffset), 0.1f);
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.TransformPoint(pickRangeOffset), pickRangeRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.TransformPoint(slamGCheckCapTop), slamGCheckCapRadius);
        Gizmos.DrawWireSphere(transform.TransformPoint(slamGCheckCapBot), slamGCheckCapRadius);
        

        

    }



}
