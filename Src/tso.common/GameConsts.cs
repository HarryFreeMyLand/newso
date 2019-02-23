using System.Reflection;

namespace FSO.Common
{
    public struct GameConsts
    {
        public const string GameName = "SimTactics Classic";

        #region Error Messages
        public const string NotImplemented = "Not Implemented";
        public const string NotImplementedMsg = "This feature is not implemented yet!";
        public const string ObjectDeprecated = "Target Object is Deprecated!";
        public const string InvalidRoomData = "Invalid room data!";
        public const string NHoodDataOutOfBands = "Neighbor data out of bounds.";
        public const string ValidInTS1 = "Only valid in TS1.";
        public const string Unused = "Unused";
        #endregion

        public static readonly string FullVersion = $"{Assembly.GetExecutingAssembly().GetName().Version}";
        public static readonly string TCBranch = "master";
        public static readonly string TCVersion = $"{TCBranch}-{FullVersion}";
        public static readonly string BranchFile = "branch.txt";
    }
}
