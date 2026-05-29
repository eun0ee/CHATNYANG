using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class FeatherRodSlash : MonoBehaviour
{
    private float damage;
    private float knockbackPower = 2.5f;
    private Vector2 sourcePosition;

    public void Initialize(WeaponData data, Vector2 sourcePos)
    {
        damage = data.damage;
        sourcePosition = sourcePos;

        transform.localScale = Vector3.one * data.aoeRadius;

        Destroy(gameObject, 0.15f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                Vector2 knockbackDir = ((Vector2)other.transform.position - sourcePosition).normalized;

                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 코루틴을 실행하여 넉백과 브레이크(정지)를 순차적으로 처리
                    StartCoroutine(ApplyKnockback(rb, knockbackDir));
                }
            }
        }
    }

    private IEnumerator ApplyKnockback(Rigidbody2D rb, Vector2 direction)
    {
        // 1. 순간적으로 힘을 가해 적을 밀쳐냄
        rb.AddForce(direction * knockbackPower, ForceMode2D.Impulse);

        // 2. 0.1초 동안 밀려나도록 대기 (이 수치가 짧을수록 덜 밀림)
        yield return new WaitForSeconds(0.1f);

        // 3. 우주 미아가 되지 않도록 물리 속도를 0으로 강제 정지
        // (정지 직후 적의 이동 스크립트가 다시 플레이어를 향해 걸어오게 만듦)
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}