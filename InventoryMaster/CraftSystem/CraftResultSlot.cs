﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KopliSoft.Inventory
{
    public class CraftResultSlot : MonoBehaviour
    {
        CraftSystem craftSystem;
        public int temp = 0;
        GameObject itemGameObject;

        void Start()
        {
            //inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
            craftSystem = transform.parent.GetComponent<CraftSystem>();
            Inventory inventory = transform.parent.gameObject.GetComponent<Inventory>();

            itemGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/Item") as GameObject);
            itemGameObject.transform.SetParent(this.gameObject.transform);
            itemGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
            itemGameObject.GetComponent<DragItem>().enabled = false;
            itemGameObject.SetActive(false);
            itemGameObject.transform.GetChild(1).GetComponent<Text>().enabled = true;
            itemGameObject.transform.GetChild(1).GetComponent<RectTransform>().localPosition =
                new Vector2(inventory.positionNumberX, inventory.positionNumberY);

        }

        void Update()
        {
            if (craftSystem.possibleItems.Count != 0)
            {
                itemGameObject.GetComponent<ItemOnObject>().item = craftSystem.possibleItems[temp];
                itemGameObject.SetActive(true);
            }
            else
            {
                itemGameObject.SetActive(false);
            }
        }
    }
}
