using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace FSOFacadeWorker
{
    public class GraphicsDeviceServiceMock : IGraphicsDeviceService
    {
        Form _hiddenForm;

        public GraphicsDeviceServiceMock()
        {
            _hiddenForm = new Form()
            {
                Visible = false,
                ShowInTaskbar = false
            };

            var Parameters = new PresentationParameters()
            {
                BackBufferWidth = 1280,
                BackBufferHeight = 720,
                DeviceWindowHandle = _hiddenForm.Handle,
                PresentationInterval = PresentInterval.Immediate,
                IsFullScreen = false
            };

            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, Parameters);
        }

        public GraphicsDevice GraphicsDevice { get; set; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        public void Release()
        {
            GraphicsDevice.Dispose();
            GraphicsDevice = null;

            _hiddenForm.Close();
            _hiddenForm.Dispose();
            _hiddenForm = null;
        }
    }
}
