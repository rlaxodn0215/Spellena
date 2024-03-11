namespace SkillRelated
{
    public enum InputSide
    {
        Left, Right
    }


    public enum SkillType
    {
        None, Unique, Casting, Channeling, Error
    }

    public interface SkillLogicEvent
    {
        void Enter();
        void Quit();
        void PlayUpdate();
        void PlayFixedUpdate();
    }
}
