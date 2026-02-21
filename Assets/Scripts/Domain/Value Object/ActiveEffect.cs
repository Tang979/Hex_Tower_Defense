using Domain.Enums;

namespace Domain.ValueObject
{
    public class ActiveEffect
    {
        public string SourceId { get; private set; }
        public EnemyEffect EffectType { get; private set; }
        public float Power { get; private set; }
        public float RemainingTime { get; private set; }

        public ActiveEffect(string sourceId, EnemyEffect effect, float power, float duration)
        {
            this.SourceId = sourceId;
            this.EffectType = effect;
            this.Power = power;
            RemainingTime = duration;
        }

        public void Update(float deltaTime)
        {
            RemainingTime -= deltaTime;
        }

        public void SetRemainingTime(float time)
        {
            RemainingTime = time;
        }
    }
}