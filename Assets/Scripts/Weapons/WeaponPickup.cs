using UnityEngine;

/// <summary>
/// Allows the player to pick up weapons from the ground.
/// When the player walks over a weapon and presses the swap key (Q),
/// the current weapon is dropped and the new one is equipped.
/// Shows UI info when the player is near an unequipped weapon.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    private BaseWeapon attachedWeapon;
    private Player player;
    private bool playerInRange = false;

    private void Start()
    {
        attachedWeapon = GetComponent<BaseWeapon>();
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        // Show pickup prompt when near
        if (playerInRange && !attachedWeapon.IsEquipped && IsPickupPressed())
        {
            PickupWeapon();
        }
    }

    private bool IsPickupPressed()
    {
        KeyCode key = SettingsManager.CurrentSettings != null ? SettingsManager.CurrentSettings.pickupWeapon : KeyCode.Q;
        if (key == KeyCode.Mouse0) return Input.GetMouseButtonDown(0);
        if (key == KeyCode.Mouse1) return Input.GetMouseButtonDown(1);
        if (key == KeyCode.Mouse2) return Input.GetMouseButtonDown(2);
        return Input.GetKeyDown(key);
    }

    private void PickupWeapon()
    {
        if (player == null) return;

        // Drop current weapon if player has one
        if (player.currentWeapon != null)
        {
            BaseWeapon oldWeapon = player.currentWeapon.GetComponent<BaseWeapon>();
            if (oldWeapon != null)
            {
                oldWeapon.Detach();
                // Place old weapon at player's position
                oldWeapon.transform.position = player.transform.position;
            }
        }

        // Equip new weapon
        player.currentWeapon = gameObject;
        attachedWeapon.AttachTo(player.gameObject);

        Debug.Log($"Picked up: {attachedWeapon.weaponName} (DMG: {attachedWeapon.damage})");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER) && !attachedWeapon.IsEquipped)
        {
            playerInRange = true;
            Debug.Log($"Press Q to pick up: {attachedWeapon.weaponName}");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER) && !attachedWeapon.IsEquipped)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER))
        {
            playerInRange = false;
        }
    }
}
