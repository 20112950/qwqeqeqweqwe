using System;
using System.Collections.Generic;

public class TextDatabase
{
	public delegate void BasicDelegate();

	private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

	public static TextDatabase textDataBaseInstance;

	public static TextDatabase GetInstance()
	{
		if (TextDatabase.textDataBaseInstance == null)
		{
			TextDatabase.textDataBaseInstance = new TextDatabase();
		}
		return TextDatabase.textDataBaseInstance;
	}

	public void Init()
	{
		this.data = CSVReader.Read("localization_sheet");
	}

	//public string GetLocalizedText(string ItemKey)
	//{
	//	string key = LocalizationController.CurrentLanguage.ToString().ToLower();
	//	string result = string.Empty;
	//	for (int i = 0; i < this.data.Count; i++)
	//	{
	//		if ((string)this.data[i]["Key"] == ItemKey)
	//		{
	//			if (this.data[i].ContainsKey(key))
	//			{
	//				result = (string)this.data[i][key];
	//			}
	//			break;
	//		}
	//	}
	//	return result;
	//}

	public string GetLocalizedText(string ItemKey, string LanguageKey)
	{
		string result = string.Empty;
		for (int i = 0; i < this.data.Count; i++)
		{
			if ((string)this.data[i]["Key"] == ItemKey)
			{
				if (this.data[i].ContainsKey(LanguageKey))
				{
					result = (string)this.data[i][LanguageKey];
				}
				break;
			}
		}
		return result;
	}
}
