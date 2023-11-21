using Opsive.ThirdPersonController;
using UnityEngine;
using UnityEngine.UI;

namespace KopliSoft.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        public GameObject inventory;
        public GameObject characterSystem;
        public GameObject craftSystem;
        [SerializeField]
        private GameObject protagonist;
        [SerializeField]
        private StorageInventory storageForProtagonist;

        private Inventory craftSystemInventory;
        private CraftSystem cS;
        private Inventory mainInventory;
        private Inventory characterSystemInventory;
        private Tooltip toolTip;

        private InputManager inputManagerDatabase;

        public GameObject HPMANACanvas;

        Text hpText;
        Text manaText;
        Image hpImage;
        Image manaImage;

        float maxHealth = 100;
        float maxMana = 100;
        float maxDamage = 0;
        float maxArmor = 0;

        public float currentHealth = 60;
        float currentMana = 100;
        float currentDamage = 0;
        float currentArmor = 0;

        int normalSize = 3;
        private bool blocked;

        void Start()
        {
            if (inputManagerDatabase == null)
                inputManagerDatabase = (InputManager)Resources.Load("InputManager");

            if (craftSystem != null)
                cS = craftSystem.GetComponent<CraftSystem>();

            if (GameObject.FindGameObjectWithTag("Tooltip") != null)
                toolTip = GameObject.FindGameObjectWithTag("Tooltip").GetComponent<Tooltip>();
            if (inventory != null)
                mainInventory = inventory.GetComponent<Inventory>();
            if (characterSystem != null)
                characterSystemInventory = characterSystem.GetComponent<Inventory>();
            if (craftSystem != null)
                craftSystemInventory = craftSystem.GetComponent<Inventory>();
        }

        public void OnEnable()
        {
            Inventory.ItemEquip += OnBackpack;
            Inventory.UnEquipItem += UnEquipBackpack;

            Inventory.ItemEquip += OnGearItem;
            Inventory.ItemConsumed += OnConsumeItem;
            Inventory.UnEquipItem += OnUnEquipItem;

            Inventory.ItemEquip += EquipWeapon;
            Inventory.UnEquipItem += UnEquipWeapon;
        }

        public void OnDisable()
        {
            Inventory.ItemEquip -= OnBackpack;
            Inventory.UnEquipItem -= UnEquipBackpack;

            Inventory.ItemEquip -= OnGearItem;
            Inventory.ItemConsumed -= OnConsumeItem;
            Inventory.UnEquipItem -= OnUnEquipItem;

            Inventory.UnEquipItem -= UnEquipWeapon;
            Inventory.ItemEquip -= EquipWeapon;
        }

        bool EquipWeapon(Item item)
        {
            if (item.itemType == ItemType.Weapon)
            {
                //TODO
            }
            return true;
        }

        bool UnEquipWeapon(Item item)
        {
            if (item.itemType == ItemType.Weapon)
            {
                //TODO
            }
            return true;
        }

        bool OnBackpack(Item item)
        {
            if (item.itemType == ItemType.Backpack)
            {
                for (int i = 0; i < item.itemAttributes.Count; i++)
                {
                    if (mainInventory == null)
                        mainInventory = inventory.GetComponent<Inventory>();
                    mainInventory.sortItems();
                    if (item.itemAttributes[i].attributeName == "Slots")
                        ChangeInventorySize(item.itemAttributes[i].attributeValue);
                }
            }
            return true;
        }

        bool UnEquipBackpack(Item item)
        {
            if (item.itemType == ItemType.Backpack)
                ChangeInventorySize(normalSize);
            return true;
        }

        void ChangeInventorySize(int size)
        {
            DropTheRestItems(size);

            if (mainInventory == null)
                mainInventory = inventory.GetComponent<Inventory>();

            if (size == 3)
            {
                mainInventory.width = 3;
                mainInventory.height = 1;
            }
            if (size == 6)
            {
                mainInventory.width = 3;
                mainInventory.height = 2;
            }
            else if (size == 12)
            {
                mainInventory.width = 4;
                mainInventory.height = 3;
            }
            else if (size == 16)
            {
                mainInventory.width = 4;
                mainInventory.height = 4;
            }
            else if (size == 24)
            {
                mainInventory.width = 6;
                mainInventory.height = 4;
            }
            else
            {
                return;
            }
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }

        void DropTheRestItems(int size)
        {
            if (size < mainInventory.ItemsInInventory.Count)
            {
                for (int i = size; i < mainInventory.ItemsInInventory.Count; i++)
                {
                    GameObject dropItem = Instantiate(mainInventory.ItemsInInventory[i].itemModel);
                    dropItem.AddComponent<PickUpItem>();
                    dropItem.GetComponent<PickUpItem>().item = mainInventory.ItemsInInventory[i];
                    dropItem.transform.localPosition = protagonist.transform.localPosition;
                }
            }
        }

        public bool OnConsumeItem(Item item)
        {
            foreach (ItemAttribute attribute in item.itemAttributes)
            {
                if ("Health".Equals(attribute.attributeName))
                {
                    if ((currentHealth + attribute.attributeValue) > maxHealth)
                        currentHealth = maxHealth;
                    else
                        currentHealth += attribute.attributeValue;
                }
                else if ("Armor".Equals(attribute.attributeName))
                {
                    if ((currentArmor + attribute.attributeValue) > maxArmor)
                        currentArmor = maxArmor;
                    else
                        currentArmor += attribute.attributeValue;
                }
                else if ("Damage".Equals(attribute.attributeName))
                {
                    if ((currentDamage + attribute.attributeValue) > maxDamage)
                        currentDamage = maxDamage;
                    else
                        currentDamage += attribute.attributeValue;
                }
                else if ("ShotgunAmmo".Equals(attribute.attributeName))
                {
                    Opsive.ThirdPersonController.Wrappers.Inventory opsiveInventory = protagonist.GetComponent<Opsive.ThirdPersonController.Wrappers.Inventory>();
                    if (opsiveInventory.HasItem(1650912923))
                    {
                        opsiveInventory.PickupItem(1548588025, attribute.attributeValue, false, false);
                    } else
                    {
                        Fungus.Flowchart flowchart = GameObject.Find("/Story/Flowcharts/Messages/noSuchWeapon").GetComponent<Fungus.Flowchart>();
                        flowchart.ExecuteBlock("Main");
                        return false;
                    }
                }
            }

            GameObject flowchartGameObject = GameObject.Find("/Story/Flowcharts/Items/item" + item.itemID);
            if (flowchartGameObject != null)
            {
                storageForProtagonist.CloseStorage();
                Fungus.Flowchart flowchart = flowchartGameObject.GetComponent<Fungus.Flowchart>();
                if (flowchart.HasVariable("Interviewer"))
                {
                    flowchart.SetStringVariable("Interviewer", protagonist.tag);
                }

                flowchart.ExecuteBlock("Start");
                return false;
            }

            return true;
        }

        public bool OnGearItem(Item item)
        {
            for (int i = 0; i < item.itemAttributes.Count; i++)
            {
                if (item.itemAttributes[i].attributeName == "Health")
                    maxHealth += item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Mana")
                    maxMana += item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Armor")
                    maxArmor += item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Damage")
                    maxDamage += item.itemAttributes[i].attributeValue;
            }
            return true;
        }

        public bool OnUnEquipItem(Item item)
        {
            for (int i = 0; i < item.itemAttributes.Count; i++)
            {
                if (item.itemAttributes[i].attributeName == "Health")
                    maxHealth -= item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Mana")
                    maxMana -= item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Armor")
                    maxArmor -= item.itemAttributes[i].attributeValue;
                if (item.itemAttributes[i].attributeName == "Damage")
                    maxDamage -= item.itemAttributes[i].attributeValue;
            }
            return true;
        }

        public void BlockInventoryUI()
        {
            blocked = true;
        }

        public void UnblockInventoryUI()
        {
            blocked = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (blocked)
            {
                return;
            }

            if (characterSystem != null && Input.GetKeyDown(inputManagerDatabase.CharacterSystemKeyCode))
            {
                if (!characterSystem.activeSelf)
                {
                    characterSystemInventory.OpenInventory();
                }
                else
                {
                    if (toolTip != null)
                    {
                        toolTip.deactivateTooltip();
                    }
                    characterSystemInventory.CloseInventory();
                }
            }

            if (inventory != null && Input.GetKeyDown(inputManagerDatabase.InventoryKeyCode))
            {
                storageForProtagonist.ToggleStorage();
            }

            if (craftSystem != null && Input.GetKeyDown(inputManagerDatabase.CraftSystemKeyCode))
            {
                if (!craftSystem.activeSelf)
                {
                    craftSystemInventory.OpenInventory();
                }
                else
                {
                    if (cS != null)
                        cS.backToInventory();
                    if (toolTip != null)
                        toolTip.deactivateTooltip();
                    craftSystemInventory.CloseInventory();
                }
            }
        }
    }
}
