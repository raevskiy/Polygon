using KopliSoft.Behaviour;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmProducer : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;

    public void Alarm(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);
        foreach (Collider collider in hitColliders)
        {
            BehaviorTreeController controller = collider.GetComponent<BehaviorTreeController>();
            if (controller != null)
            {
                controller.CheckAlarm(gameObject);
            }
        }
    }
}
