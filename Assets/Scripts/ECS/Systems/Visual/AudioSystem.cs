using System.Collections.Generic;

public class AudioSystem : SystemBase
{
    public AudioSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取所有请求播放音频的单帧事件
        var audioEvents = GetEntitiesWith<AudioPlayEventComponent>();

        for (int i = audioEvents.Count - 1; i >= 0; i--)
        {
            var entity = audioEvents[i];
            var audioData = entity.GetComponent<AudioPlayEventComponent>();

            if (AudioManager.Instance != null)
            {
                if (audioData.IsPositional)
                {
                    AudioManager.Instance.PlaySFXAtPosition(audioData.ClipName, audioData.Position);
                }
                else
                {
                    AudioManager.Instance.PlaySFX(audioData.ClipName);
                }
            }

            // 消费完毕，打上销毁标签（将在帧末被 EntityCleanupSystem 清理）
            if (!entity.HasComponent<PendingDestroyComponent>())
            {
                entity.AddComponent(new PendingDestroyComponent());
            }
        }
    }
}