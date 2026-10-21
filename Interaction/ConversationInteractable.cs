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
    /// <summary>
    /// Talking to a character: stops its AI, turns it to the interviewer and resumes the AI afterwards.
    /// Subclasses start the actual conversation (Fungus or ink) and call OnConversationEnded.
    /// </summary>
    public abstract class ConversationInteractable : BaseInteractable
    {
        [Tooltip("Should this person turn to the interviewer?")]
        [SerializeField]
        private bool m_ShouldTurnToInerviewer = true;
        [SerializeField]
        private bool canTalkIfDead;
        [Tooltip("Turning speed in degrees per second. Characters in Opsive's Combat movement mode use their own Rotation Speed instead.")]
        [SerializeField]
        private float turnSpeed = 270f;
        [Tooltip("The person turns only if the interviewer is more than this many degrees off; the head and torso look covers smaller angles.")]
        [SerializeField]
        private float turnThreshold = 30f;

        private bool inProgress;
        private PatrolController patrolController;
        private CustomHealth health;
        private BehaviorTree behaviorTree;
        private NavMeshAgent navMeshAgent;
        private DeathmatchAgent deathmatchAgent;
        private RigidbodyCharacterController controller;
        private NavMeshAgentBridge agentBridge;
        private Transform interviewer;
        private Coroutine facing;
        private Vector3 destination;

        protected abstract bool HasConversation();
        protected abstract void StartConversation();

        protected virtual void Start()
        {
            patrolController = GetComponentInParent<PatrolController>();
            health = GetComponentInParent<CustomHealth>();
            behaviorTree = GetComponentInParent<BehaviorTree>();
            navMeshAgent = GetComponentInParent<NavMeshAgent>();
            deathmatchAgent = GetComponentInParent<DeathmatchAgent>();
            controller = GetComponentInParent<RigidbodyCharacterController>();
            agentBridge = GetComponentInParent<NavMeshAgentBridge>();
        }

        public override bool CanInteract()
        {
            return !inProgress
                && m_Interactor != null
                && HasConversation()
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
            if (m_Interactor != null && HasConversation())
            {
                inProgress = true;
                // Stop the AI first: a conversation may end immediately, and the end resumes the AI.
                if (m_ShouldTurnToInerviewer && navMeshAgent != null && IsAlive())
                {
                    destination = navMeshAgent.destination;
                    DisableAI();
                    // Stand still: the person only turns, so it never walks into the interviewer.
                    navMeshAgent.isStopped = true;
                    navMeshAgent.ResetPath();
                    interviewer = m_InteractorGameObject.transform;
                    facing = StartCoroutine(FaceInterviewer());
                }
                StartConversation();
            }
        }

        /// <summary>
        /// Turns in place towards the interviewer for the whole conversation. Replaces the NavMeshAgentBridge,
        /// which would otherwise keep driving the controller with the agent's (now empty) path.
        /// </summary>
        private IEnumerator FaceInterviewer()
        {
            if (agentBridge != null)
            {
                agentBridge.enabled = false;
            }
            var person = controller != null ? controller.transform : navMeshAgent.transform;
            var wait = new WaitForFixedUpdate();
            var turning = false;
            while (true)
            {
                yield return wait;
                var direction = Vector3.ProjectOnPlane(interviewer.position - person.position, Vector3.up);
                if (direction.sqrMagnitude < 0.0001f)
                {
                    continue;
                }
                var target = Quaternion.LookRotation(direction);
                // Start turning past the threshold, then keep turning until really facing the interviewer.
                turning = Quaternion.Angle(person.rotation, target) > (turning ? 2f : turnThreshold);

                if (controller == null)
                {
                    if (turning)
                    {
                        person.rotation = Quaternion.RotateTowards(person.rotation, target, turnSpeed * Time.fixedDeltaTime);
                    }
                }
                else if (controller.Movement == RigidbodyCharacterController.MovementType.Combat)
                {
                    // In Combat mode Opsive turns the character in place towards the look rotation itself.
                    controller.Move(0, 0, turning ? target : person.rotation);
                }
                else
                {
                    // Other modes only turn while moving: keep the controller updated and rotate it directly.
                    controller.Move(0, 0, person.rotation);
                    if (turning)
                    {
                        controller.SetRotation(Quaternion.RotateTowards(person.rotation, target, turnSpeed * Time.fixedDeltaTime));
                    }
                }
                navMeshAgent.nextPosition = person.position;
            }
        }

        private void StopFacingInterviewer()
        {
            if (facing != null)
            {
                StopCoroutine(facing);
                facing = null;
                // A dead character's bridge stays disabled (Opsive disables it on death).
                if (agentBridge != null && IsAlive())
                {
                    agentBridge.enabled = true;
                }
            }
        }

        protected void OnConversationEnded()
        {
            inProgress = false;
            StopFacingInterviewer();
            EventHandler.ExecuteEvent(m_InteractorGameObject, "OnAnimatorInteractionComplete");
            if (m_ShouldTurnToInerviewer && navMeshAgent != null && IsAlive())
            {
                EnableAI();
                if (deathmatchAgent != null && (deathmatchAgent.TargetLayerMask & LayerMask.GetMask("Player")) != 0)
                {
                    navMeshAgent.isStopped = false;
                }
                else
                {
                    SetDestination(destination);
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
