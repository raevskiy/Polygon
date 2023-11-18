using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KopliSoft.Inventory
{
    public class BlueprintDatabase : ScriptableObject
    {
        [SerializeField]
        public List<Blueprint> blueprints = new List<Blueprint>();
    }
}
