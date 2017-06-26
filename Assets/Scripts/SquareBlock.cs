using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SquareBlock : MonoBehaviour {
    public Square square;
    public SquareBlockType square_block_type;
    public Text number;
    public GameObject hammer;

    public void SetBlockType(int type)
    {
        square_block_type = (SquareBlockType)type;
        number.text = type.ToString();
    }

    public void ShowHammer(bool state)
    {
        this.hammer.SetActive(state);
    }

}

public enum SquareBlockType
{
    ONEBLOCK,
}
