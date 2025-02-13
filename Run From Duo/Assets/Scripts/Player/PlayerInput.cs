using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeReference] private Teacher teacher;

    [SerializeField] private InputActionReference primaryButtonReference;

    private void Awake()
    {
        primaryButtonReference.action.performed += _ => teacher.StartTalkToTeacher();
        primaryButtonReference.action.canceled += _ => teacher.StopTalkToTeacher();
    }
}
