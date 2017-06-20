using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CSVReader
{
	private static string SPLIT_RE = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

	private static string LINE_SPLIT_RE = "\\r\\n|\\n\\r|\\n|\\r";

	private static char[] TRIM_CHARS = new char[]
	{
		'"'
	};

	public static List<Dictionary<string, object>> Read(string file)
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		TextAsset textAsset = Resources.Load(file) as TextAsset;
		string[] array = Regex.Split(textAsset.text, CSVReader.LINE_SPLIT_RE);
		if (array.Length <= 1)
		{
			return list;
		}
		string[] array2 = Regex.Split(array[0], CSVReader.SPLIT_RE);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array3 = Regex.Split(array[i], CSVReader.SPLIT_RE);
			if (array3.Length != 0 && !(array3[0] == string.Empty))
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				int num = 0;
				while (num < array2.Length && num < array3.Length)
				{
					string text = array3[num];
					text = text.TrimStart(CSVReader.TRIM_CHARS).TrimEnd(CSVReader.TRIM_CHARS).Replace("\\", string.Empty);
					object value = text;
					int num2;
					float num3;
					if (int.TryParse(text, out num2))
					{
						value = num2;
					}
					else if (float.TryParse(text, out num3))
					{
						value = num3;
					}
					dictionary[array2[num]] = value;
					num++;
				}
				list.Add(dictionary);
			}
		}
		return list;
	}
}
