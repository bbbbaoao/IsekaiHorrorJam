using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "New Mask Item", menuName = "Inventory system /Scriptable Objects/Item Object", order = 0)]
public class ItemObject : ScriptableObject
{
    [SerializeField] private string MaskName;
    [SerializeField] private int MaskSet;
    [SerializeField] private bool evil;
    [SerializeField] private Sprite MaskImage;
    
}
