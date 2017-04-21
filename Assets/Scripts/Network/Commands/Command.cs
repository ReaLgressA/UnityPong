namespace Pong.Network {
    using System;

    public abstract class Command {
        public const int SessionIdLength = 16;

        protected abstract void Parse(byte[] data);

        public abstract CommandCode Code { get; }

        public static Command Create(byte[] data) {
            if(data.Length < 4) {
                return null;
            }
            CommandCode code = (CommandCode)BitConverter.ToInt32(data, 0);
            var type = Type.GetType(string.Format("Pong.Network.Command{0}", code.ToString()));
            if(type != null) {
                Command cmd = Activator.CreateInstance(type) as Command;
                cmd.Parse(data);
                return cmd;
            } else {
                return null;
            }
        }

        public abstract byte[] GetBytes(String sessionId);
    }

    public enum CommandCode : int {
        Undefined = 0,
        Connect = 1,
        ConnectEstablished = 2,
        PaddleInitialized = 3,
        PaddleMove = 4,
        BallSpawn = 5,
        BallLaunch = 6,
        BallUpdate = 7,
        ScoreUpdate = 8,
        GameEnd = 9
    }
}