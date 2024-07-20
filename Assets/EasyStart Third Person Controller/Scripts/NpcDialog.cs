using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialog : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject toActivate;
    [SerializeField] private Transform standingPoint;
    private Transform avatar;
    private CharacterController characterController;
    private Animator animator;
    private MonoBehaviour[] avatarScripts;

    void Start()
    {
        // Ensure the main camera, toActivate, and standingPoint are assigned
        if (mainCamera == null)
        {
            Debug.LogError("MainCamera is not assigned.");
        }
        if (toActivate == null)
        {
            Debug.LogError("ToActivate is not assigned.");
        }
        if (standingPoint == null)
        {
            Debug.LogError("StandingPoint is not assigned.");
        }
    }

    private async void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");

        if (other == null)
        {
            Debug.LogError("Collider 'other' is null");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Collider does not have the 'Player' tag");
            return;
        }

        avatar = other.transform;
        Debug.Log("Avatar assigned: " + (avatar != null));

        characterController = avatar.GetComponent<CharacterController>();
        animator = avatar.GetComponent<Animator>();

        if (characterController == null)
        {
            Debug.LogError("CharacterController component not found on the avatar");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator component not found on the avatar");
            return;
        }

        Debug.Log("CharacterController and Animator components found");

        // Disable character controller to prevent movement
        characterController.enabled = false;

        // Disable all other scripts on the avatar to prevent any input or movement
        avatarScripts = avatar.GetComponents<MonoBehaviour>();
        foreach (var script in avatarScripts)
        {
            if (script != this) // Ensure not to disable this NPCDialog script
            {
                script.enabled = false;
            }
        }

        // Disable animator's root motion to stop movement from animations
        animator.applyRootMotion = false;

        // Stop all current animations
        animator.SetBool("run", false);
        animator.SetBool("air", false);
        animator.SetBool("sprint", false);
        animator.SetBool("crouch", false);

        await Task.Delay(50);

        if (standingPoint == null)
        {
            Debug.LogError("StandingPoint is not assigned");
            return;
        }

        // Teleport the avatar to standing point
        avatar.position = standingPoint.position;
        avatar.rotation = standingPoint.rotation;

        if (mainCamera == null || toActivate == null)
        {
            Debug.LogError("MainCamera or ToActivate is not assigned");
            return;
        }

        // Disable main cam, enable dialog cam
        mainCamera.SetActive(false);
        toActivate.SetActive(true);

        // Display cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Focus the input field
        var inputField = toActivate.GetComponentInChildren<InputField>();
        if (inputField != null)
        {
            Debug.Log("InputField found and activated.");
            inputField.interactable = true;
            inputField.Select();
            inputField.ActivateInputField();
        }
        else
        {
            Debug.LogError("InputField not found in toActivate");
        }
    }

    public void Recover()
    {
        Debug.Log("Recover method called");

        if (avatar == null)
        {
            Debug.LogError("Avatar is null");
            return;
        }

        if (characterController == null || animator == null)
        {
            Debug.LogError("CharacterController or Animator component is not found on the avatar");
            return;
        }

        // Enable character controller
        characterController.enabled = true;

        // Enable all other scripts on the avatar
        foreach (var script in avatarScripts)
        {
            script.enabled = true;
        }

        // Enable animator's root motion
        animator.applyRootMotion = true;

        if (mainCamera == null || toActivate == null)
        {
            Debug.LogError("MainCamera or ToActivate is not assigned");
            return;
        }

        mainCamera.SetActive(true);
        toActivate.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
