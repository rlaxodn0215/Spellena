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
        //�ڽ��� Ŭ���̾�Ʈ������ �帧
        public List<ListenNode> route = new List<ListenNode>();
        public bool isLocalReady = false;

        //�ٸ� Ŭ���̾�Ʈ������ �帧
        public List<ListenNode> networkRoute = new List<ListenNode>();
        public bool isReady = false;

        //���� �ð�
        public float progressTime = 0f;
        //��Ÿ�������� ��� Ŭ���̾�Ʈ�� ����� -> isReady�� �����Ϳ����� ��ȣ�� ����
        public float coolDownTime = 0f;
    }

    public class AnimationRoute
    {
        public List<AnimationType> route = new List<AnimationType>();
        public List<float> routeTime = new List<float>();
        public int routeIndex = 0;
    }

}
