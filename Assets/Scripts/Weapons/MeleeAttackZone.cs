using UnityEngine;

/// <summary>
/// Routes trigger events from the child AttackZone collider back to the parent MeleeWeapon script.
/// </summary>
public class MeleeAttackZone : MonoBehaviour
{
    private MeleeWeapon meleeWeapon;

    private void Start()
    {
        meleeWeapon = GetComponentInParent<MeleeWeapon>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.OnAttackTriggerEnter2D(collision);
        }
    }
}
