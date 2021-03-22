using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using System.Text;

public static class MobileHierarchyUtils
{
    private static Dictionary<Type, MemberInfo[]> typeToVariables = new Dictionary<Type, MemberInfo[]>(89);
    public static HashSet<Type> serializableUnityTypes = new HashSet<Type>() { typeof( Vector2 ), typeof( Vector3 ), typeof( Vector4), typeof( Rect ), typeof( Quaternion ),
                typeof( Matrix4x4 ), typeof( Color ), typeof( Color32 ), typeof( LayerMask ), typeof( Bounds ),
                typeof( AnimationCurve ), typeof( Gradient ), typeof( RectOffset ), typeof( GUIStyle )};

    public static string ToTitleCase(this string str)
    {
        if (str == null || str.Length == 0)
            return string.Empty;

        StringBuilder titleCaser = new StringBuilder(str.Length + 5);
        byte lastCharType = 1; // 0 -> lowercase, 1 -> _ (underscore), 2 -> number, 3 -> uppercase
        int i = 0;
        char ch = str[0];
        if ((ch == 'm' || ch == 'M') && str.Length > 1 && str[1] == '_')
            i = 2;

        for (; i < str.Length; i++)
        {
            ch = str[i];
            if (char.IsUpper(ch))
            {
                if ((lastCharType < 2 || (str.Length > i + 1 && char.IsLower(str[i + 1]))) && titleCaser.Length > 0)
                    titleCaser.Append(' ');

                titleCaser.Append(ch);
                lastCharType = 3;
            }
            else if (ch == '_')
            {
                lastCharType = 1;
            }
            else if (char.IsNumber(ch))
            {
                if (lastCharType != 2 && titleCaser.Length > 0)
                    titleCaser.Append(' ');

                titleCaser.Append(ch);
                lastCharType = 2;
            }
            else
            {
                if (lastCharType == 1 || lastCharType == 2)
                {
                    if (titleCaser.Length > 0)
                        titleCaser.Append(' ');

                    titleCaser.Append(char.ToUpper(ch));
                }
                else
                    titleCaser.Append(ch);

                lastCharType = 0;
            }
        }

        if (titleCaser.Length == 0)
            return str;

        return titleCaser.ToString();
    }

    public static MemberInfo[] GetAllVariables(this Type type)
    {
        MemberInfo[] result;
        if (!typeToVariables.TryGetValue(type, out result))
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance;
            FieldInfo[] fields = type.GetFields(flag);
            PropertyInfo[] properties = type.GetProperties(flag);

            int validFieldCount = 0;
            int validPropertyCount = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!field.IsLiteral && !field.IsInitOnly && (field.FieldType.IsSerializable()))
                    validFieldCount++;
            }

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.GetIndexParameters().Length == 0 && property.CanRead && property.CanWrite && property.PropertyType.IsSerializable())
                    validPropertyCount++;
            }

            int validVariableCount = validFieldCount + validPropertyCount;
            if (validVariableCount == 0)
                result = null;
            else
            {
                result = new MemberInfo[validVariableCount];

                int j = 0;
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (!field.IsLiteral && !field.IsInitOnly && field.FieldType.IsSerializable())
                        result[j++] = field;
                }

                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo property = properties[i];
                    if (property.GetIndexParameters().Length == 0 && property.CanRead && property.CanWrite && property.PropertyType.IsSerializable())
                        result[j++] = property;
                }
            }

            typeToVariables[type] = result;
        }

        return result;
    }

    public static bool IsSerializable(this Type type)
    {
#if UNITY_EDITOR || !NETFX_CORE
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
#else
            if( type.GetTypeInfo().IsPrimitive || type == typeof( string ) || type.GetTypeInfo().IsEnum )
#endif
            return true;

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return true;

        if (serializableUnityTypes.Contains(type))
            return true;

        if (type.IsArray)
        {
            if (type.GetArrayRank() != 1)
                return false;

            return type.GetElementType().IsSerializable();
        }
#if UNITY_EDITOR || !NETFX_CORE
        else if (type.IsGenericType)
#else
            else if( type.GetTypeInfo().IsGenericType )
#endif
        {
            if (type.GetGenericTypeDefinition() != typeof(List<>))
                return false;

            return type.GetGenericArguments()[0].IsSerializable();
        }

#if UNITY_EDITOR || !NETFX_CORE
        if (Attribute.IsDefined(type, typeof(SerializableAttribute), false))
#else
            if( type.GetTypeInfo().IsDefined( typeof( SerializableAttribute ), false ) )
#endif
            return true;

        return false;
    }//end func
}//end class

public class StringData
{
    public StreamReader sr;

    public string Pop()
    {
        return sr.ReadLine();
    }

    public string ReadString()
    {
        return Pop().Split(':')[1];
    }
    public bool ReadBoolean()
    {
        return Boolean.Parse(ReadString());
    }
    public int ReadInt()
    {
        return int.Parse(ReadString());
    }
    public void Init(StreamReader scr)
    {
        sr = scr;
    }
}


public class PropertyRenderData
{
    public const string INDENT_SPACE = "    ";

    public string name;
    public string value;

    public void CopyFromMemberInfo(MemberInfo memInfo, UnityEngine.Object targetComponent)
    {
        value = "null";
        object propertyValue = null;
        if (memInfo is PropertyInfo)
        {
            PropertyInfo proInfo = memInfo as PropertyInfo;
            name = proInfo.Name.ToTitleCase();
            propertyValue = proInfo.GetValue(targetComponent, null);
        }
        else if (memInfo is FieldInfo)
        {
            FieldInfo fInfo = memInfo as FieldInfo;
            name = fInfo.Name.ToTitleCase();
            propertyValue = fInfo.GetValue(targetComponent);
        }

        if (propertyValue != null)
        {
            if (propertyValue.GetType().IsValueType)
            {
                value = propertyValue.ToString();
            }
            else
            {
                if (propertyValue is List<int>)
                {
                    value = this.GetListIntFormat(propertyValue as List<int>);
                }
                else if (propertyValue is List<string>)
                {
                    value = this.GetListStringFormat(propertyValue as List<string>);
                }
                else
                {
                    value = propertyValue + "(" + propertyValue.ToString() + ")";
                }
            }//end else
        }
    }//end func

    private string GetListIntFormat(List<int> datalist)
    {
        StringBuilder allCodeBuilder = new StringBuilder();
        int count = datalist.Count;
        for (int i = 0; i < count; ++i)
        {
            allCodeBuilder.Append(Utils.combine("Element", i, ":", datalist[i].ToString(), "\n"));
        }
        return allCodeBuilder.ToString();
    }

    private string GetListStringFormat(List<string> datalist)
    {
        StringBuilder allCodeBuilder = new StringBuilder();
        int count = datalist.Count;
        for (int i = 0; i < count; ++i)
        {
            allCodeBuilder.Append(Utils.combine("Element", i, ":", datalist[i].ToString(), "\n"));
        }
        return allCodeBuilder.ToString();
    }


}//end class



public class PropertyItem : MonoBehaviour
{
    public RectTransform _rect;
    public Text _NameTxt;
    public RectTransform _rectValueBg;
    public RectTransform _rectValue;
    public Text _ValueTxt;


    public void Render(PropertyRenderData data)
    {
        _NameTxt.text = data.name;
        _ValueTxt.text = data.value;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectValue);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectValueBg);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
    }

}//end class