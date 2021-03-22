using System;
using System.Text;

public class Utils
{
    public static string GenerateUId()
    {
        byte[] buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0).ToString();
    }

    private static object m_builderLock = new object();
    private static StringBuilder m_builder = new StringBuilder();

    public static string combine(params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, int intParam)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            m_builder.Append(intParam.ToString());
            return m_builder.ToString(0, m_builder.Length);
        }
    }
}//end class