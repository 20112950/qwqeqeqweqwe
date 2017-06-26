using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Item : MonoBehaviour {

    // 行
    public int row;
    // 列
    public int col;

    public Square square;

    public Image current_item_sprite;

    public Text number;

    public GameObject hammer;

    public Animator synthesis;

    public Animator synthesis_result;

    public Image synthesis_result_image;

    public Color[] item_sprite_color;

    public ItemType item_type = ItemType.ONE;

    public Collider2D square_collider;

    public void SetItemType(int type)
    {
        if (type <= 7)
        {
            item_type = (ItemType)type;
            number.text = type.ToString();
            current_item_sprite.color = item_sprite_color[type - 1];
        }else
        {
            item_type = (ItemType)type;
            current_item_sprite.color = item_sprite_color[type - 1];
            number.text = type.ToString();
        }
   
    }

    public void SetItemSynType(int type)
    {
        if (type <= 7)
            synthesis_result_image.color = item_sprite_color[type - 1];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "square")
        {
            if (LevelManager.instance.struct_manager.draging)
            {
                square_collider = collision;
                LevelManager.instance.CheckMatchItemStructSquare();
                //Debug.Log("OnTriggerEnter2D" + collision);
            }
        }
      
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "square")
        {
            if (LevelManager.instance.struct_manager.draging)
            {
                square_collider = null;
                collision.GetComponent<Square>().MatchedItemStructReset();
                //Debug.Log("OnTriggerExit2D");
            }
        }
        
    }

    public void ShowHammer(bool state)
    {
        this.hammer.transform.localRotation = this.number.transform.localRotation;
        this.hammer.SetActive(state);
    }
}

public enum ItemType
{
    NONE,
    ONE,
    TWO,
    THREE,
    FOUR,
    FIVE,
    SIX,
    THEVEN,
    EIGHT,
    NIGHT,
}


