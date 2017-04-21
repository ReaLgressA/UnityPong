namespace Pong.Network {
    using System;

    public class CommandConnect : Command {
        public override CommandCode Code { get { return CommandCode.Connect; } }

        public override byte[] GetBytes(string sessionId = null) {
            return BitConverter.GetBytes((Int32)Code);
        }

        protected override void Parse(byte[] data) { }

        public CommandConnect() { }
    }
}