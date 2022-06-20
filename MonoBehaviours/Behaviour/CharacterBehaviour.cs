using BehaviorDesigner.Runtime;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    [SerializeField]
    private bool ableToDriveBlimp;
    private bool driving;
    [SerializeField]
    private bool playerControlled;

    [SerializeField]
    public ExternalBehaviorTree soloTree;
    [SerializeField]
    public ExternalBehaviorTree teamTree;

    private BehaviorTree behaviorTree;

    private void Start()
    {
        behaviorTree = GetComponent<BehaviorTree>();
    }

    public void switchToSolo()
    {
        behaviorTree.ExternalBehavior = soloTree;
    }

    public void switchToTeam()
    {
        behaviorTree.ExternalBehavior = teamTree;
    }
    
    public bool IsDriving()
    {
        return driving;
    }

    public void SetDriving(bool driving)
    {
        this.driving = driving;
    }

    public bool IsPlayerControlled()
    {
        return playerControlled;
    }

    public void SetPlayerControlled(bool playerControlled)
    {
        this.playerControlled = playerControlled;
    }

    public bool IsAbleToDriveBlimp()
    {
        return ableToDriveBlimp;
    }
}
