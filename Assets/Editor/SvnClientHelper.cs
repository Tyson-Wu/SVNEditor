using System.Diagnostics;
namespace Svn
{
    static class SvnClientHelper
    {
        public static void OpenCommitWindow(string workDirectory)
        {
            ExecuteTortoiseClient("/command:commit ", workDirectory);
        }
        public static void OpenUpdateWindow(string workDirectory)
        {
            ExecuteTortoiseClient("/command:update ", workDirectory);
        }
        public static void OpenRevertWindow(string workDirectory)
        {
            ExecuteTortoiseClient("/command:revert ", workDirectory);
        }
        private static void ExecuteTortoiseClient(string cmd, string workDirectory)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate (object state)

            {
                Process p = null;
                try
                {
                    ProcessStartInfo start = new ProcessStartInfo("TortoiseProc.exe");
                    start.Arguments = cmd + "/path:\"" + workDirectory + "\"";
                    p = Process.Start(start);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    if (p != null) p.Close();
                }
            });
        }
    }
}