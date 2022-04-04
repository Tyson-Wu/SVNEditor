using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Svn
{
    [System.Serializable]
    public class SvnFilterData
    {
        public int careerIndex = 0;
        public List<string> ignorePatternModify = new List<string>();
        public List<string> ignoreFilesModify = new List<string>();
        public List<string> ignoreFoldersModify = new List<string>();
        public List<string> ignorePatternAdd = new List<string>();
        public List<string> ignoreFilesAdd = new List<string>();
        public List<string> ignoreFoldersAdd = new List<string>();
        public List<string> ignorePatternDelete = new List<string>();
        public List<string> ignoreFilesDelete = new List<string>();
        public List<string> ignoreFoldersDelete = new List<string>();
        public SvnFilterData()
        {
            careerIndex = 0;
            ignorePatternModify = new List<string>();
            ignoreFilesModify = new List<string>();
            ignoreFoldersModify = new List<string>();
            ignorePatternAdd = new List<string>();
            ignoreFilesAdd = new List<string>();
            ignoreFoldersAdd = new List<string>();
            ignorePatternDelete = new List<string>();
            ignoreFilesDelete = new List<string>();
            ignoreFoldersDelete = new List<string>();
        }
        public static void Save(SvnFilterData data)
        {
            string jsonStr = JsonUtility.ToJson(data);
            EditorPrefs.SetString(SaveKey, jsonStr);
        }
        public static bool Load(out SvnFilterData data)
        {
            string jsonStr = EditorPrefs.GetString(SaveKey, null);
            bool hasData = !string.IsNullOrEmpty(jsonStr);
            if (hasData)
            {
                data = JsonUtility.FromJson<SvnFilterData>(jsonStr);
            }
            else
                data = new SvnFilterData();
            return hasData;
        }
        public static string SaveKey
        {
            get { return string.Format("{0}_svnFilterData", Application.dataPath); }
        }
    }
}