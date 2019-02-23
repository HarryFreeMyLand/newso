using System.Diagnostics;
using System.Reflection;

namespace FSO.Common
{
    public struct GameConsts
    {
        public static string GameName = "SimTactics";

        #region Error Messages
        public static string NotImplemented = "Not Implemented";
        public static string NotImplementedMsg = "This feature is not implemented yet!";
        public static string ObjectDeprecated = "Target Object is Deprecated!";
        public static string ValidInTS1 = "Only valid in TS1.";
        public static string Unused = "Unused";
        #endregion

        public static string FullVersion = $"{Assembly.GetExecutingAssembly().GetName().Version}";
        public static string TCBranch = "alpha";
        public static string TCVersion = $"{TCBranch}-{FullVersion}";
        public static string BranchFile = "branch.txt";
    }
}
