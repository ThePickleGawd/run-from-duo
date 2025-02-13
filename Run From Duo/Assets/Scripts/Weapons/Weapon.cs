using UnityEngine;
using System.Collections;
using TMPro;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip shootEmptySound;

    [Header("Weapon Settings")]
    [SerializeField] private bool autoFire = false;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private int maxAmmo = 21;
    private float range = 100f;

    // Components
    private AudioSource audioSource;

    // State
    private int ammo = 21;
    private bool isFiring = false;
    public bool IsFiring { get { return isFiring; } }

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartFiring()
    {
        if (isFiring) return;

        if (!CanFire())
        {
            PlaySound(shootEmptySound);
            return;
        }

        isFiring = true;
        StartCoroutine(Fire());
    }

    public void StopFiring()
    {
        isFiring = false;
    }

    private IEnumerator Fire()
    {
        do
        {
            PlaySound(shootSound);
            ammo--;
            UpdateAmmoUI();

            RaycastHit hit;
            if (Physics.Raycast(bulletSpawn.position, bulletSpawn.forward, out hit, range))
            {
                Health health = hit.transform.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                // TODO: Spawn impact effect
            }

            yield return new WaitForSeconds(1f / fireRate);

        } while (isFiring && autoFire && CanFire());
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void UpdateAmmoUI()
    {
        ammoText.text = $"{ammo}/{maxAmmo}";
    }

    public bool CanFire() => ammo > 0;
}
