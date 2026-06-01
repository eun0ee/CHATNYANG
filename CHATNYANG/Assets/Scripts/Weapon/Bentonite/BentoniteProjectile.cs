using UnityEngine;

public class BentoniteProjectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private WeaponData data;
    private bool isInitialized = false;

    public void Initialize(Vector2 target, WeaponData weaponData)
    {
        targetPosition = target;
        data = weaponData;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // ��ǥ ������ ���� ��� �̵� (������ ������ ���� Z���̳� Ʈ�������� �߰� ����)
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, data.projectileSpeed * Time.deltaTime);

        // ��ǥ ���� ���� Ȯ��
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log($"[Explode] 위치: {transform.position}, 반경: {data.aoeRadius}");
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.aoeRadius);
        Debug.Log($"[Explode] 감지된 Collider 수: {hitEnemies.Length}");
        
        foreach (Collider2D hit in hitEnemies)
        {
            Debug.Log($"[Explode] 감지됨: {hit.name} / tag: {hit.tag}");
            
            if (hit.CompareTag("Enemy"))
            {
                EnemyStats enemy = hit.GetComponent<EnemyStats>();
                Debug.Log($"[Explode] EnemyStats: {(enemy != null ? "있음" : "없음")}");
                
                if (enemy != null)
                    enemy.TakeDamage(data.damage);
            }
        }

        if (data.areaPrefab != null)
        {
            GameObject puddle = Instantiate(data.areaPrefab, transform.position, Quaternion.identity);
            puddle.GetComponent<BentoniteArea>()?.Initialize(data);
        }

        Destroy(gameObject);
    }
}