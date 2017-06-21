using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

public class LevelManager : MonoBehaviour {

    public Square[] squares;

    public Camera m_camera;

    public GameObject item_struct_background;

    public Transform square_parent;

    public Transform square_block_parent;

    public Transform item_struct_parent;

    public Transform item_parent;

    public Map map;

    public ItemStructManager struct_manager;

    public GameObject square_prefab;

    public GameObject item_prefab;

    public GameObject square_block_prefab;

    public GameObject square_prefab_deformation;

    public GameObject item_prefab_deformation;

    static public GameState game_state;

    static public GameHandleState game_handle_state;

    private Square[] matched_square;

    static public LevelManager instance;

    public int[] square_hang_count;

    private static readonly float wait_remove_squares_time = 0.25f;

    private Queue<Square> queue_remove = new Queue<Square>();

    public GameMode game_mode = GameMode.CLASSIC;
    // 单次消除的次数
    private int remove_count = 0;
    // 下一次生产砖块的位置
    private int _next_block_pisition = 0;
    private void Awake()
    {
        instance = this;
        map = new Map();
        struct_manager = new ItemStructManager();
        struct_manager.RegistEvent(item_struct_background);
        game_state = GameState.PLAYING;
    }

    public void CreateMaps(GameMode mode)
    {  
        InitGame(mode);
        map.CrerateSquares(GetSquarePrefab(), mode, (s)=> 
        {
            this.squares = s;
            ShowPreSquareBlock(true);
        });
        struct_manager.CreateItemStruct(GetItemPrefab());

    }
    public GameObject GetSquarePrefab()
    {
        switch (this.game_mode)
        {
            case GameMode.CLASSIC:
                return square_prefab;
            case GameMode.DEFORMATION:
                return square_prefab_deformation;
        }
        return square_prefab;
    }

    public GameObject GetItemPrefab()
    {
        switch (this.game_mode)
        {
            case GameMode.CLASSIC:
                return item_prefab;
            case GameMode.DEFORMATION:
                return item_prefab_deformation;
        }
        return item_prefab;
    }

    private void Update()
    {
        if(game_state == GameState.PLAYING)
        {
            struct_manager.Update();
            //if (LevelManager.game_handle_state == GameHandleState.DRAG_SQUARE_FINISHED)
            //{
            //    LevelManager.game_handle_state = GameHandleState.NULL;      
            //}
            
        }
    }

    public void ProcessGetSqualItems(Square squares)
    {
        if (squares != null && !squares.square_checked &&  squares.item!=null)
        {
            List<Square> neighbor_square = new List<Square>();
            squares.square_checked = true;
            squares.square_checked2 = true;
            neighbor_square.Add(squares);   
            CheckSquares(squares, neighbor_square, squares.item.item_type);
            if (neighbor_square.Count >= 3)
            {
                checked_squares.Add(neighbor_square);
            }
        }
    }

    public void ProcessSqualItems(Square[] changeed_squares)
    {
        remove_count = 0;
        LevelManager.game_handle_state = GameHandleState.Eliminating;
        for (int i = 0; i < this.squares.Length; i++)
        {
            if (this.squares[i] != null)
            {
                this.squares[i].square_checked = false;
                this.squares[i].square_checked2 = false;
                this.squares[i].search_last_square.Clear();
            }
        }
        checked_squares.Clear();
        // reset need removed squares queue
        queue_remove.Clear();
        if (changeed_squares!=null && changeed_squares.Length == 1)
        {
            queue_remove.Enqueue(changeed_squares[0]);
        }else if(changeed_squares != null && changeed_squares.Length == 2)
        {
            changeed_squares = SortArrary(changeed_squares);
            if(changeed_squares[0].item.item_type == changeed_squares[1].item.item_type)
            {
                queue_remove.Enqueue(changeed_squares[0]);
            }else
            {
                for (int i = 0; i < changeed_squares.Length; i++)
                {
                    queue_remove.Enqueue(changeed_squares[i]);
                }
            }
        }
        RemoveSquareFromQueue(queue_remove);
    }

    private Square[] SortArrary(Square[] arrary)
    {
        Square temp;
        for (int i = 0; i < arrary.Length; i++)
        {
            for (int j = i + 1; j < arrary.Length; j++)
            {
                if (arrary[j].item.item_type < arrary[i].item.item_type)
                {
                    temp = arrary[j];
                    arrary[j] = arrary[i];
                    arrary[i] = temp;
                }
            }
        }
        return arrary;
    }

