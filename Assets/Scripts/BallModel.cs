namespace Pong {
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using Utility;

    public class BallModel : ScriptableObject {
        [System.Serializable]
        public struct BallSpeedOnHit {
            public int hitCounter;
            public float ballSpeed;
            public float paddleSpeedMod;
        }
        public BallSpeedOnHit[] model;

        public float GetSpeed(int hits) {
            float spd = 0f;
            for(int i = 0; i < model.Length; ++i) {
                if(model[i].hitCounter <= hits) {
                    spd = model[i].ballSpeed;
                } else {
                    break;
                }
            }
            return spd;
        }

        public float GetPaddleMod(int hits) {
            float mod = 0f;
            for(int i = 0; i < model.Length; ++i) {
                if(model[i].hitCounter <= hits) {
                    mod = model[i].paddleSpeedMod;
                } else {
                    break;
                }
            }
            return mod;
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ball Speed Model")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<BallModel>();
        }
#endif
    }
}