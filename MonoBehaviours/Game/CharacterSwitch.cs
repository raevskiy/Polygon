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
            GameObject oldCharacter = characters[currentCharacter];
            if (isDead(oldCharacter))
            {
                return;
            }

            cameraController.ChangeState(cameraStates[currentCharacter], false);

            GameObject newCharacter = null;
            do
            {
                NextCharacter();
                newCharacter = characters[currentCharacter];
            } while (!newCharacter.activeInHierarchy || isDead(newCharacter));

            switchControl(oldCharacter, true);
            switchControl(newCharacter, false);
            oldCharacter.GetComponent<BehaviorTreeController>().WaitAtPlace();
            oldCharacter.GetComponentInChildren<KopliSoft.Inventory.StorageInventory>().inventory = npcInventory;

            cameraController.Character = newCharacter;
            cameraController.Anchor = newCharacter.transform;
            cameraController.FadeTransform = fadeTransforms[currentCharacter];
            cameraController.ChangeState(cameraStates[currentCharacter], true);
            cameraController.DeathAnchor = newCharacter.transform;
            newCharacter.GetComponentInChildren<KopliSoft.Inventory.StorageInventory>().inventory = playerInventory;

        }
    }

    private bool isDead(GameObject newCharacter)
    {
        return !newCharacter.GetComponent<CharacterHealth>().IsAlive();
    }

    private void NextCharacter()
    {
        currentCharacter++;
        if (currentCharacter == characters.Length)
        {
            currentCharacter = 0;
        }
    }

    private void switchControl(GameObject character, bool aiControl)
    {
        character.GetComponent<ControllerHandler>().enabled = !aiControl;
        character.GetComponent<ItemHandler>().enabled = !aiControl;
        character.GetComponent<UnityInput>().enabled = !aiControl;

        character.GetComponent<NavMeshAgentBridge>().enabled = aiControl;
        character.GetComponent<NavMeshAgent>().enabled = aiControl;
        character.GetComponent<BehaviorTree>().enabled = aiControl;
        character.GetComponent<DeathmatchAgent>().enabled = aiControl;

    }

    public GameObject getCurrentCharacter()
    {
        return characters[currentCharacter];
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }
}
