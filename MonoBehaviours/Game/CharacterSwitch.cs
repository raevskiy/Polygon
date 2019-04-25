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
    
    private int currentCharacter = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Next Character"))
        {
            GameObject oldCharacter = characters[currentCharacter];
            cameraController.ChangeState(cameraStates[currentCharacter], false);

            GameObject newCharacter = null;
            do
            {
                NextCharacter();
                newCharacter = characters[currentCharacter];
            } while (!newCharacter.activeInHierarchy);

            switchControl(oldCharacter, true);
            switchControl(newCharacter, false);
            oldCharacter.GetComponent<BehaviorTreeController>().WaitAtPlace();

            cameraController.Character = newCharacter;
            cameraController.Anchor = newCharacter.transform;
            cameraController.ChangeState(cameraStates[currentCharacter], true);

        }
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
}
