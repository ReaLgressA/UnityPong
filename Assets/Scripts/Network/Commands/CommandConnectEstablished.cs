namespace Pong.Network {
    using System;
    using System.Linq;
    using System.Text;

    public class CommandConnectEstablished : Command {
        protected string sessionId;
        public override CommandCode Code { get { return CommandCode.ConnectEstablished; } }
        public string SessionId { get { return sessionId; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[20];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(this.sessionId), 0, bytes, 4, Command.SessionIdLength);
            return bytes;
        }

        protected override void Parse(byte[] data) {
            sessionId = Encoding.ASCII.GetString(data.Skip(4).Take(16).ToArray());
        }

        public CommandConnectEstablished() { }
        public CommandConnectEstablished(string sessionId) {
            this.sessionId = sessionId;
        }
    }
}