    private void RemoveSquareFromQueue(Queue<Square> queue)
    {
        Square square = null;
        if (queue.Count > 0)
        {
            square = queue.Dequeue();
        }else
        {
            Debug.Log("finish");
        }
        ProcessGetSqualItems(square);
        if (checked_squares.Count > 0)
        {
            Square generate_square = null;
            RemoveSquares(checked_squares[0], (t) => 
            {
                generate_square = t;
                for (int i = 0; i < this.squares.Length; i++)
                {
                    if (this.squares[i] != null)
                    {
                        this.squares[i].square_checked = false;
                        this.squares[i].square_checked2 = false;
                        this.squares[i].search_last_square.Clear();
                    }
                }
                checked_squares.Clear();
                if (generate_square != null && generate_square.item != null)
                {
                    //DestroyAroundSquareBlock(generate_square);
                    if (queue.Count > 0)
                    {
                        Square first_square = queue.Peek();
                        if (first_square.item != null && first_square.item.item_type != generate_square.item.item_type)
                        {
                            queue.Enqueue(generate_square);
                        }
                        else if(first_square.item != null)
                        {
                            Square[] temp = SortArrary(queue.ToArray());
                            queue.Clear();
                            for (int i = 0; i < temp.Length; i++)
                            {
                                queue.Enqueue(temp[i]);
                            }
                        }
                    }
                    else
                    {
                        queue.Enqueue(generate_square);
                    }
                    RemoveSquareFromQueue(queue);
                }else
                {
                    RemoveSquareFromQueue(queue);
                }
            });
            remove_count++;
            //DelayCall(wait_remove_squares_time+0.6f, () =>
            //{
            //    for (int i = 0; i < this.squares.Length; i++)
            //    {
            //        if (this.squares[i] != null)
            //        {
            //            this.squares[i].square_checked = false;
            //            this.squares[i].square_checked2 = false;
            //            this.squares[i].search_last_square.Clear();
            //        }
            //    }
            //    checked_squares.Clear();
            //    if (generate_square != null && generate_square.item!=null)
            //    {
            //        //DestroyAroundSquareBlock(generate_square);
            //        if (queue.Count > 0)
            //        {
            //            Square first_square = queue.Peek();
            //            if (first_square.item != null && first_square.item.item_type != generate_square.item.item_type)
            //            {
            //                queue.Enqueue(generate_square);
            //            }
            //        }else
            //        {
            //            queue.Enqueue(generate_square);
            //        }
            //        RemoveSquareFromQueue(queue);
            //    }
            //});
        }
        else
        {
            if (queue.Count == 0)
            {
                LevelManager.game_handle_state = GameHandleState.NULL;
                if (remove_count == 0)
                {
                    GenerateSquareBlock();
                }else
                {
                    if (this.squares[_next_block_pisition].item != null)
                    {
                        ShowPreSquareBlock(true);
                    }
                }
                struct_manager.CreateItemStruct(GetItemPrefab());
                if (!CheckEliminate(struct_manager.current_item_struct))
                {
                    Debug.Log("fail");
                    FSoundManager.PlaySound("Cheers");
                    for (int i = 0; i < this.squares.Length; i++)
                    {
                        if (this.squares[i].item)
                        {
                            DestroyImmediate(this.squares[i].item.gameObject);
                            this.squares[i].item = null;
                        }
                        if (this.squares[i].square_block)
                        {
                            DestroyImmediate(this.squares[i].square_block.gameObject);
                            this.squares[i].square_block = null;
                        }
                        this.squares[i].square_checked = false;
                        this.squares[i].square_checked2 = false;
                        this.squares[i].search_last_square.Clear();
                    }
                    checked_squares.Clear();
                    // reset need removed squares queue
                    queue_remove.Clear();
                    if (this.squares != null)
                    {
                        for (int i = 0; i < this.squares.Length; i++)
                        {
                            DestroyImmediate(this.squares[i].gameObject);
                        }
                    }
                    if (this.struct_manager.current_item_struct.item != null)
                    {
                        for (int i = 0; i < this.struct_manager.current_item_struct.item.Length; i++)
                        {
                            DestroyImmediate(this.struct_manager.current_item_struct.item[i].gameObject);
                        }
                    }
                    if (this.game_mode == GameMode.CLASSIC)
                    {
                        CreateMaps(GameMode.DEFORMATION);
                    }else
                    {
                        CreateMaps(GameMode.CLASSIC);
                    }
                 
                }
            }
            else
            {
                RemoveSquareFromQueue(queue);
            }
        }

    }

