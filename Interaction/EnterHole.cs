using Opsive.ThirdPersonController;
using UnityEngine;

public class EnterHole : MonoBehaviour
{
    [SerializeField]
    private TerrainCollider terrainCollider;

    void OnTriggerEnter(Collider collider)
    {
        Physics.IgnoreCollision(collider, terrainCollider, true);
        CharacterIK ik = collider.GetComponent<CharacterIK>();
        if (ik != null) {
            ik.enabled = false;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Physics.IgnoreCollision(collider, terrainCollider, false);
        CharacterIK ik = collider.GetComponent<CharacterIK>();
        if (ik != null)
        {
            ik.enabled = true;
        }
    }
}
