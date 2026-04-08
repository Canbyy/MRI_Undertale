using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged; //event to notify quest system (or any other system that needs to know!)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;

        //Check if we have this item type in inventory
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    //Same item, stack them
                    slotItem.AddToStack();
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        //Look for empty slot
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTranform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTranform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        //Clear inventory panel - avoid duplicates
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // Populate slots with saved items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data == null)
            {
                Debug.LogError("InventorySaveData is NULL");
                continue;
            }

            Debug.Log($"Processing itemID: {data.itemID}, slotIndex: {data.slotIndex}, quantity: {data.quantity}");

            if (inventoryPanel == null)
            {
                Debug.LogError("inventoryPanel is NULL");
                return;
            }

            if (itemDictionary == null)
            {
                Debug.LogError("itemDictionary is NULL");
                return;
            }

            if (data.slotIndex < 0 || data.slotIndex >= slotCount)
            {
                Debug.LogError($"Invalid slotIndex: {data.slotIndex} (slotCount: {slotCount})");
                continue;
            }

            if (data.slotIndex >= inventoryPanel.transform.childCount)
            {
                Debug.LogError($"Slot index {data.slotIndex} does not exist in inventoryPanel (childCount: {inventoryPanel.transform.childCount})");
                continue;
            }

            Transform slotTransform = inventoryPanel.transform.GetChild(data.slotIndex);
            if (slotTransform == null)
            {
                Debug.LogError($"slotTransform is NULL at index {data.slotIndex}");
                continue;
            }

            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null)
            {
                Debug.LogError($"No Slot component found on child index {data.slotIndex}");
                continue;
            }

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null)
            {
                Debug.LogError($"itemPrefab is NULL for itemID: {data.itemID}");
                continue;
            }

            GameObject item = Instantiate(itemPrefab, slot.transform);

            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                Debug.LogWarning($"RectTransform missing on item prefab: {data.itemID}");
            }

            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent == null)
            {
                Debug.LogWarning($"Item component missing on prefab: {data.itemID}");
            }
            else if (data.quantity > 1)
            {
                itemComponent.quantity = data.quantity;
                itemComponent.UpdateQuantityDisplay();
            }

            slot.currentItem = item;
        }
        RebuildItemCounts();
    }

    public void RemoveItemsFromInventory(int itemID, int ammountToRemove)
    {
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            if (ammountToRemove <= 0) break;
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(ammountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                ammountToRemove -= removed;

                if (item.quantity == 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }

        RebuildItemCounts();
    }
}