    private void ShowPreSquareBlock(bool state)
    {
        if (game_mode == GameMode.DEFORMATION)
        {
            if (state)
            {
                if (GetSquareBlockCount() >=10)
                {
                    return;
                }
                if (_next_block_pisition != -1)
                {
                    this.squares[_next_block_pisition].ShowPreSquareBlock(false);
                }
                _next_block_pisition = GenerateNextSquareBlockPosintion();
                this.squares[_next_block_pisition].ShowPreSquareBlock(state);
            }else
            {
                if (_next_block_pisition != -1)
                {
                    this.squares[_next_block_pisition].ShowPreSquareBlock(state);
                }
            }
           
        }
    }

    private int GetSquareBlockCount()
    {
        int number = 0;
        for (int i = 0; i < this.squares.Length; i++)
        {
            if (this.squares[i].square_block != null)
            {
                number++;
            }
        }
        return number;
    }
    private int GenerateNextSquareBlockPosintion()
    {
        List<Square> empty_squares = new List<Square>();
        for (int i = 0; i < this.squares.Length; i++)
        {
            if (this.squares[i].item == null && this.squares[i].square_block == null)
            {
                empty_squares.Add(this.squares[i]);
            }
        }
        int random = Random.Range(0, empty_squares.Count);
        return map.GetIndex(empty_squares[random].hang, empty_squares[random].lie);
    }

    private void GenerateSquareBlock()
    {
        if(this.game_mode == GameMode.DEFORMATION)
        {
#if false
            List<Square> empty_squares = new List<Square>();
            for (int i = 0; i < this.squares.Length; i++)
            {
                if (this.squares[i].item == null && this.squares[i].square_block == null)
                {
                    empty_squares.Add(this.squares[i]);
                }
            }
            int random = Random.Range(0, empty_squares.Count);
            GameObject square_block_obj = Instantiate(this.square_block_prefab);
            SquareBlock block = square_block_obj.GetComponent<SquareBlock>();
            square_block_obj.transform.SetParent(this.square_block_parent);
            empty_squares[random].square_block = block;
            square_block_obj.transform.localScale = Vector3.one;
            square_block_obj.transform.localRotation = Quaternion.identity;
            square_block_obj.transform.localPosition = empty_squares[random].transform.localPosition;
#else

            if(this.squares[_next_block_pisition].item ==null && this.squares[_next_block_pisition].square_block == null)
            {
                GameObject square_block_obj = Instantiate(this.square_block_prefab);
                SquareBlock block = square_block_obj.GetComponent<SquareBlock>();
                square_block_obj.transform.SetParent(this.square_block_parent);
                this.squares[_next_block_pisition].square_block = block;
                square_block_obj.transform.localScale = Vector3.one;
                square_block_obj.transform.localRotation = Quaternion.identity;
                square_block_obj.transform.localPosition = this.squares[_next_block_pisition].transform.localPosition;
            }
            ShowPreSquareBlock(true);
#endif

        }
    }

    private void DestroyAroundSquareBlock(Square square)
    {
        List<Square> list = GetAroundSquares(square);
        for (int i = 0; i < list.Count; i++)
        {
            if(list[i]!=null && list[i].square_block != null)
            {
                DestroyImmediate(list[i].square_block.gameObject);
                list[i].square_block = null;
                FSoundManager.PlaySound("Cheers");
            }
        }
    }

    private void RemoveSquares(List<Square> squares, System.Action<Square> generate_callback)
    {
        //List<Square> genetate_squares = new List<Square>();
        for (int i = 0; i < checked_squares.Count; i++)
        {
            CombineSquare(checked_squares[i],(t)=> 
            {
                generate_callback(t);
            });
        }   
    }


