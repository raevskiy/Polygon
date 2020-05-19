using BehaviorDesigner.Runtime;
using KopliSoft.Behaviour;
using Opsive.DeathmatchAIKit.AI;
using Opsive.ThirdPersonController;
using Opsive.ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.AI;

public class CharacterSwitch : MonoBehaviour
{
    public CameraController cameraController;
    public string[] cameraStates;
    public GameObject[] characters;
    public Transform[] fadeTransforms;

    public KopliSoft.Inventory.Inventory playerInventory;
    public KopliSoft.Inventory.Inventory npcInventory;

    private int currentCharacter = 0;
    private bool locked;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Next Character") && !locked)
        {
            Switch();
        }
    }

    public void Switch()
    {
        GameObject oldCharacter = characters[currentCharacter];
        string oldState = cameraStates[currentCharacter];

        GameObject newCharacter = FindNewCharacter(oldCharacter);
        if (newCharacter == null)
        {
            return;
        }

        cameraController.ChangeState(oldState, false);
        SwitchControl(oldCharacter, newCharacter);
        oldCharacter.GetComponentInChildren<KopliSoft.Inventory.StorageInventory>().inventory = npcInventory;
        newCharacter.GetComponentInChildren<KopliSoft.Inventory.StorageInventory>().inventory = playerInventory;

        SwitchCamera(newCharacter);
    }

    private GameObject FindNewCharacter(GameObject oldCharacter)
    {
        GameObject newCharacter = null;
        do
        {
            NextCharacter();
            newCharacter = characters[currentCharacter];
            if (newCharacter == oldCharacter)
            {
                return null;
            }
        } while (!newCharacter.activeInHierarchy || IsDead(newCharacter));

        return newCharacter;
    }

    private bool IsDead(GameObject character)
    {
        return !character.GetComponent<CharacterHealth>().IsAlive();
    }

    private void NextCharacter()
    {
        currentCharacter++;
        if (currentCharacter == characters.Length)
        {
            currentCharacter = 0;
        }
    }

    private void SwitchControl(GameObject oldCharacter, GameObject newCharacter)
    {
        if (IsDead(oldCharacter))
        {
            DeactivateUserInput(oldCharacter);
        }
        else
        {
            SwitchControlToAi(oldCharacter, true);
            oldCharacter.GetComponent<PatrolController>().WaitAtPlace();
        }
        SwitchControlToAi(newCharacter, false);
    }

    private void SwitchControlToAi(GameObject character, bool aiControl)
    {
        character.GetComponent<ControllerHandler>().enabled = !aiControl;
        character.GetComponent<ItemHandler>().enabled = !aiControl;
        character.GetComponent<UnityInput>().enabled = !aiControl;

        character.GetComponent<NavMeshAgentBridge>().enabled = aiControl;
        character.GetComponent<NavMeshAgent>().enabled = aiControl;
        character.GetComponent<BehaviorTree>().enabled = aiControl;
        character.GetComponent<DeathmatchAgent>().enabled = aiControl;
    }

    private void DeactivateUserInput(GameObject character)
    {
        character.GetComponent<ControllerHandler>().enabled = false;
        character.GetComponent<ItemHandler>().enabled = false;
        character.GetComponent<UnityInput>().enabled = false;
    }

    private void SwitchCamera(GameObject newCharacter)
    {
        cameraController.Deactivate();

        cameraController.Anchor = newCharacter.transform;
        cameraController.FadeTransform = fadeTransforms[currentCharacter];
        cameraController.DeathAnchor = newCharacter.transform;

        cameraController.InitializeCharacter(newCharacter);
        cameraController.ChangeState(cameraStates[currentCharacter], true);
    }

    public GameObject GetCurrentCharacter()
    {
        return characters[currentCharacter];
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }
}
