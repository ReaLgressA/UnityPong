namespace Pong.Network {
    using System;
    using System.Text;

    public class CommandScoreUpdate : Command {
        public override CommandCode Code { get { return CommandCode.ScoreUpdate; } }
        public Int32 leftScore;
        public Int32 rightScore;

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[28];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(leftScore), 0, bytes, 20, sizeof(float));
            Array.Copy(BitConverter.GetBytes(rightScore), 0, bytes, 24, sizeof(float));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            leftScore = BitConverter.ToInt32(data, 20);
            rightScore = BitConverter.ToInt32(data, 24);
        }

        public CommandScoreUpdate() { }
        public CommandScoreUpdate(Int32 leftScore, Int32 rightScore) {
            this.leftScore = leftScore;
            this.rightScore = rightScore;
        }
    }
}
