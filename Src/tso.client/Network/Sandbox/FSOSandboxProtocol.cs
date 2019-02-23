using Mina.Filter.Codec;
using Mina.Core.Session;

namespace FSO.Client.Network.Sandbox
{
    public class FSOSandboxProtocol : IProtocolCodecFactory
    {
        IProtocolDecoder _decoder;
        IProtocolEncoder _encoder;

        public IProtocolDecoder GetDecoder(IoSession session)
        {
            if (_decoder == null)
            {
                _decoder = new FSOSandboxProtocolDecoder();
            }
            return _decoder;
        }

        public IProtocolEncoder GetEncoder(IoSession session)
        {
            if (_encoder == null)
            {
                _encoder = new FSOSandboxProtocolEncoder();
            }
            return _encoder;
        }
    }
}
