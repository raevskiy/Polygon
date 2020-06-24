using System.Collections;
using UnityEngine;
using Opsive.ThirdPersonController;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;
using KopliSoft.Behaviour;
using KopliSoft.Game;
using Opsive.DeathmatchAIKit.AI;

namespace KopliSoft.Interaction
{
    public class DialogInteractable : BaseInteractable
    {
        [Tooltip("Should this person turn to the interviewer?")]
        [SerializeField]
        private bool m_ShouldTurnToInerviewer = true;
        [SerializeField]
        private string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;
        [SerializeField]
        private bool canTalkIfDead;

        private bool inProgress;
        private PatrolController patrolController;
        private CustomHealth health;
        private BehaviorTree behaviorTree;
        private NavMeshAgent navMeshAgent;
        private DeathmatchAgent deathmatchAgent;
        private Vector3 destination;

        void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find(flowchartName).GetComponent<Fungus.Flowchart>();
            }

            patrolController = GetComponentInParent<PatrolController>();
            health = GetComponentInParent<CustomHealth>();
            behaviorTree = GetComponentInParent<BehaviorTree>();
            navMeshAgent = GetComponentInParent<NavMeshAgent>();
            deathmatchAgent = GetComponentInParent<DeathmatchAgent>();
        }

        public override bool CanInteract()
        {
            return !inProgress
                && m_Interactor != null
                && flowchart != null
                && !IsInCriminalMode()
                && (canTalkIfDead || health == null || IsAlive());
        }

        private bool IsAlive()
        {
            return health != null && health.CurrentHealth > 0;
        }

        public bool IsInProgress()
        {
            return inProgress;
        }

        public override void Interact()
        {
            if (m_Interactor != null && flowchart != null)
            {
                inProgress = true;
                if (flowchart.HasVariable("Interviewer"))
                {
                    flowchart.SetStringVariable("Interviewer", m_InteractorGameObject.tag);
                }
                flowchart.ExecuteBlock("Start");
                if (m_ShouldTurnToInerviewer && navMeshAgent != null && IsAlive())
                {
                    destination = navMeshAgent.destination;
                    DisableAI();
                    SetDestination(m_InteractorGameObject.transform.position);
                    StartCoroutine(CheckFacingInterviewer());
                }
                Fungus.BlockSignals.OnBlockEnd += OnBlockEnd;
            }
        }

        IEnumerator CheckFacingInterviewer()
        {
            Vector3 dir = Vector3.ProjectOnPlane(m_InteractorGameObject.transform.position - navMeshAgent.transform.position, Vector3.up).normalized;
            while (Vector3.Dot(navMeshAgent.transform.forward, dir) < 0.9f)
            {
                yield return new WaitForSeconds(.1f);
            }
            navMeshAgent.isStopped = true;
        }

        void OnBlockEnd(Fungus.Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                Fungus.BlockSignals.OnBlockEnd -= OnBlockEnd;
                inProgress = false;
                EventHandler.ExecuteEvent(m_InteractorGameObject, "OnAnimatorInteractionComplete");
                if (m_ShouldTurnToInerviewer && navMeshAgent != null && IsAlive())
                {
                    EnableAI();
                    if ((deathmatchAgent.TargetLayerMask & LayerMask.GetMask("Player")) != 0)
                    {
                        navMeshAgent.isStopped = false;
                    } else
                    {
                        SetDestination(destination);
                    }
                }
            }
        }

        private void EnableAI()
        {
            if (patrolController != null)
            {
                patrolController.EnableBehavior();
            }
            else
            {
                behaviorTree.EnableBehavior();
            }
        }

        private void DisableAI()
        {
            if (patrolController != null)
            {
                patrolController.DisableBehavior();
            }
            else
            {
                behaviorTree.DisableBehavior();
            }
        }

        protected void SetDestination(Vector3 destination)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }
    }
}
