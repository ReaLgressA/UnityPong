namespace Pong.Network {
    using System;
    using System.Text;

    public class CommandGameEnd : Command {
        public override CommandCode Code { get { return CommandCode.GameEnd; } }
        protected bool isWinnerLeft;
        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[21];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(isWinnerLeft), 0, bytes, 20, sizeof(bool));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            isWinnerLeft = BitConverter.ToBoolean(data, 20);
        }

        public CommandGameEnd() { }
        public CommandGameEnd(bool isWinnerLeft) {
            this.isWinnerLeft = isWinnerLeft;
        }
    }
}