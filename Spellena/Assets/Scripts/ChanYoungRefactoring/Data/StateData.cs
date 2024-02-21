using System.Collections.Generic;
using ListenerNodeData;

namespace StateData
{
    public enum State
    {
        None, Unique, Casting, Holding, Channeling
    }

    public enum AnimationType
    {
        None, Casting, Channeling
    }

    public class SkillRoute
    {
        public int routeIndex = 0;
        //자신의 클라이언트에서의 흐름
        public List<ListenNode> route = new List<ListenNode>();
        public bool isLocalReady = false;

        //다른 클라이언트에서의 흐름
        public List<ListenNode> networkRoute = new List<ListenNode>();
        public bool isReady = false;

        //진행 시간
        public float progressTime = 0f;
        //쿨타임적용은 모든 클라이언트에 적용됨 -> isReady는 마스터에서만 신호를 보냄
        public float coolDownTime = 0f;
    }

    public class AnimationRoute
    {
        public List<AnimationType> route = new List<AnimationType>();
        public List<float> routeTime = new List<float>();
        public int routeIndex = 0;
    }

}
