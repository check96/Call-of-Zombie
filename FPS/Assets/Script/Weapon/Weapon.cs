using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class Weapon : MonoBehaviour
{

    private Animator anim;
    private AudioSource _AudioSource;

    [Header("Properties")]
    public float fireRate = 0.1f;       //The delay between each shot
    public float damage = 20f;

    public float range = 100f;          //Max range of the weapon
    public int bulletsPerMag = 30;      //bullet per each magazine
    public int bulletsLeft = 200;       //Total bullets we have

    public int currentBullets;          //The current bullets in our magazine

    public float spreadFactor = 0.1f;   //accuracy

    public enum ShootMode { Auto, Semi }
    public ShootMode shootingMode;

    [Header("UI")]
    public Text ammoText;

    [Header("SeUp")]
    public Transform shootPoint;        //The point from whitch the bullet leave the muzzle
    public GameObject hitParticles;
    public GameObject bulletImpact;

    public ParticleSystem muzzleFlash;  //Muzzle flash

    [Header("Sound Effects")]
    public AudioClip shootSound;
    
    float fireTimer;                    //Time counter for the delay
    private bool isReloading;
    private bool isAiming;
    private bool shootInput;

    private Vector3 originalPosition;

    [Header("ADS")]
    public Vector3 aimPosition;
    public float aodSpeed = 8f;

    private void OnEnable()
    {
        //update when active state is changed
        UpdateAmmoText();
    }
    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        _AudioSource = GetComponent<AudioSource>();
        currentBullets = bulletsPerMag;
        originalPosition = transform.localPosition;

        UpdateAmmoText();
    }

    // Update is called once per frame
    void Update()
    {
        switch (shootingMode)
        {
            case ShootMode.Auto:
                shootInput = Input.GetButton("Fire1");
                break;

            case ShootMode.Semi:
                shootInput = Input.GetButtonDown("Fire1");
                break;
        }

        if (shootInput)
        {
            if (currentBullets > 0)
                Fire();                         //Execute the fire function if we press/hold the left mouse button
            else if (bulletsLeft > 0)
                DoReload();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentBullets < bulletsPerMag && bulletsLeft > 0)
                DoReload();
        }

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;    //Add into time counter
        }

        AimDownSights();
    }

    private void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");
        anim.SetBool("Aim", isAiming);
    }

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aodSpeed);
            isAiming = true;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aodSpeed);
            isAiming = false;
        }
    }

    private void Fire()
    {
        if (fireTimer < fireRate || currentBullets <= 0)
            return;

        RaycastHit hit;

        //accuracy
        Vector3 shootDirection = shootPoint.transform.forward;
        shootDirection.x += Random.Range(-spreadFactor, spreadFactor);
        shootDirection.y += Random.Range(-spreadFactor, spreadFactor);

        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
        {
            Debug.Log(hit.transform.name + " found!");

            //Spawn a hit particle effect and the bullet impact position
            GameObject hitParticleEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));     //allign the sparcle to the plane of impact od the bullet
                                                                                                                                        //Spawn a hit bullet hole decal at the bullet impact position
            GameObject bulletHole = Instantiate(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));

            Destroy(hitParticleEffect, 1f);
            Destroy(bulletHole, 2f);

            if (hit.transform.GetComponent<HealtController>())
            {
                hit.transform.GetComponent<HealtController>().ApplyDamage(damage);
            }
        }

        anim.CrossFadeInFixedTime("Fire", 0.01f);   //Play the fire animation
        muzzleFlash.Play();                         //Show the muzzle flash
        PlayShootSound();                           //Play the shooting sound effect

        currentBullets--;                            //deduct one bullet

        UpdateAmmoText();                           //update ammo text

        fireTimer = 0.0f;                           //Reset fire timer
    }

    public void Reload()
    {
        Debug.Log("here");
        if (bulletsLeft <= 0) return;

        int bulletToLoad = bulletsPerMag - currentBullets;
        //                                  IF            then            else
        int bulletsToDeduct = (bulletsLeft >= bulletToLoad) ? bulletToLoad : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;
    }

    private void DoReload()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        if (isReloading) return;

        anim.CrossFadeInFixedTime("Reload", 0.01f);
    }

    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
        //_AudioSource.clip = shootSound;
        //_AudioSource.Play();
    }

    private void UpdateAmmoText()
    {
        ammoText.text = currentBullets + " / " + bulletsLeft;
    }
}