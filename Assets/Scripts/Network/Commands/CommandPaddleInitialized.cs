namespace Pong.Network {
    using System;
    using System.Text;

    public class CommandPaddleInitialized : Command {
        protected Int32 paddleId;
        protected PaddleColors paddleColor;
        protected bool isLeftSide;
        protected bool isControllable;

        public override CommandCode Code { get { return CommandCode.PaddleInitialized; } }
        public Int32 Id { get { return paddleId; } }
        public PaddleColors Color { get { return paddleColor; } }
        public bool IsLeftSide { get { return isLeftSide; } }
        public bool IsControllable { get { return isControllable; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[30];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            Array.Copy(BitConverter.GetBytes((Int32)paddleColor), 0, bytes, 24, sizeof(Int32));
            bytes[28] = BitConverter.GetBytes(isLeftSide)[0];
            bytes[29] = BitConverter.GetBytes(isControllable)[0];
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
            paddleColor = (PaddleColors)BitConverter.ToInt32(data, 24);
            isLeftSide = BitConverter.ToBoolean(data, 28);
            isControllable = BitConverter.ToBoolean(data, 29);
        }

        public CommandPaddleInitialized() { }
        public CommandPaddleInitialized(Int32 paddleId, PaddleColors paddleColor, bool isLeftSide, bool isControllable) {
            this.paddleId = paddleId;
            this.paddleColor = paddleColor;
            this.isLeftSide = isLeftSide;
            this.isControllable = isControllable;
        }
    }
}