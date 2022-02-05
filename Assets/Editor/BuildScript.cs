using UnityEditor;
using UnityEngine;

namespace RacingGameBot.Editors {
    public class BuildScript {

        [SerializeField, HideInInspector] private static string gameName = "Gra";
        [SerializeField, HideInInspector] private static string[] levelsGame = new string[] { "Assets/Scenes/Main Menu.unity", "Assets/Scenes/Create Level.unity", "Assets/Scenes/Game.unity" };
        [SerializeField, HideInInspector] private static string[] levelsTraining = new string[] { "Assets/Scenes/Multi Training.unity" };
        [SerializeField, HideInInspector] private static string[] filesLinux = new string[] { "unity_builtin_extra", "unity default resources", "UnityPlayer.png" };
        [SerializeField, HideInInspector] private static string[] filesWindows = new string[] { "unity_builtin_extra", "unity default resources" };

        /// <summary>
        /// Build game executable for Windows
        /// </summary>
        [MenuItem("MyTools/Custom Windows Build")]
        public static void BuildGameWin() => BuildGame(BuildTarget.StandaloneWindows64, "exe", filesWindows, levelsGame);

        /// <summary>
        /// Build game executable for Linux
        /// </summary>
        [MenuItem("MyTools/Custom Linux Build")]
        public static void BuildGameLinux() => BuildGame(BuildTarget.StandaloneLinux64, "x86_64", filesLinux, levelsGame);

        /// <summary>
        /// Build training executable
        /// </summary>
        [MenuItem("MyTools/Custom Linux Training Build")]
        public static void BuildTraining() => BuildGame(BuildTarget.StandaloneLinux64, "x86_64", filesLinux, levelsTraining);

        /// <summary>
        /// Builder for game executables based on system
        /// </summary>
        /// <param name="system">target system</param>
        /// <param name="extension">game file extension</param>
        /// <param name="files">list of </param>
        private static void BuildGame(BuildTarget system, string extension, string[] files, string[] levels) {
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
}