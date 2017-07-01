using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class GameDataCenter
{
    public static bool classic_finish = true;

    public static bool deformation_finish = true;

    public static int classic_score;

    public static ItemStructType class_last_item_struct_type;

    public static ItemType[] class_last_item_struct;

    public static ItemData[] classic_last_item_datas;

    public static string[] classic_item_datas = new string[19];

    public static int deformation_score;

    public static ItemStructType deformation_last_item_struct_type;

    public static ItemType[] deformation_last_item_struct;

    public static ItemData[] deformation_last_item_datas;

    public static string[] deformation_item_datas = new string[37];

    public static ItemData StringToItemData(string data)
    {
        char[] split = new char[1] { ','};
        string[] str = data.Split(split);
        int item_type = int.Parse(str[2]);
        ItemData item_data = new ItemData
        {
            hang_index = int.Parse(str[0]),
            lie_index = int.Parse(str[1]),
            type = (ItemType)item_type,
        };
        return item_data;
    }

    public static void SetLastScore(GameMode game_mode ,int socre)
    {
        if (game_mode == GameMode.CLASSIC)
        {
            classic_score = socre;
        }
        else
        {
            deformation_score = socre;
        }
    }
    public static void SetLastItemStruct(GameMode game_mode , Item[] item ,ItemStructType struct_type)
    {
        if (game_mode == GameMode.CLASSIC)
        {
            class_last_item_struct = new ItemType[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                class_last_item_struct[i] = item[i].item_type;
            }
            class_last_item_struct_type = struct_type;
        }
        else
        {
            deformation_last_item_struct = new ItemType[item.Length];
            for (int i = 0; i < item.Length; i++)
            {
                deformation_last_item_struct[i] = item[i].item_type;
            }
            deformation_last_item_struct_type = struct_type;
        }
    }
    public static void SaveCurrentDropItemDatas(GameMode game_mode , ItemData[] item_datas)
    {
        if(game_mode == GameMode.CLASSIC)
        {
            classic_last_item_datas = item_datas;
        }
        else
        {
            deformation_last_item_datas = item_datas;
        }
    }

}

public struct ItemData
{
    public int hang_index;

    public int lie_index;

    public ItemType type;

    public SquareBlockType block_type;
}
