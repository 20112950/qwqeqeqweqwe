using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class Square : MonoBehaviour{
    public GameObject pre_square_block;
    public GameObject background_prefab;
    // 行
    public int hang;
    // 列 
    public int lie;

    public Item item;

    public SquareBlock square_block;

    public Item match_struct_item;

    public bool square_checked = false;

    public bool square_checked2 = false;

    // 为了消除移动记录
    public List<Square> search_last_square = new List<Square>();

    public int search_value;
    public void Init(int row ,int col ,Vector3 position)
    {
        this.lie = row;
        this.hang = col;
        transform.SetParent(LevelManager.instance.square_parent);
        transform.localPosition = position;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        transform.gameObject.SetActive(true);
        EventTriggerListener.Get(gameObject).onClick = OnClickSquare;
    }

    private void OnClickSquare(GameObject obj)
    {
        if(LevelManager.game_handle_state == GameHandleState.Hammer)
        {
            UIGame _ui_game = UIManager.Instance.GetUI(emUIWindow.emUIWindow_Game) as UIGame;
            if (this.item != null)
            {
                GameObject.Destroy(this.item.gameObject);
                this.item = null;
                _ui_game.Hammer(null);
            }
            else if (this.square_block != null)
            {
                GameObject.Destroy(this.square_block.gameObject);
                this.square_block = null;
                _ui_game.Hammer(null);
            }
        }
        
    }

    public void MatchedItemStruct(Item collider)
    {
        match_struct_item = collider;
        //调用DOmove方法来让图片移动
        Tweener tweener = transform.DOScale(new Vector3(1.1f,1.1f,1), 0.1f);
        //设置移动类型
        tweener.SetEase(Ease.InSine);
    }

    public void MatchedItemStructReset(System.Action callback = null)
    {
        match_struct_item = null;
        //调用DOmove方法来让图片移动
        Tweener tweener = transform.DOScale(Vector3.one, 0.1f);
        //设置移动类型
        tweener.SetEase(Ease.InSine);
        tweener.OnComplete(()=> 
        {
            if (callback != null)
            {
                callback();
            }
        });
    }

    public void ShowPreSquareBlock(bool state)
    {
        pre_square_block.SetActive(state);
    }

}
