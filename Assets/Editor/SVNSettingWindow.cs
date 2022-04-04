using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
namespace Svn
{
    public class SVNSettingWindow : EditorWindow
    {
        const string _MenuPath = "Svn/";
        const string _tip = "";
        string[] _career = { "Artist", "Tester", "Programmer", "Designer" };
        GUIContent[] _tabContents = new GUIContent[]{
            new GUIContent("Artist"),
            new GUIContent("Tester"),
            new GUIContent("Programmer"),
            new GUIContent("Designer"),
        };
        [MenuItem(_MenuPath + "Update", priority = 0, validate = false)]
        public static void UpdateSVN()
        {
            SvnClientHelper.OpenUpdateWindow(SvnCmdHelper.GetSvnWorkDir());
        }
        [MenuItem(_MenuPath + "Revert", priority = 0, validate = false)]
        public static void RevertSVN()
        {
            SvnClientHelper.OpenRevertWindow(SvnCmdHelper.GetSvnWorkDir());
        }
        [MenuItem(_MenuPath + "CommitAll", priority = 0, validate = false)]
        public static void CommitAllSVN()
        {
            SvnClientHelper.OpenCommitWindow(SvnCmdHelper.GetSvnWorkDir());
        }
        [MenuItem(_MenuPath + "CommitAllWithFilter", priority = 0, validate = true)]
        public static bool CommitAllWithFilterValidate()
        {
            if (string.IsNullOrEmpty(SvnCmdHelper.GetSvnVertsion())) return false;
            if (string.IsNullOrEmpty(SvnCmdHelper.GetSvnWorkDir())) return false;
            return true;
        }
        [MenuItem(_MenuPath + "CommitAllWithFilter", priority = 0, validate = false)]
        public static void CommitAllWithFilter()
        {
            if (SvnFilterData.Load(out var data))
            {
                SvnCmdHelper.SetChangelistForFilterData(data);
                SvnClientHelper.OpenCommitWindow(SvnCmdHelper.GetSvnWorkDir());
            }
            else
                Open();
        }
        [MenuItem(_MenuPath + "Settings", false, 1000)]
        public static void Open()
        {
            SVNSettingWindow window = GetWindow<SVNSettingWindow>("Setting");
            window._svnVersion = SvnCmdHelper.GetSvnVertsion();
            window._workDir = SvnCmdHelper.GetSvnWorkDir();
            if (!string.IsNullOrEmpty(window._workDir))
                window._workDir = window._workDir.Replace("\\", "/");
            window.minSize = new Vector2(520, 520);
            window.InitConfigData();
            window.Show();
        }

        [SerializeField] private string _svnVersion = null;
        [SerializeField] private string _workDir = null;

        [SerializeField] private int careerSelectedIdx = -1;
        [SerializeField] private string ignorePatternModify = "";
        [SerializeField] private List<string> ignoreFilesModify = new List<string>();
        [SerializeField] private List<string> ignoreFoldersModify = new List<string>();
        [SerializeField] private string ignorePatternAdd = "";
        [SerializeField] private List<string> ignoreFilesAdd = new List<string>();
        [SerializeField] private List<string> ignoreFoldersAdd = new List<string>();
        [SerializeField] private string ignorePatternDelete = "";
        [SerializeField] private List<string> ignoreFilesDelete = new List<string>();
        [SerializeField] private List<string> ignoreFoldersDelete = new List<string>();