    private void CombineSquare(List<Square> squares ,System.Action<Square> generate_callback)
    {
        squares.Sort((Square v1, Square v2) => 
        {
            int value1 = v1.search_last_square.Count;
            int value2 = v2.search_last_square.Count;
            return value1.CompareTo(value2);
        }
        );
#if false
        int search_value_max = squares[squares.Count - 1].search_last_square.Count;
        for (int i = 1; i < squares.Count; i++)
        {
            Debug.Log("开始");
            for (int j = 0; j < squares[i].search_last_square.Count; j++)
            {
                Debug.Log(squares[i].search_last_square[j].hang + ":" + squares[i].search_last_square[j].lie);
            }
            StartCoroutine(MoveSquare(squares[i] ,search_value_max));
        }
#elif true
        for (int i = 1; i < squares.Count; i++)
        {
            //Debug.Log("开始");
            //for (int j = 0; j < squares[i].search_last_square.Count; j++)
            //{
            //    Debug.Log(squares[i].search_last_square[j].hang + ":" + squares[i].search_last_square[j].lie);
            //}
            MoveSquarePath(squares[i]);
        }
#else
        MoveSquarePath(squares ,()=> 
        {
            squares[0].item.SetItemType((int)squares[0].item.item_type + 1);
            generate_callback(squares[0]);
        });

#endif
        DelayCall(wait_remove_squares_time, () =>
        {
            PlaySyntheticResult(squares[0],generate_callback);
            //squares[0].item.SetItemType((int)squares[0].item.item_type + 1);
            //generate_callback(squares[0]);
        });
    }

    private void PlaySyntheticResult(Square square, System.Action<Square> generate_callback)
    {
        StartCoroutine(PlaySyntheticResultItr(square ,generate_callback));
    }

    private IEnumerator PlaySyntheticResultItr(Square square ,System.Action<Square> generate_callback)
    {
        if ((int)square.item.item_type == 8)
        {
            List<Square> remove_around = GetAroundSquares(square);
            for (int i = 0; i < remove_around.Count; i++)
            {
                if(remove_around[i]!=null && remove_around[i].item != null)
                {
                    DestroyImmediate(remove_around[i].item.gameObject);
                }
            }
            DestroyImmediate(square.item.gameObject);
            yield return new WaitForSeconds(1);
            generate_callback(null);
        }
        else
        {
            square.item.SetItemType((int)square.item.item_type + 1);
            generate_callback(square);
            square.item.current_item_sprite.enabled = false;
            square.item.SetItemSynType((int)square.item.item_type);
            square.item.synthesis_result.gameObject.SetActive(true);
            square.item.synthesis_result.Play("beiheccheng_1", 0, 0);
            yield return new WaitForSeconds(0.4f);
            if(square!=null && square.item != null)
            {
                square.item.synthesis_result.gameObject.SetActive(false);
                square.item.current_item_sprite.enabled = true;
            }
          
        }
       
       
    }

    private void DelayCall(float time , System.Action callback)
    {
        StartCoroutine(DelayCallItr(time , callback));
    }

    private IEnumerator DelayCallItr(float time , System.Action callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
        {
            callback();
        }
    }

    private void MoveSquarePath(Square src)
    {
        Vector3[] path = new Vector3[src.search_last_square.Count];
        for (int i = 0; i < src.search_last_square.Count ; i++)
        {
            path[i] = src.search_last_square[src.search_last_square.Count-i-1].transform.position;
        }
        src.item.transform.DOPath(path, wait_remove_squares_time).SetEase(Ease.InSine).OnComplete(()=> 
        {
            FSoundManager.PlaySound("Color_bomb");
            GameObject.DestroyImmediate(src.item.gameObject);
            src.item = null;
            src.search_last_square.Clear();
        });
    }

    private void MoveSquarePath(List<Square> squares, System.Action callback = null)
    {
        StartCoroutine(MoveSquarePathItr(squares, callback));
    }

