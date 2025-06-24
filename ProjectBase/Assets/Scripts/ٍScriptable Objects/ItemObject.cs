using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Mask Item", menuName = "Inventory system /Scriptable Objects/Item Object", order = 0)]
public class ItemObject : ScriptableObject
{
    [SerializeField] private string MaskName;
    [SerializeField] private string MaskLayerName;
    [SerializeField] private string MaskDescription;
    
}