        [SerializeField] private Vector2 scrollPos = Vector2.zero;
        private GUIContent content = new GUIContent();
        private StringBuilder stringBuilder = new StringBuilder();
        private GUIContent GetGUIContent(string text, Texture image, string tooltip)
        {
            content.text = text;
            content.image = image;
            content.tooltip = tooltip;
            return content;
        }
        private void InitConfigData()
        {
            if (SvnFilterData.Load(out var data))
            {
                careerSelectedIdx = data.careerIndex;
                AddWorkDir(_workDir, data.ignoreFilesModify, ref ignoreFilesModify);
                AddWorkDir(_workDir, data.ignoreFoldersModify, ref ignoreFoldersModify);
                PatternCombine(data.ignorePatternModify, ref ignorePatternModify);
                AddWorkDir(_workDir, data.ignoreFilesAdd, ref ignoreFilesAdd);
                AddWorkDir(_workDir, data.ignoreFoldersAdd, ref ignoreFoldersAdd);
                PatternCombine(data.ignorePatternAdd, ref ignorePatternAdd);
                AddWorkDir(_workDir, data.ignoreFilesDelete, ref ignoreFilesDelete);
                AddWorkDir(_workDir, data.ignoreFoldersDelete, ref ignoreFoldersDelete);
                PatternCombine(data.ignorePatternDelete, ref ignorePatternDelete);
            }
        }
        void PatternSplit(string pattern, ref List<string> patternList)
        {
            patternList.Clear();
            var splits = pattern.Split(' ');

            foreach (var v in splits)
            {
                if (string.IsNullOrEmpty(v))
                    patternList.Add(v);
            }
        }
        void PatternCombine(List<string> patternList, ref string pattern)
        {
            StringBuilder stringBuilder = new StringBuilder();
            const string space = " ";
            foreach (var v in patternList)
            {
                stringBuilder.Append(v);
                stringBuilder.Append(space);
            }
            pattern = stringBuilder.ToString();
        }
        void RemoveWorkDir(string workDir, List<string> org, ref List<string> dst)
        {
            dst.Clear();
            workDir = string.Format("{0}/", workDir);
            workDir = workDir.Replace("\\", "/");
            for (int i = 0; i < org.Count; ++i)
            {
                dst.Add(org[i].Replace("\\", "/").Replace(workDir, ""));
            }
        }
        void AddWorkDir(string workDir, List<string> org, ref List<string> dst)
        {
            dst.Clear();
            for (int i = 0; i < org.Count; ++i)
            {
                if (org[i].StartsWith(workDir))
                    dst.Add(org[i]);
                else
                    dst.Add(string.Format("{0}/{1}", workDir, org[i]));
            }
        }
        private void OnGUI()
        {
            bool show = false;
            EditorGUILayout.LabelField("Base Info");
            if (_svnVersion == null)
            {
                EditorGUILayout.HelpBox("never find svn!", MessageType.Error, true);
            }
            else if (_workDir == null)
            {
                EditorGUI.indentLevel += 1;
                GUI.enabled = false;
                EditorGUILayout.TextField("svn version:", _svnVersion);
                GUI.enabled = true;
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.HelpBox("current project not in svn control", MessageType.Warning, true);
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.indentLevel += 1;
                EditorGUILayout.TextField("svn version:", _svnVersion);
                EditorGUILayout.TextField("workDir:", _workDir);
                EditorGUI.indentLevel -= 1;
                GUILayout.Space(10);
                GUI.enabled = true;
                show = true;
            }
            GUI.enabled = show;
            DrawBody();
            GUI.enabled = true;
        }
        void DrawBody()
        {
            EditorGUILayout.HelpBox(_tip, MessageType.Info, true);
            EditorGUI.BeginChangeCheck();
            careerSelectedIdx = GUILayout.Toolbar(careerSelectedIdx, _tabContents);
            if (EditorGUI.EndChangeCheck())
            {
                PresetConfig(careerSelectedIdx);
                EditorGUIUtility.editingTextField = false;
            }

            GUI.enabled = careerSelectedIdx != -1;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ignore Setting");
            if (GUILayout.Button(GetGUIContent("save", null, "save"), GUILayout.Width(50)))
            {
                SaveConfig(careerSelectedIdx);
            }
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
            Panel("Modify Ignore", ref ignorePatternModify, ref ignoreFilesModify, ref ignoreFoldersModify);
            Panel("Add Ignore", ref ignorePatternAdd, ref ignoreFilesAdd, ref ignoreFoldersAdd);
            Panel("Delete Ignore", ref ignorePatternDelete, ref ignoreFilesDelete, ref ignoreFoldersDelete);
            GUILayout.EndScrollView();
            GUILayout.Space(10);
        }
        private void Panel(string name, ref string ignPat, ref List<string> ignFiles, ref List<string> ignFolders)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(name);
            EditorGUI.indentLevel += 1;
            ignPat = EditorGUILayout.TextField(GetGUIContent("Ignore Pattern:", null, ""), ignPat);

