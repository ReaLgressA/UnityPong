namespace Pong.Network {
    using System;
    using System.Text;
    using UnityEngine;

    public class CommandBallLaunch : Command {
        public override CommandCode Code { get { return CommandCode.BallLaunch; } }
        protected float dirX;
        protected float dirY;
        protected float posX;
        protected float posY;

        public Vector2 Dir { get { return new Vector2(dirX, dirY); } }
        public Vector2 Pos { get { return new Vector2(posX, posY); } }

        public override byte[] GetBytes(string sessionId) {
            byte[] bytes = new byte[36];
            Array.Copy(BitConverter.GetBytes((Int32)Code), 0, bytes, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(sessionId), 0, bytes, 4, Command.SessionIdLength);
            Array.Copy(BitConverter.GetBytes(dirX), 0, bytes, 20, sizeof(float));
            Array.Copy(BitConverter.GetBytes(dirY), 0, bytes, 24, sizeof(float));
            Array.Copy(BitConverter.GetBytes(posX), 0, bytes, 28, sizeof(float));
            Array.Copy(BitConverter.GetBytes(posY), 0, bytes, 32, sizeof(float));
            return bytes;
        }

        protected override void Parse(byte[] data) {
            dirX = BitConverter.ToSingle(data, 20);
            dirY = BitConverter.ToSingle(data, 24);
            posX = BitConverter.ToSingle(data, 28);
            posY = BitConverter.ToSingle(data, 32);
        }

        public CommandBallLaunch() { }
        public CommandBallLaunch(Vector2 dir, Vector2 pos) {
            this.dirX = dir.x;
            this.dirY = dir.y;
            this.posX = pos.x;
            this.posY = pos.y;
        }
    }
}