    private IEnumerator MoveSquarePathItr(List<Square> squares, System.Action callback)
    {
        int count = squares[squares.Count - 1].search_last_square.Count;
        int temp_count = count;
        bool have_play = false;
        for (int i = 0; i < squares.Count; i++)
        {
            have_play = false;
            for (int j = 0; j < squares.Count; j++)
            {
                if (temp_count == squares[j].search_last_square.Count && squares[j].search_last_square.Count>0)
                {
                    have_play = true;
                    StartCoroutine(PlayOneMove(squares[j], (count - squares[j].search_last_square.Count + 1) * 0.18f));
                }
            }
            if (have_play)
            {
                temp_count--;
                yield return new WaitForSeconds(0.1f);
            }
           
        }
        squares[0].item.current_item_sprite.enabled = false;
        squares[0].item.SetItemSynType((int)squares[0].item.item_type + 1);
        squares[0].item.synthesis_result.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        squares[0].item.synthesis_result.gameObject.SetActive(false);
        squares[0].item.current_item_sprite.enabled = true;
        if (callback != null)
        {
            callback();
        }
    }
    private IEnumerator PlayOneMove(Square src ,float time)
    {
        if (src != null && src.item != null)
        {
            float angel = angle_360(src.transform.position, src.search_last_square[src.search_last_square.Count - 1].transform.position);
            Debug.Log(angle_360( new Vector3(272, -235, 0), new Vector3(340, -117, 0)));
            src.item.synthesis.transform.parent.transform.localRotation = Quaternion.Euler(0, 0, angel);
            src.item.current_item_sprite.enabled = false;
            src.item.synthesis.gameObject.SetActive(true);
            yield return new WaitForSeconds(time);
            src.item.synthesis.gameObject.SetActive(false);
            GameObject.DestroyImmediate(src.item.gameObject);
            src.item = null;
            src.search_last_square.Clear();
        }
    }
    float angle_360(Vector3 from_, Vector3 to_)
    {
        //两点的x、y值
        float x = from_.x - to_.x;
        float y = from_.y - to_.y;

        //斜边长度
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(x, 2f) + Mathf.Pow(y, 2f));

        //求出弧度
        float cos = x / hypotenuse;
        float radian = Mathf.Acos(cos);

        //用弧度算出角度    
        float angle = 180 / (Mathf.PI / radian);

