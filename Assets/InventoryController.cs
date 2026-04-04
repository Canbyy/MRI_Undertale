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

    // Start is called before the first frame update
    private void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        if (itemDictionary == null)
        {
            Debug.LogError("ItemDictionary not found in scene!");
        }

        //for (int i = 0; i < slotCount; i++)
        //{
        //    Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
        //    if (i < itemPrefabs.Length)
        //    {
        //        GameObject item = Instantiate(itemPrefabs[i], slot.transform);
        //        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //        slot.currentItem = item;
        //    }
        //}
    }

    public bool AddItem(GameObject itemPrefab)
    {
        //Look for empty slot
        foreach(Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTranform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach(Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTranform.GetSiblingIndex() });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        //Clear inventory panel - avoid duplicates
        foreach(Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Create new slots
        for(int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // Populate slots with saved items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data == null)
            {
                Debug.LogError("data is NULL");
                continue;
            }

            Debug.Log($"Loading itemID: {data.itemID}, slotIndex: {data.slotIndex}");

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

            if (data.slotIndex < 0 || data.slotIndex >= inventoryPanel.transform.childCount)
            {
                Debug.LogError($"Invalid slotIndex: {data.slotIndex}, childCount: {inventoryPanel.transform.childCount}");
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
                Debug.LogError($"No item prefab found for itemID: {data.itemID}");
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
                Debug.LogWarning($"Instantiated item {item.name} has no RectTransform");
            }

            slot.currentItem = item;
        }
    }
}
