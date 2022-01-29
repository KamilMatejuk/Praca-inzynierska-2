using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public static class Logging {

    private static string basePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")) + "/Logs/";

    /// <summary>
    /// Convert list to string
    /// </summary>
    /// <param name="l">list of objects of type T</param>
    /// <returns>list in string format</returns>
    public static string GetListString<T>(List<T> l) {
        if (l == null) {
            return "null";
        }
        StringBuilder sb = new StringBuilder();
        sb.Append("(" + l.Count + ") elements -> [");
        for(int i = 0; i < l.Count; i++) {
            T item = l[i];
            try {
                sb.Append(item.ToString());
            } catch {
                sb.Append("item doesn't have .ToString() method");
            }
            if (i != l.Count - 1) {
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    /// Convert multiple params to one string
    /// </summary>
    /// <param name="args">objects of type T</param>
    /// <returns>objects in one string</returns>
    public static string GetParamString<T>(params T[] args) {
        string log = "";
        foreach (T arg in args) {
            log += arg.ToString() + " ";
        }
        return log;
    }

    /// <summary>
    /// Append string to file. Create file if not exists.
    /// </summary>
    /// <param name="filename">name of log file</param>
    /// <param name="text">data to log</param>
    public static void LogToFile(string filename, string text) {
        string file = basePath + filename;
        StreamWriter sw;
        if (!File.Exists(file)) {
            sw = File.CreateText(file);
        } else {
            sw = File.AppendText(file);
        }
        sw.WriteLine(text);
        sw.Flush();
        sw.Close();
    }
}
