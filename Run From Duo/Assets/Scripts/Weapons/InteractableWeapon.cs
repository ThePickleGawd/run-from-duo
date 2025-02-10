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
  [HideInInspector] public XRSocketInteractor ammoSocket;
  [HideInInspector] public XRGrabInteractable interactable;

  private Rigidbody rb;

  private void Awake()
  {
    // Find Components
    ammoSocket = GetComponentInChildren<XRSocketInteractor>();
    interactable = GetComponent<XRGrabInteractable>();
    rb = GetComponent<Rigidbody>();

    // Gun listeners
    interactable.hoverEntered.AddListener(args => HoverEnterWeapon());
    interactable.hoverExited.AddListener(args => HoverExitWeapon());
    interactable.activated.AddListener(args => ActivateWeapon());
    interactable.deactivated.AddListener(args => DeactivateWeapon());
  }

  private void Start()
  {
    rb.linearVelocity = Vector3.zero;
  }

  private void HoverEnterWeapon() { /* XR interaction */ }
  private void HoverExitWeapon() { /* XR interaction */ }
  private void ActivateWeapon() { }
  private void DeactivateWeapon() { }
}
