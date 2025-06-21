using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private  List<Item> items; // List of items that can be picked up
    [SerializeField] private GameObject itemHolder;


    private void OnCollisionEnter(Collision collision)
    {
   
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with interactable object.");
            // Check if the player has an Inventory component
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                OnmouseVisible(true); // Show the cursor
                itemHolder.SetActive(true); // Hide the item holder
                foreach (Item item in items)
                {
                    if (item != null && item.itemObject != null)
                    {
                        // Add the item to the player's inventory
                        item.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    public void OnmouseVisible(bool visible)
    {
        Cursor.visible = visible;
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            Cursor.visible = false;
        }
    }
}