            EditorGUILayout.LabelField("Ignore Files");
            EditorGUI.indentLevel += 1;
            int removeIdx = -1;
            for (int i = 0; i < ignFiles.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextField(GetGUIContent("Ignore Files:", null, ""), ignFiles[i]);
                GUI.enabled = true;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    removeIdx = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx != -1)
            {
                ignFiles.RemoveAt(removeIdx);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(24));
            if (GUILayout.Button(GetGUIContent("Add File", null, "")))
            {
                string file = EditorUtility.OpenFilePanel("Select File", _workDir, null);
                if (file.Contains(_workDir) && !ignFiles.Contains(file))
                    ignFiles.Add(file);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.LabelField("Ignore Folders");
            EditorGUI.indentLevel += 1;
            removeIdx = -1;
            for (int i = 0; i < ignFolders.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.TextField(GetGUIContent("Ignore Folders:", null, ""), ignFolders[i]);
                GUI.enabled = true;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    removeIdx = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIdx != -1)
            {
                ignFolders.RemoveAt(removeIdx);
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(24));
            if (GUILayout.Button(GetGUIContent("Add Folder", null, "")))
            {
                string file = EditorUtility.OpenFolderPanel("Select Folder", _workDir, null);
                if (file.Contains(_workDir) && !ignFolders.Contains(file))
                    ignFolders.Add(file);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 1;
        }

        private void PresetConfig(int careerIndex)
        {
            string career = _career[careerIndex];
            stringBuilder.Clear();
            switch (career)
            {
                case "Artist":
                    break;
                case "Tester":
                case "Programmer":
                case "Designer":
                    {
                        stringBuilder.Append(".mat .anim .png .jpg");
                        break;
                    }
            }
            ignorePatternModify = stringBuilder.ToString();
            ignoreFilesModify.Clear();
            ignoreFoldersModify.Clear();
            ignorePatternAdd = stringBuilder.ToString();
            ignoreFilesAdd.Clear();
            ignoreFoldersAdd.Clear();
            ignorePatternDelete = stringBuilder.ToString();
            ignoreFilesDelete.Clear();
            ignoreFoldersDelete.Clear();
        }

        private void SaveConfig(int careerIndex)
        {
            SvnFilterData data = new SvnFilterData();
            data.careerIndex = careerIndex;
            RemoveWorkDir(_workDir, ignoreFilesModify, ref data.ignoreFilesModify);
            RemoveWorkDir(_workDir, ignoreFoldersModify, ref data.ignoreFoldersModify);
            PatternSplit(ignorePatternModify, ref data.ignorePatternModify);
            RemoveWorkDir(_workDir, ignoreFilesAdd, ref data.ignoreFilesAdd);
            RemoveWorkDir(_workDir, ignoreFoldersAdd, ref data.ignoreFoldersAdd);
            PatternSplit(ignorePatternAdd, ref data.ignorePatternAdd);
            RemoveWorkDir(_workDir, ignoreFilesDelete, ref data.ignoreFilesDelete);
            RemoveWorkDir(_workDir, ignoreFoldersDelete, ref data.ignoreFoldersDelete);
            PatternSplit(ignorePatternDelete, ref data.ignorePatternDelete);
        }
    }
}