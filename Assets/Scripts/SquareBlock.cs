using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SquareBlock : MonoBehaviour {
    public SquareBlockType square_block_type;
    public Text number;

    public void SetBlockType(int type)
    {
        square_block_type = (SquareBlockType)type;
        number.text = type.ToString();
    }

}

public enum SquareBlockType
{
    ONEBLOCK,
}
