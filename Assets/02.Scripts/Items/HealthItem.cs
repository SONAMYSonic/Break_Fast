using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private int _healAmount = 20;
    [SerializeField] private float _lifeTime = 8f;

    private void Update()
    {
        if (_lifeTime <= 0f)
            return;

        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerCarController>();
        if (player == null)
            return;

        player.Heal(_healAmount);

        // 있으면 픽업 사운드도 재생
        SoundManager.Instance?.PlaySfx(SfxType.UiClick); // 나중에 HealPickup 같은 타입 추가해도 됨

        Destroy(gameObject);
    }
}
