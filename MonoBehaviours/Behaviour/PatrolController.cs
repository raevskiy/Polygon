using BehaviorDesigner.Runtime;
using Opsive.DeathmatchAIKit.AI;
using Opsive.ThirdPersonController.Wrappers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace KopliSoft.Behaviour
{
    [System.Serializable]
    public class TrackEvent : UnityEvent<string, string>
    {
    }

    [RequireComponent(typeof(CharacterHealth))]
    public class PatrolController : MonoBehaviour
    {
        public static TrackEvent trackEvent = new TrackEvent();

        public List<GameObject> planA;
        public List<GameObject> planB;
        public List<GameObject> planC;
        public List<GameObject> planD;
        public List<GameObject> planE;
        public List<GameObject> planF;
        public List<GameObject> planG;
        public List<GameObject> planH;
        public GameObject waitingPoint;

        [SerializeField]
        protected string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;
        [SerializeField]
        protected GameObject trackedTarget;
        [SerializeField]
        private string characterName;

        private CharacterHealth health;
        private BehaviorTree behaviorTree;
        private DeathmatchAgent deathmatchAgent;
        private NavMeshAgent navMeshAgent;
        private Opsive.ThirdPersonController.Wrappers.Inventory inventory;

        private bool trackedTargetFound;
        private int disableBehaviorCounter = 0;

        void Start()
        {
            health = GetComponent(typeof(CharacterHealth)) as CharacterHealth;
            behaviorTree = GetComponent<BehaviorTree>();
            deathmatchAgent = GetComponent<DeathmatchAgent>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            inventory = GetComponent<Opsive.ThirdPersonController.Wrappers.Inventory>();

            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find("/Fungus/Flowcharts/" + flowchartName).GetComponent<Fungus.Flowchart>();
            }

            trackEvent.AddListener(TrackTargetsInLayers);
        }

        void Update()
        {
            if (flowchart != null && !trackedTargetFound && behaviorTree.GetVariable("Target").GetValue() == trackedTarget)
            {
                trackedTargetFound = true;
                flowchart.ExecuteBlock("Main");
            }
        }

        public void FollowPlanA()
        {
            FollowPlan(planA);
        }

        public void FollowPlanB()
        {
            FollowPlan(planB);
        }

        public void FollowPlanC()
        {
            FollowPlan(planC);
        }

        public void FollowPlanD()
        {
            FollowPlan(planD);
        }

        public void FollowPlanE()
        {
            FollowPlan(planE);
        }

        public void FollowPlanF()
        {
            FollowPlan(planF);
        }

        public void FollowPlanG()
        {
            FollowPlan(planG);
        }

        public void FollowPlanH()
        {
            FollowPlan(planH);
        }

        public void WaitAtPlace()
        {
            waitingPoint.transform.position = transform.position;
            GoToWaypoint(waitingPoint);
        }

        public void CheckAlarm(GameObject alarmSource)
        {
            if (planA != null && planA.Count > 0)
            {
                behaviorTree.SetVariableValue("Waypoints", planA);
            }
            
            health.Damage(0, Vector3.zero, Vector3.zero, alarmSource);
        }

        public void GoToWaypoint(GameObject waypoint)
        {
            DisableBehavior();
            List<GameObject> gameObjects = new List<GameObject>
            {
                waypoint
            };
            StartCoroutine(SetWaypoints(gameObjects));
        }

        public void TeleportToWaypoint(GameObject waypoint)
        {
            GoToWaypoint(waypoint);
            navMeshAgent.Warp(waypoint.transform.position);
        }

        private void FollowPlan(List<GameObject> plan)
        {
            DisableBehavior();
            StartCoroutine(SetWaypoints(plan));
        }

        IEnumerator SetWaypoints(List<GameObject> plan)
        {
            while (behaviorTree.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running)
            {
                yield return new WaitForSeconds(.1f);
            }
            
            behaviorTree.SetVariableValue("Waypoints", plan);
            
            EnableBehavior();
        }

        public void EnableBehavior()
        {
            disableBehaviorCounter--;
            if (disableBehaviorCounter == 0)
            {
                behaviorTree.EnableBehavior();
            }
        }

        public void DisableBehavior()
        {
            if (disableBehaviorCounter == 0)
            {
                behaviorTree.DisableBehavior();
            }
            disableBehaviorCounter++;
        }

        public void UntrackEverything()
        {
            deathmatchAgent.TargetLayerMask = new LayerMask();
        }

        public void TrackPlayer()
        {
            deathmatchAgent.TargetLayerMask |= LayerMask.GetMask("Player");
        }

        public void TrackTargetsInLayers(string layersCsv)
        {
            string[] layers = layersCsv.Split(',');
            deathmatchAgent.TargetLayerMask = LayerMask.GetMask(layers);
        }

        private void TrackTargetsInLayers(string characterName, string layersCsv)
        {
            if (characterName.Equals(this.characterName))
            {
                TrackTargetsInLayers(layersCsv);
            }
        }

        public void SendTrackTargetsInLayersEvent(string characterName, string layersCsv)
        {
            trackEvent.Invoke(characterName, layersCsv);
        }
    }
}

