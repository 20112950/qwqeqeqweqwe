using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemStruct {
    
    public Item[] item;

    public ItemStructType item_struct_type;

    public Transform transform;

    public void CreateItemStruct(ItemStructType type ,GameObject item_prefab)
    {
        transform = new GameObject().transform;
        transform.SetParent(LevelManager.instance.item_struct_parent);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        switch (type)
        {
            case ItemStructType.ONE:
                CreateOne(item_prefab , type ,(ItemType)CreateItemType());
                break;
            case ItemStructType.TWO_HORIZONTAL:
            case ItemStructType.TWO_LEFT_INCLINED:
            case ItemStructType.TWO_RIGHT_INCLIED:
                ItemType[] item_type = new ItemType[2];
                item_type[0] = (ItemType) CreateItemType();
                item_type[1] = (ItemType)CreateItemType();
                CreateTwo(item_prefab , type , item_type);
                break;
        }

    }

    public void CreateItemStructByItemType(ItemStructType type, GameObject item_prefab ,ItemType[] item_types)
    {
        transform = new GameObject().transform;
        transform.SetParent(LevelManager.instance.item_struct_parent);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        switch (type)
        {
            case ItemStructType.ONE:
                CreateOne(item_prefab, type ,item_types[0]);
                break;
            case ItemStructType.TWO_HORIZONTAL:
            case ItemStructType.TWO_LEFT_INCLINED:
            case ItemStructType.TWO_RIGHT_INCLIED:
                CreateTwo(item_prefab, type, item_types);
                break;
        }

    }

    private void CreateOne(GameObject item_prefab , ItemStructType item_struct_type ,ItemType type)
    {
        item = new Item[1];
        GameObject item_obj = GameObject.Instantiate(item_prefab);
        item[0] = item_obj.GetComponent<Item>();
        item[0].transform.SetParent(transform);
        item[0].transform.localPosition = Vector3.zero;
        item[0].transform.localRotation = Quaternion.identity;
        item[0].transform.localScale = Vector3.one;
        item[0].SetItemType((int)type);
        this.item_struct_type = item_struct_type;
    }

    private void CreateTwo(GameObject item_prefab , ItemStructType type ,ItemType[] item_types)
    {
        item = new Item[2];
        GameObject item_obj = GameObject.Instantiate(item_prefab);
        item[0] = item_obj.GetComponent<Item>();
        item[0].transform.SetParent(transform);
        item[0].transform.localRotation = Quaternion.identity;
        item[0].transform.localScale = Vector3.one;
        item[0].SetItemType((int)item_types[0]);
        GameObject item_obj1 = GameObject.Instantiate(item_prefab);
        item[1] = item_obj1.GetComponent<Item>();
        item[1].transform.SetParent(transform);
        item[1].transform.localRotation = Quaternion.identity;
        item[1].transform.localScale = Vector3.one;
        item[1].SetItemType((int)item_types[1]);
        SetItemTwoPosition(type);

    }

    private int CreateItemType()
    {
        int number = LevelManager.instance.GetCurrentMaxNumber();
        int range = Random.Range(1, 1000);
        if (number <= 2)
        {
            if (range < 500)
            {
                return 2;
            }else
            {
                return 3;
            }
            
        }else
        {
            number =number < 7 ? number + 1 : number;
            range = Random.Range(10, number*10);
            return range/10;
        }
       
    }
    private void SetItemTwoPosition(ItemStructType type)
    {
        switch (type)
        {
            case ItemStructType.TWO_HORIZONTAL:
                item[0].transform.localPosition = new Vector3(-Map.interval_x / 2 - Map.square_with, 0, 0);
                item[1].transform.localPosition = new Vector3(Map.interval_x / 2 + Map.square_with, 0, 0);
                break;
            case ItemStructType.TWO_LEFT_INCLINED:
                item[0].transform.localPosition = new Vector3((-Map.interval_x / 2 - Map.square_with) * Mathf.Cos(Mathf.Deg2Rad * 60), (-Map.interval_x / 2 - Map.square_with) * Mathf.Sin(Mathf.Deg2Rad * 60), 0);
                item[1].transform.localPosition = new Vector3((Map.interval_x / 2 + Map.square_with) * Mathf.Cos(Mathf.Deg2Rad * 60), (Map.interval_x / 2 + Map.square_with) * Mathf.Sin(Mathf.Deg2Rad * 60), 0);
                break;
            case ItemStructType.TWO_RIGHT_INCLIED:
                item[0].transform.localPosition = new Vector3((-Map.interval_x / 2 - Map.square_with) * Mathf.Cos(Mathf.Deg2Rad * 60), (Map.interval_x / 2 + Map.square_with) * Mathf.Sin(Mathf.Deg2Rad * 60), 0);
                item[1].transform.localPosition = new Vector3((Map.interval_x / 2 + Map.square_with) * Mathf.Cos(Mathf.Deg2Rad * 60), (-Map.interval_x / 2 - Map.square_with) * Mathf.Sin(Mathf.Deg2Rad * 60), 0);
                break;
        }
        item_struct_type = type;
    }
}

public enum ItemStructType
{
    ONE,
    TWO_HORIZONTAL,
    TWO_LEFT_INCLINED,
    TWO_RIGHT_INCLIED,

}

