using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningWarning : MonoBehaviour
{
    private float warningDuration = 0.8f;
    private float strikeDuration = 0.35f;
    private int damage = 25;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isStriking = false;
    private bool hasDealtDamage = false;
    private Sprite[] lightningAnimSprites;

    public static LightningWarning CreateLineWarning(Vector3 startPos, Vector3 endPos, float width = 1.8f, float warningTime = 0.8f, int damageAmount = 25)
    {
        GameObject obj = new GameObject("LightningLineWarning");
        obj.transform.position = (startPos + endPos) / 2f;
        
        Vector3 dir = (endPos - startPos);
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.Euler(0, 0, angle);

        LightningWarning lw = obj.AddComponent<LightningWarning>();
        lw.warningDuration = warningTime;
        lw.damage = damageAmount;

        lw.sr = obj.AddComponent<SpriteRenderer>();
        lw.sr.sortingOrder = 8;
        lw.sr.sprite = CreateSquareSprite();
        obj.transform.localScale = new Vector3(length, width, 1f);

        BoxCollider2D boxCol = obj.AddComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
        lw.col = boxCol;
        lw.col.enabled = false;

        lw.LoadLightningSprites();

        return lw;
    }

    public static LightningWarning CreateBoxWarning(Vector3 position, Vector2 size, float warningTime = 0.8f, int damageAmount = 25)
    {
        GameObject obj = new GameObject("LightningBoxWarning");
        obj.transform.position = position;

        LightningWarning lw = obj.AddComponent<LightningWarning>();
        lw.warningDuration = warningTime;
        lw.damage = damageAmount;

        lw.sr = obj.AddComponent<SpriteRenderer>();
        lw.sr.sortingOrder = 8;
        lw.sr.sprite = CreateSquareSprite();
        obj.transform.localScale = new Vector3(size.x, size.y, 1f);

        BoxCollider2D boxCol = obj.AddComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
        lw.col = boxCol;
        lw.col.enabled = false;

        lw.LoadLightningSprites();

        return lw;
    }

    private void LoadLightningSprites()
    {
#if UNITY_EDITOR
        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Lightning Animation.png");
        List<Sprite> list = new List<Sprite>();
        foreach (Object a in assets)
        {
            if (a is Sprite s) list.Add(s);
        }
        lightningAnimSprites = list.ToArray();
#endif
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(2, 2);
        Color[] colors = new Color[4];
        for (int i = 0; i < 4; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }

    private void Start()
    {
        StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        float timer = 0f;

        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.PingPong(timer * 10f, 0.5f) + 0.25f;
            if (sr != null) sr.color = new Color(1f, 0.15f, 0.15f, alpha);
            yield return null;
        }

        // Lightning Strike phase!
        isStriking = true;
        if (col != null) col.enabled = true;

        if (sr != null)
        {
            sr.color = Color.white;
            if (lightningAnimSprites != null && lightningAnimSprites.Length > 0)
            {
                float frameTime = strikeDuration / lightningAnimSprites.Length;
                foreach (Sprite spr in lightningAnimSprites)
                {
                    sr.sprite = spr;
                    yield return new WaitForSeconds(frameTime);
                }
            }
            else
            {
                sr.color = new Color(0.3f, 0.85f, 1f, 0.9f);
                yield return new WaitForSeconds(strikeDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(strikeDuration);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AttemptStrikeDamage(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        AttemptStrikeDamage(other.gameObject);
    }

    private void AttemptStrikeDamage(GameObject target)
    {
        if (!isStriking || hasDealtDamage) return;

        if (target.CompareTag("Player"))
        {
            Player player = target.GetComponent<Player>();
            if (player == null) player = target.GetComponentInParent<Player>();

            if (player != null)
            {
                hasDealtDamage = true;
                player.TakeDamage(damage, transform.position);
                Debug.Log($"⚡ Lightning strike dealt {damage} damage to Player!");
            }
        }
    }
}
