using UnityEngine;
using System.Collections;

public class Map{

    static public int map_length = 19;

    private float square_create_time = 0.02f;

    static public float square_edge =68f/ Mathf.Cos(Mathf.Deg2Rad * 30);

    static public float square_with = 68f;

    static public float interval_x = 0;

    static public float interval_y;

    public delegate void SquareFinish(Square[] square);

    public SquareFinish SquareFinishEvent;

    private void InitGameMap(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.CLASSIC:
                map_length = 19;
                square_edge = 68f / Mathf.Cos(Mathf.Deg2Rad * 30);
                square_with = 68f;
                break;

            case GameMode.DEFORMATION:
                map_length = 37;
                square_edge = 45f / Mathf.Cos(Mathf.Deg2Rad * 30);
                square_with = 45f;
                break;
        }
    }

    public void CrerateSquares(GameObject square_prefab, GameMode mode, SquareFinish callback)
    {
        InitGameMap(mode);
        Square[] squares = new Square[map_length];
        LevelManager.instance.StartCoroutine(DoInitializeMap(square_prefab , squares ,callback));
    }


    private IEnumerator DoInitializeMap(GameObject square_prefab, Square[] squares, SquareFinish callback = null)
    {
        int temp = LevelManager.instance.square_hang_count[0];
        Vector3 start_position = Vector3.zero;
        Vector3 start_position_copy = start_position;
        int index = 0;
        for (int i = LevelManager.instance.square_hang_count.Length-1; i >= LevelManager.instance.square_hang_count.Length/2; i--)
        {
            start_position_copy = start_position;
            for (int j = 0; j < temp; j++)
            {
                GameObject square_obj = GameObject.Instantiate(square_prefab);
                index = GetIndex(i, j);
                squares[index] = square_obj.GetComponent<Square>();
                squares[index].Init(j, i, start_position);
                start_position = new Vector3(start_position.x + interval_x + square_with*2, start_position.y, start_position.z);
                yield return new WaitForSeconds(square_create_time);
            }
            start_position = new Vector3(start_position_copy.x- interval_x / 2- square_with, start_position_copy.y - interval_y - square_edge / 2 - square_edge, start_position_copy.z);
            temp++;
        }

        start_position = new Vector3(start_position_copy.x + interval_x / 2 + square_with, start_position_copy.y - interval_y - square_edge / 2 - square_edge, start_position_copy.z);
        temp = LevelManager.instance.square_hang_count[LevelManager.instance.square_hang_count.Length/2+1];
        for (int i = LevelManager.instance.square_hang_count.Length - LevelManager.instance.square_hang_count.Length/2-2; i >= 0; i--)
        {
            start_position_copy = start_position;
            for (int j = 0; j < temp; j++)
            {
                GameObject square_obj = GameObject.Instantiate(square_prefab);
                index = GetIndex(i, j);
                squares[index] = square_obj.GetComponent<Square>();
                squares[index].Init(j, i, start_position);
                start_position = new Vector3(start_position.x + interval_x + square_with*2, start_position.y, start_position.z);
                yield return new WaitForSeconds(square_create_time);
            }
            start_position = new Vector3(start_position_copy.x + interval_x / 2 + square_with, start_position_copy.y -interval_y - square_edge /2- square_edge, start_position_copy.z);
            temp--;
        }
        if (callback != null)
        {
            callback(squares);
        }
    }

    public int GetIndex(int hang ,int lie)
    {
        int index = 0;
        for (int i = 0; i < LevelManager.instance.square_hang_count.Length-1 - hang; i++)
        {
            index += LevelManager.instance.square_hang_count[i];
        }
        index += lie;
        return index;
    }
}
