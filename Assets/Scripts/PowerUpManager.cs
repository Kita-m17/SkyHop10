using UnityEngine;
using static PowerUpManager;

public class PowerUpManager : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float duration = 5f;
    public enum PowerUpType
    {
        SpeedBoost,
        Invincibility,
        ExtraJump,
        SlowMotion,
        None
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivatePowerUp(powerUpType, duration);
                gameObject.SetActive(false); // Hide or pool it
            }
        }
    }
}
