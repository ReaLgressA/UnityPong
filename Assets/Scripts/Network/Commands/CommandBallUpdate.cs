namespace Pong.Network {
    using UnityEngine;

    public class CommandBallUpdate : CommandBallLaunch {
        public override CommandCode Code { get { return CommandCode.BallUpdate; } }

        public CommandBallUpdate() { }
        public CommandBallUpdate(Vector2 dir, Vector2 pos) : base(dir, pos) { }
    }
}