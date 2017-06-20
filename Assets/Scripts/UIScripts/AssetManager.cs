//#define SHOW_DETAIL_LOG

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UScene = UnityEngine.SceneManagement.Scene;


public static class AssetManager
{
	#region Sprite

	/// <summary>
	/// 释放一个Sprite资源
	/// </summary>
	/// <param name="IconType"></param>
	/// <param name="szIconName"></param>
	public static void UnLoadSprite( emIconType IconType, string szIconName )
	{
		return;
		if( szIconName == string.Empty )
		{
			return;
		}
		if( !DUICommon._Icon_Bundle.ContainsKey( IconType ) )
		{
			Develop.LogError( "IconType Error!" );
			return;
		}

		string szBundle = DUICommon._Icon_Bundle[IconType];

		Paran.AssetManager.Bundle ab = Paran.AssetManager.GetAssetBundle( szBundle );
		if( ab == null )
		{
			return;
		}

		Paran.AssetManager.Asset<Texture2D> tex = ab.LoadAsset<Texture2D>( szIconName );
		if( tex == null )
		{
			return;
		}

		Resources.UnloadAsset( tex.Object );
	}

	#endregion
	#region UI Prefab

	/// <summary>
	/// 异步加载UIprefab，编辑器模式下直接加载prefab，发布版本加载assetbundle
	/// </summary>
	/// <param name="strUIPrefab">prefab名字</param>
	/// <param name="onload">加载完成的回调</param>
	/// <returns></returns>
	public static IEnumerator AysncLoadUI( string strUIPrefab, LoadUICallBack onload )
	{
#if UNITY_EDITOR
		if( !Paran.AssetManager.UseBundleInEditor )
		{
			GameObject pfb = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/Art/Prefabs/UI/" + strUIPrefab + ".prefab" );
			GameObject objTarget = pfb == null ? null : GameObject.Instantiate( pfb );
			if( objTarget != null )
			{
				objTarget.name = strUIPrefab;
			}

			if( onload != null )
			{
				onload( objTarget );
			}
			yield break;
		}
#endif
		string strAbPrefab = "us_" + strUIPrefab;

#if !UI_RESOURCE
        yield return Paran.AssetManager.Enumerate_AsyncLoad( strAbPrefab );
#endif
        
		yield return AsyncLoadUIPrefab( strUIPrefab, onload );

        Paran.AssetManager.UnloadBundle( strAbPrefab, false );
		//Task.CreateTask(AsyncLoadUIPrefab(strUIPrefab, onload));
	}

	/// <summary>
	/// 异步加载一个UIPrefab，确保调用此接口前已加载UIScene的Assetbundle
	/// </summary>
	/// <param name="szUIPrefab"></param>
	/// <param name="onload"></param>
	/// <returns></returns>
	public static IEnumerator AsyncLoadUIPrefab( string szUIPrefab, LoadUICallBack onload )
	{
#if UNITY_EDITOR
		if( !Paran.AssetManager.UseBundleInEditor )
		{
            GameObject pfb = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/Art/Prefabs/UI/" + szUIPrefab + ".prefab" );
			GameObject objTarget = pfb == null ? null : GameObject.Instantiate( pfb );
			if( objTarget != null )
			{
				objTarget.name = szUIPrefab;
			}
			if( onload != null )
			{
				onload( objTarget );
			}
            yield break;
		}
#endif
        GPerformance pfm = GPerformance.Start("AysncLoadUI:" + szUIPrefab);
        StringBuilder szScene = new StringBuilder();
		szScene.Append( "us_" );
		szScene.Append( szUIPrefab );

		AsyncOperation async = USceneManager.LoadSceneAsync( szScene.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Additive );
        ResourcesEx.AsyncLoadCount++;
        yield return async;
        pfm.LogPoint(1);
        ResourcesEx.AsyncLoadCount--;

		UScene srcScene = USceneManager.GetActiveScene();
		if( srcScene == null )
		{
			Develop.LogError( "SrcScene is NULL" );
		}

		UScene curScene = USceneManager.GetSceneByName( szScene.ToString() );
		if( curScene == null )
		{
			Develop.LogError( "UIScene is NULL or InValid" );
		}

		GameObject[] so = curScene.GetRootGameObjects();
		if( so != null && so.Length > 0 )
		{

			GameObject res = so[0];
			USceneManager.MoveGameObjectToScene( res, srcScene );
			USceneManager.UnloadScene( curScene.name );

			GameObject uires = GameObject.Find( szScene.ToString() );
			if( uires != null && uires.transform.childCount > 0 )
			{
				GameObject tempPrefab = uires.transform.GetChild( 0 ).gameObject;
				tempPrefab.transform.SetParent( null );
				Paran.AssetManager.ReassignShader( tempPrefab );

				if( onload != null )
				{
					onload( tempPrefab );
				}

				if( res != null )
				{
					GameObject.Destroy( res );
				}
			}
			else
			{
				Develop.LogError( "uires.transform.childCount == 0" );
			}
		}
        pfm.Stop();
    }

	#endregion
}