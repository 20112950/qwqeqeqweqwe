using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Entity = UnityEngine.GameObject;

/// <summary>
/// Icon类型，对应相应资源加载
/// </summary>
public enum emIconType
{
    Common,                 //对应目录Art/UI/Icon/CommonIcon下的Icon资源
    BuffIcon,               //对应目录Art/UI/Icon/BuffIcon下的Icon资源
    HeadIcon,               //对应目录Art/UI/Icon/HeadIcon下的Icon资源
    SkillIcon,              //对应目录Art/UI/Icon/SkillIcon下的Icon资源
    EmojiIcon,              //对应目录Art/UI/Icon/Emoji下的Icon资源
    EquipIcon,              //对应目录Art/UI/Icon/EquipIcon下的Icon资源
    PropsIcon,              //对应目录Art/UI/Icon/PropsIcon下的Icon资源
    Max,
}

public class DUICommon
{
    public static Dictionary<emIconType, string> _Icon_Bundle = new Dictionary<emIconType, string>()
    {
#if UI_RESOURCE
        { emIconType.Common, "UI/Icon/CommonIcon/"},
        { emIconType.BuffIcon, "UI/Icon/BuffIcon/"},
        { emIconType.HeadIcon, "UI/Icon/HeadIcon/"},
        { emIconType.SkillIcon, "UI/Icon/SkillIcon/"},
        { emIconType.EmojiIcon, "UI/Icon/Emoji/"},
        { emIconType.EquipIcon, "UI/Icon/Equipicon/"},
        {emIconType.PropsIcon, "UI/Icon/PropsIcon/"},
#else
        { emIconType.Common, "commonicon"},
        { emIconType.BuffIcon, "bufficon"},
        { emIconType.HeadIcon, "headicon"},
        { emIconType.SkillIcon, "skillicon"},
        { emIconType.EmojiIcon, "emoji"},
        { emIconType.EquipIcon, "equipicon"},
        { emIconType.PropsIcon, "propsicon"},

#endif
    };


    public static void Sprite2Material(Sprite sprite, Material mat)
    {
        if (sprite == null || mat == null)
        {
            return;
        }
        mat.mainTexture = sprite.texture;
        Rect tr = sprite.textureRect;
        float inverse_width = 1.0f / sprite.texture.width;
        float inverse_height = 1.0f / sprite.texture.height;

        Vector2 vTilling = new Vector2
        {
            x = tr.width * inverse_width,
            y = tr.height * inverse_height,
        };

        Vector2 vOffset = new Vector2
        {
            x = tr.xMin * inverse_width,
            y = tr.yMin * inverse_height,
        };

        mat.SetTextureScale("_MainTex", vTilling);
        mat.SetTextureOffset("_MainTex", vOffset);
    }

    public static void ReassignShader(Entity entity)
    {
        var renderer_entities = entity.GetComponentsInChildren<Renderer>(true);
        for (int entity_index = 0; entity_index < renderer_entities.Length; entity_index++)
        {
            var materials = renderer_entities[entity_index].sharedMaterials;
            foreach (var material in materials.Where(i => i != null && i.shader != null && !i.shader.isSupported))
            {
                material.shader = Shader.Find(material.shader.name);
            }
        }
    }

}


public class IconSprite
{
    public emIconType IconType;
    public string IconName;
}

public delegate void LoadUICallBack(GameObject prefab);
public delegate void LoadUISceneCallBack(emUIWindow wnd);

