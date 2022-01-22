using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public static class Logging {

    private static string basePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")) + "/Logs/";

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

    public static string GetParamString<T>(params T[] args) {
        string log = "";
        foreach (T arg in args) {
            log += arg.ToString() + " ";
        }
        return log;
    }

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
