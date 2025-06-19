using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Inventory inventory; // Reference to the Inventory scriptable object

 

    private void OnCollisionEnter(Collision collision)
    {
        // Try to get the Item component from the collided object
        Item itemComponent = collision.gameObject.GetComponent<Item>();
        if (itemComponent != null && itemComponent.itemObject != null)
        {
            // Add the item to the inventory
            inventory.AddItem(itemComponent.itemObject, 1);
            Debug.Log($"Added {itemComponent.itemObject.name} to inventory.");

            // Optionally deactivate the item in the scene after picking it up
            collision.gameObject.SetActive(false);
        }
    }
}
