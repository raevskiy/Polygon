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
            PatrolController controller = collider.GetComponent<PatrolController>();
            if (controller != null)
            {
                controller.CheckAlarm(gameObject);
            }
        }
    }
}
