using FSO.Client.UI.Hints;
using FSO.Client.UI.Panels;
using Ninject;

namespace FSO.Client
{
    public class FSOFacade
    {
        public static KernelBase Kernel;
        public static GameController Controller;
        public static UIMessageController MessageController = new UIMessageController();

        public static UIHintManager Hints;
    }
}
