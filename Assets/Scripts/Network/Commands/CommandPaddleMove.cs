namespace Pong.Network {
    using System;
    using System.Text;

    public class CommandPaddleMove : Command {
        protected Int32 paddleId;
        protected PaddleMoveDir dir;
        public override CommandCode Code { get { return CommandCode.PaddleMove; } }

        public Int32 Id { get { return paddleId; } }
        public PaddleMoveDir Dir { get { return dir; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[28];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            Array.Copy(BitConverter.GetBytes((Int32)dir), 0, bytes, 24, sizeof(Int32));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
            dir = (PaddleMoveDir)BitConverter.ToInt32(data, 24);
        }

        public CommandPaddleMove() { }
        public CommandPaddleMove(Int32 paddleId, PaddleMoveDir dir) {
            this.paddleId = paddleId;
            this.dir = dir;
        }
    }
}