        if (y < 0)
        {
            angle = -angle;
        }
        else if ((y == 0) && (x < 0))
        {
            angle = 180;
        }
        return angle;
    }

    private List<List<Square>> checked_squares = new List<List<Square>>();
    private void CheckSquares(Square square , List<Square> squares , ItemType type ,int search_value =0)
    {
        List<Square> list = GetAroundSquares(square);
        if (list.Count > 0)
        {
            search_value++;
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].square_checked && list[i].item!=null && list[i].item.item_type == type)
                {
                    list[i].square_checked = true;

                    for (int k = 0; k <= square.search_last_square.Count - 1; k++)
                    {
                        list[i].search_last_square.Add(square.search_last_square[k]);
                    }
                    list[i].search_last_square.Add(square);
                    squares.Add(list[i]);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].square_checked2 && list[i].item != null && list[i].item.item_type == type)
                {
                    list[i].square_checked2 = true;
                    CheckSquares(list[i], squares , type , search_value);
                }
            }
        }
    }

    private List<Square> GetAroundSquares(Square square)
    {
        List<Square> list = new List<Square>();
        if (square.hang > square_hang_count.Length/2)
        {
            // 左上角
            int hang = square.hang + 1;
            int lie = square.lie - 1;
            ListUpAdd(hang, lie, list);
            // 右上角
            hang = square.hang + 1;
            lie = square.lie;
            ListUpAdd(hang, lie, list);
            // 左边
            hang = square.hang;
            lie = square.lie - 1;
            ListUpAdd(hang, lie, list);
            // 右边
            hang = square.hang;
            lie = square.lie + 1;
            ListUpAdd(hang, lie, list);
            // 左下角
            hang = square.hang - 1;
            lie = square.lie;
            ListUpAdd(hang, lie, list);
            // 右下角
            hang = square.hang - 1;
            lie = square.lie + 1;
            ListUpAdd(hang, lie, list);
        }else if(square.hang == square_hang_count.Length / 2)
        {
            // 左上角
            int hang = square.hang + 1;
            int lie = square.lie - 1;
            ListUpAdd(hang, lie, list);
            // 右上角
            hang = square.hang + 1;
            lie = square.lie;
            ListUpAdd(hang, lie, list);
            // 左边
            hang = square.hang;
            lie = square.lie - 1;
            ListUpAdd(hang, lie, list);
            // 右边
            hang = square.hang;
            lie = square.lie + 1;
            ListUpAdd(hang, lie, list);
            // 左下角
            hang = square.hang - 1;
            lie = square.lie-1;
            ListUpAdd(hang, lie, list);
            // 右下角
            hang = square.hang - 1;
            lie = square.lie;
            ListUpAdd(hang, lie, list);
        }else
        {
            // 左上角
            int hang = square.hang + 1;
            int lie = square.lie;
            ListUpAdd(hang, lie, list);
            // 右上角
            hang = square.hang + 1;
            lie = square.lie+1;
            ListUpAdd(hang, lie, list);
            // 左边
            hang = square.hang;
            lie = square.lie - 1;
            ListUpAdd(hang, lie, list);
            // 右边
            hang = square.hang;
            lie = square.lie + 1;
            ListUpAdd(hang, lie, list);
            // 左下角
            hang = square.hang - 1;
            lie = square.lie-1;
            ListUpAdd(hang, lie, list);
            // 右下角
            hang = square.hang - 1;
            lie = square.lie;
            ListUpAdd(hang, lie, list);
        }
       
        return list;
    }

    private void ListUpAdd(int hang ,int lie , List<Square> list)
    {
        if (lie >= 0 && hang>=0&& hang<square_hang_count.Length&&lie< square_hang_count[hang])
        {
            int index = 0;
            for (int i = 0; i < square_hang_count.Length-1-hang; i++)
            {
                index += square_hang_count[i];
            }
            index += lie;
            list.Add(this.squares[index]);
            //Debug.Log("获得周边的"+hang+":"+ lie + "---"+this.squares[index].hang+ this.squares[index].lie);
        }
    }

    private bool CheckEliminate(ItemStruct generate_struct)
    {
        if(this.game_mode == GameMode.CLASSIC)
        {
            return CheckEliminateClissic(generate_struct);
        }else
        {
            return CheckEliminateDeformation(generate_struct);
        }
    }

    private bool CheckEliminateDeformation(ItemStruct generate_struct)
    {
        if (generate_struct.item.Length == 1)
        {
            for (int i = 0; i < this.squares.Length; i++)
            {
                if (this.squares[i].item == null && this.squares[i].square_block==null)
                {
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < this.squares.Length; i++)
            {
                if (this.squares[i].item == null && this.squares[i].square_block == null)
                {
                    List<Square> list = GetAroundSquares(this.squares[i]);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].item == null && list[j].square_block == null)
                        {
                            return true;
                        }
                    }

                }
            }
        }
        return false;
    }

    private bool CheckEliminateClissic(ItemStruct generate_struct)
    {
        if (generate_struct.item.Length == 1)
        {
            for (int i = 0; i < this.squares.Length; i++)
            {
                if (this.squares[i].item == null)
                {
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < this.squares.Length; i++)
            {
                if (this.squares[i].item == null)
                {
                    List<Square> list = GetAroundSquares(this.squares[i]);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].item == null)
                        {
                            return true;
                        }
                    }

                }
            }
        }
        return false;
    }

    public bool CheckMatchItemStructSquare()
    {
        matched_square = new Square[struct_manager.current_item_struct.item.Length];
        for (int j = 0; j < struct_manager.current_item_struct.item.Length; j++)
        {
            if (struct_manager.current_item_struct.item[j].square_collider == null || struct_manager.current_item_struct.item[j].square_collider.GetComponent<Square>().item!=null || struct_manager.current_item_struct.item[j].square_collider.GetComponent<Square>().square_block != null)
            {
                return false;
            }
        }

        for (int j = 0; j < struct_manager.current_item_struct.item.Length; j++)
        {
            if (struct_manager.current_item_struct.item[j].square_collider != null)
            {
                matched_square[j] = struct_manager.current_item_struct.item[j].square_collider.GetComponent<Square>();
                matched_square[j].MatchedItemStruct(struct_manager.current_item_struct.item[j]);
                //Debug.Log(matched_square[j].col+":"+matched_square[j].row);
            }
        }
        return true;
    }

    public Square[] GetMatchedSquares()
    {
        return matched_square;
    }

    public int GetCurrentMaxNumber()
    {
        int number = 1;
        for (int i = 0; i < this.squares.Length; i++)
        {
            if (this.squares[i].item!=null && (int)this.squares[i].item.item_type > number)
            {
                number = (int)this.squares[i].item.item_type;
            }
        }
        return number;
    }

   private void InitGame(GameMode mode)
    {
        this.game_mode = mode;
        switch (this.game_mode)
        {
            case GameMode.CLASSIC:
                square_hang_count = new int[5] { 3, 4, 5, 4, 3 };
                break;

            case GameMode.DEFORMATION:
                square_hang_count = new int[7] { 4, 5, 6, 7, 6 ,5,4 };
                break;
        }

    }

}

public enum GameState
{
    NULL,
    PLAYING,
    SUCCESS,
    FAIL,

}

public enum GameHandleState
{
    NULL,
    Eliminating,
    DRAGING_SQUARE,
    DRAG_SQUARE_FINISHED,
}

public enum GameMode
{
    CLASSIC,
    DEFORMATION,
}


