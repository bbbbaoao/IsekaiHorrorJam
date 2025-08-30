using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [System.Serializable]
    public class InventoryEntry
    {
        public ItemObject item;
        public int amount;

        public InventoryEntry(ItemObject item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }

    [Header("Inventory UI")]
    public GameObject inventoryPanel;           // Assign your UI Panel
    public Transform itemIconContainer;         // Parent with layout group
    public GameObject iconPrefab;               // A prefab with just an Image component

    private bool inventoryVisible = false;
    private PlayerInputActions inputActions;

    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Inventory.performed += ToggleInventoryUI;
    }

    private void OnDisable()
    {
        inputActions.Player.Inventory.performed -= ToggleInventoryUI;
        inputActions.Player.Disable();
    }

    private void ToggleInventoryUI(InputAction.CallbackContext ctx)
    {
        inventoryVisible = !inventoryVisible;
        inventoryPanel.SetActive(inventoryVisible);

        if (inventoryVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            RefreshInventoryUI();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void AddItem(ItemObject itemToAdd, int amount = 1)
    {
        foreach (var entry in inventory)
        {
            if (entry.item == itemToAdd)
            {
                entry.amount += amount;
                RefreshInventoryUI();
                return;
            }
        }

        inventory.Add(new InventoryEntry(itemToAdd, amount));
        RefreshInventoryUI();
    }

    void RefreshInventoryUI()
    {
        // Clear old icons
        foreach (Transform child in itemIconContainer)
        {
            Destroy(child.gameObject);
        }

        // Add icons for current inventory
        foreach (var entry in inventory)
        {
            GameObject icon = Instantiate(iconPrefab, itemIconContainer);
            icon.GetComponent<Image>().sprite = entry.item.icon;
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        RefreshInventoryUI();
    }
}