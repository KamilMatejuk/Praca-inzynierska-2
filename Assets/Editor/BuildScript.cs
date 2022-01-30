using UnityEditor;
using UnityEngine;


public class BuildScript {

    [SerializeField, HideInInspector] private static string gameName = "Gra";
    [SerializeField, HideInInspector] private static string[] levels = new string[] { "Assets/Scenes/Main Menu.unity", "Assets/Scenes/Create Level.unity", "Assets/Scenes/Game.unity" };

    [MenuItem("MyTools/Custom Windows Build")]
    public static void BuildGameWin() => BuildGame(BuildTarget.StandaloneWindows64, "exe", new string[] { "unity_builtin_extra", "unity default resources" });

    [MenuItem("MyTools/Custom Linux Build")]
    public static void BuildGameLinux() => BuildGame(BuildTarget.StandaloneLinux64, "x86_64", new string[] { "unity_builtin_extra", "unity default resources", "UnityPlayer.png" });

    private static void BuildGame(BuildTarget system, string extension, string[] files) {
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        if (path.Length > 0) {
            BuildPipeline.BuildPlayer(levels, $"{path}/{gameName}.{extension}", system, BuildOptions.None);
            FileUtil.CopyFileOrDirectory($"{path}/{gameName}_Data/Resources/", $"{path}/{gameName}_Data/Temp_Resources/");
            FileUtil.DeleteFileOrDirectory($"{path}/{gameName}_Data/Resources/");
            FileUtil.CopyFileOrDirectory("Assets/Resources/", $"{path}/{gameName}_Data/Resources/");
            foreach (string file in files) {
                FileUtil.CopyFileOrDirectory($"{path}/{gameName}_Data/Temp_Resources/{file}", $"{path}/{gameName}_Data/Resources/{file}");
            }
            FileUtil.DeleteFileOrDirectory($"{path}/{gameName}_Data/Temp_Resources/");
        }
    }
}