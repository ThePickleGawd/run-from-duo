using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


[RequireComponent(typeof(OutlineOnHover))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class InteractableWeapon : MonoBehaviour
{
  [SerializeField] private Ammo ammoPrefab;
  [HideInInspector] public Weapon weapon;
  [HideInInspector] public XRSocketInteractor ammoSocket;
  [HideInInspector] public XRGrabInteractable interactable;

  private Rigidbody rb;

  private void Awake()
  {
    // Find Components
    weapon = GetComponent<Weapon>();
    ammoSocket = GetComponentInChildren<XRSocketInteractor>();
    interactable = GetComponent<XRGrabInteractable>();
    rb = GetComponent<Rigidbody>();

    // Gun listeners
    interactable.hoverEntered.AddListener(args => HoverEnterWeapon());
    interactable.hoverExited.AddListener(args => HoverExitWeapon());
    interactable.activated.AddListener(args => ActivateWeapon());
    interactable.deactivated.AddListener(args => DeactivateWeapon());
    interactable.selectExited.AddListener(args => DeactivateWeapon());

    // Ammo socket
    ammoSocket.selectEntered.AddListener(OnAmmoInsert);
    ammoSocket.selectExited.AddListener(OnAmmoRemove);

  }

  private void Start()
  {

  }

  private void HoverEnterWeapon() { /* XR interaction */ }
  private void HoverExitWeapon() { /* XR interaction */ }
  private void ActivateWeapon() => weapon.StartFiring();
  private void DeactivateWeapon() => weapon.StopFiring();

  public void OnAmmoInsert(SelectEnterEventArgs args)
  {
    Ammo ammo = args.interactableObject.transform.GetComponent<Ammo>();
    if (ammo == null) return;

    // Disable physics on rb
    ammo.GetComponent<Rigidbody>().isKinematic = true;

    // Tell weapon we have ammo
    weapon.insertedAmmo = ammo;
    weapon.UpdateAmmoUI();
  }

  public void OnAmmoRemove(SelectExitEventArgs args)
  {
    // Remove ammo from weapon
    weapon.insertedAmmo = null;
    weapon.UpdateAmmoUI();

    // Re-enable physics
    Ammo ammo = args.interactableObject.transform.GetComponent<Ammo>();
    if (ammo == null) return;
    ammo.GetComponent<Rigidbody>().isKinematic = false;

  }
}
