using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemStructManager  {

    public ItemStruct current_item_struct;

    public bool draging = false;

    private bool down_item = false;

    private Tweener tweener_rotate;

    private bool rotate_state = false;
    //private static ItemStructManager _instatnce;
    //public static ItemStructManager instatnce
    //{
    //    get
    //    {
    //        if (_instatnce == null)
    //        {
    //            _instatnce = new ItemStructManager();
    //        }
    //        return _instatnce;
    //    }
    //}

    public void CreateItemStruct(GameObject item_prefab)
    {
        ItemStruct item_struct = new ItemStruct();
        int range = UnityEngine.Random.Range(0, 70);
        if (range <=10)
            item_struct.CreateItemStruct(ItemStructType.ONE, item_prefab);
        if (range >10 && range<=30)
            item_struct.CreateItemStruct(ItemStructType.TWO_HORIZONTAL, item_prefab);
        if (range >30 && range<=50)
            item_struct.CreateItemStruct(ItemStructType.TWO_LEFT_INCLINED, item_prefab);
        if (range >50 && range<=70)
            item_struct.CreateItemStruct(ItemStructType.TWO_RIGHT_INCLIED, item_prefab);
        current_item_struct = item_struct;
    }

    public void ReCreateItemStruct(GameObject item_prefab ,ItemType[] item_types ,ItemStructType struct_type)
    {
        GameObject.DestroyImmediate(current_item_struct.transform.gameObject);
        ItemStruct item_struct = new ItemStruct();
        item_struct.CreateItemStructByItemType( struct_type , item_prefab ,item_types);
        current_item_struct = item_struct;
    }

    public void SetSquareItemByType(GameObject item_prefab ,Transform parent , Square square ,ItemType type)
    {
        Item item = new Item();
        GameObject item_obj = GameObject.Instantiate(item_prefab);
        item = item_obj.GetComponent<Item>();
        item.transform.SetParent(parent);
        item.transform.localPosition = square.transform.localPosition;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
        item.SetItemType((int)type);
        square.item = item;
    }

    public void TrashItemStruct(float time, System.Action callback)
    {
        if(LevelManager.game_handle_state == GameHandleState.NULL)
        {
            LevelManager.game_handle_state = GameHandleState.Trash_Item_Struct;
            LevelManager.instance.StartCoroutine(TrashItemStructItr(time, callback));
        }
     
    }

    public IEnumerator TrashItemStructItr(float time ,System.Action callback)
    {
        if (current_item_struct != null)
        {
            GameObject.DestroyImmediate(current_item_struct.transform.gameObject);
        }
        yield return new WaitForSeconds(time);
        if (callback!=null)
        {
            callback();
        }
        LevelManager.game_handle_state = GameHandleState.NULL;
    }

    public void RegistEvent(GameObject item_struct_background)
    {
        EventTriggerListener.Get(item_struct_background).onDown = OnDownItemStruct;
#if UNITY_EDITOR
        EventTriggerListener.Get(item_struct_background).onDragBegin = OnDragBeginItemStruct;
        EventTriggerListener.Get(item_struct_background).onDragEnd = OnDragEndItemStruct;
        EventTriggerListener.Get(item_struct_background).onClick = OnClickItemStruct;
#endif
    }

    private void OnDownItemStruct(GameObject go, PointerEventData ev)
    {
        if(LevelManager.game_handle_state== GameHandleState.NULL)
        {
            down_item = true;
        }
    }

    private void OnDragBeginItemStruct(GameObject go, PointerEventData ev)
    {
#if UNITY_EDITOR
        Vector3 pos = LevelManager.instance.m_camera.ScreenToWorldPoint(Input.mousePosition);
        pos = new Vector3(pos.x + extra_pos, pos.y + extra_pos, 0);
        //调用DOmove方法来让图片移动
        Tweener tweener = current_item_struct.transform.DOMove(pos, 0.05f);
        //设置这个Tween不受Time.scale影响
        tweener.SetUpdate(true);
        //设置移动类型
        tweener.SetEase(Ease.InSine);
        tweener.OnComplete(() =>
        {
            draging = true;
            LevelManager.game_handle_state = GameHandleState.DRAGING_SQUARE;
            FSoundManager.PlaySound("Stripes_bonus");
        });
#else
        int count = Input.touchCount;
        if (count == 1)
        {
            Vector3 pos = LevelManager.instance.m_camera.ScreenToWorldPoint(Input.GetTouch(0).position);
            pos = new Vector3(pos.x + extra_pos, pos.y + extra_pos, 0);
            //调用DOmove方法来让图片移动
            Tweener tweener = current_item_struct.transform.DOMove(pos, 0.05f);
            //设置这个Tween不受Time.scale影响
            tweener.SetUpdate(true);
            //设置移动类型
            tweener.SetEase(Ease.InSine);
            tweener.OnComplete(() =>
            {
                draging = true;
                LevelManager.game_handle_state = GameHandleState.DRAGING_SQUARE;
            });
        }
#endif
    }


    private void OnClickItemStruct(GameObject go)
    {
        if (!draging)
        {
            if (current_item_struct.item_struct_type != ItemStructType.ONE)
            {
                for (int i = 0; i < current_item_struct.item.Length; i++)
                {
                    Tweener tweener_number = current_item_struct.item[i].number.transform.DOLocalRotate(new Vector3(0, 0, current_item_struct.item[i].number.transform.localEulerAngles.z + 60), 0.1f, RotateMode.FastBeyond360);
                    tweener_number.SetEase(Ease.InSine);
                }
                Tweener tweener = current_item_struct.transform.DORotate(new Vector3(0, 0, current_item_struct.transform.localEulerAngles.z - 60), 0.1f, RotateMode.FastBeyond360);
                tweener.SetEase(Ease.InSine);
            }
        }
    }

    private void OnDragEndItemStruct(GameObject go, PointerEventData ev)
    {
        DragEndItemStruct();
    }

    public void SaveCurrentDropItemDatas(GameMode game_mode)
    {
        ItemData[] item_data;
        if(game_mode == GameMode.CLASSIC)
        {
            item_data = new ItemData[19];
            
        }else
        {
            item_data = new ItemData[37];

        }
        for (int i = 0; i < LevelManager.instance.squares.Length; i++)
        {
            ItemType type = ItemType.NONE;
            SquareBlockType block_type = SquareBlockType.None;
            if (LevelManager.instance.squares[i].item != null)
            {
                type = LevelManager.instance.squares[i].item.item_type;
            }
            if (LevelManager.instance.squares[i].square_block != null)
            {
                block_type = LevelManager.instance.squares[i].square_block.square_block_type;
            }
            ItemData data = new ItemData
            {
                hang_index = LevelManager.instance.squares[i].hang,
                lie_index = LevelManager.instance.squares[i].lie,
                type = type,
                block_type = block_type,

            };
            item_data[i] = data;
        }
        GameDataCenter.SaveCurrentDropItemDatas(LevelManager.instance.game_mode, item_data);
    }

    private void DragEndItemStruct()
    {
        draging = false;
        if (LevelManager.instance.CheckMatchItemStructSquare())
        {
            GameDataCenter.SetLastScore(LevelManager.instance.game_mode, LevelManager.instance.score);
            GameDataCenter.SetLastItemStruct(LevelManager.instance.game_mode, current_item_struct.item , current_item_struct.item_struct_type);
            SaveCurrentDropItemDatas(LevelManager.instance.game_mode);
            Square[] matched_squares = LevelManager.instance.GetMatchedSquares();
            for (int i = 0; i < matched_squares.Length; i++)
            {
                if (matched_squares[i] != null)
                {
                    matched_squares[i].item = matched_squares[i].match_struct_item;
                    matched_squares[i].item.square = matched_squares[i];
                    matched_squares[i].item.transform.SetParent(LevelManager.instance.item_parent);
                    matched_squares[i].item.gameObject.GetComponent<CircleCollider2D>().enabled = false;
                }
            }
            GameObject.DestroyImmediate(current_item_struct.transform.gameObject);
            for (int i = 0; i < matched_squares.Length; i++)
            {
                if (matched_squares[i] != null)
                {
                    matched_squares[i].MatchedItemStructReset(() =>
                    {
                        if (LevelManager.game_handle_state == GameHandleState.DRAGING_SQUARE)
                        {
                            LevelManager.game_handle_state = GameHandleState.DRAG_SQUARE_FINISHED;
                            LevelManager.instance.ProcessSqualItems(matched_squares);
                        }

                    });
                    //Debug.Log("jieshu11" + matched_squares[i].item);
                    matched_squares[i].item.transform.localPosition = matched_squares[i].transform.localPosition;
                    matched_squares[i].item.transform.localScale = Vector3.one;
                    FSoundManager.PlaySound("drop");

                }
            }
        }
        else
        {
            LevelManager.game_handle_state = GameHandleState.NULL;
            Square[] matched_squares = LevelManager.instance.GetMatchedSquares();
            for (int i = 0; i < matched_squares.Length; i++)
            {
                if (matched_squares[i] != null)
                {
                    matched_squares[i].MatchedItemStructReset();
                }
            }
            FSoundManager.PlaySound("Star_win_01");
            //调用DOmove方法来让图片移动
            Tweener tweener = current_item_struct.transform.DOLocalMove(Vector3.zero, 0.1f);
            //设置移动类型
            tweener.SetEase(Ease.OutElastic);
        }
    }

    float extra_pos = 1f;
    public void Update()
    {
#if UNITY_EDITOR
        if (draging)
        {
            if (current_item_struct != null)
            {
                Vector3 pos = LevelManager.instance.m_camera.ScreenToWorldPoint(Input.mousePosition);
                current_item_struct.transform.position = new Vector3(pos.x, pos.y + extra_pos, 0);
            }
        }
#else
        int count = Input.touchCount;
        if (count>0)
        {
            if (!down_item)
            {
                return;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                //Vector3 pos = LevelManager.instance.camera.ScreenToWorldPoint(Input.GetTouch(0).position);
                //pos = new Vector3(pos.x + extra_pos, pos.y + extra_pos, 0);
                ////调用DOmove方法来让图片移动
                //Tweener tweener = current_item_struct.transform.DOMove(pos, 0.05f);
                ////设置这个Tween不受Time.scale影响
                //tweener.SetUpdate(true);
                ////设置移动类型
                //tweener.SetEase(Ease.InSine);
                //tweener.OnComplete(() =>
                //{
                //    draging = true;
                //    LevelManager.game_handle_state = GameHandleState.DRAGING_SQUARE;
                //});
                if(draging == false)
                {
                    //FSoundManager.PlaySound("Stripes_bonus");
                    draging = true;
                    LevelManager.game_handle_state = GameHandleState.DRAGING_SQUARE;
                }
                if (current_item_struct != null)
                {
                    Vector3 pos = LevelManager.instance.m_camera.ScreenToWorldPoint(Input.GetTouch(0).position);
                    current_item_struct.transform.position = new Vector3(pos.x, pos.y + extra_pos, 0);
                }
            }else if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (draging)
                {
                    DragEndItemStruct();
                }else
                {
                    if (current_item_struct.item_struct_type != ItemStructType.ONE)
                    {
                        if (!rotate_state)
                        {
                            rotate_state = true;
                            for (int i = 0; i < current_item_struct.item.Length; i++)
                            {
                                Tweener tweener_number = current_item_struct.item[i].number.transform.DOLocalRotate(new Vector3(0, 0, current_item_struct.item[i].number.transform.localEulerAngles.z + 60), 0.1f, RotateMode.FastBeyond360);
                                tweener_number.SetEase(Ease.InSine);
                            }
                            tweener_rotate = current_item_struct.transform.DORotate(new Vector3(0, 0, current_item_struct.transform.localEulerAngles.z - 60), 0.1f, RotateMode.FastBeyond360);
                            tweener_rotate.SetEase(Ease.InSine);
                            tweener_rotate.OnComplete(() => 
                            {
                                rotate_state = false;
                                FSoundManager.PlaySound("drop");
                            });
                        }
                    }
                }
                down_item = false;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //if (!draging)
                //{
                //    if (current_item_struct.item_struct_type != ItemStructType.ONE)
                //    {
                //        Tweener tweener = current_item_struct.transform.DORotate(new Vector3(0, 0, (current_item_struct.transform.localEulerAngles.z + 180)%360), 0.2f);
                //        tweener.SetEase(Ease.Flash);
                //    }
                //}
            }
        }
       
#endif
    }
}
