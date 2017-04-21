namespace Pong.Network {
    using System;
    using System.Text;
    public class CommandBallSpawn : Command {
        public override CommandCode Code { get { return CommandCode.BallSpawn; } }
        protected Int32 paddleId;

        public Int32 PaddleId { get { return paddleId; } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[24];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(paddleId), 0, bytes, 20, sizeof(Int32));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            paddleId = BitConverter.ToInt32(data, 20);
        }

        public CommandBallSpawn() { }
        public CommandBallSpawn(Int32 paddleId) {
            this.paddleId = paddleId;
        }
    }
}