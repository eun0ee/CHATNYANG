using UnityEngine;

public class EnemyMovementTest : MonoBehaviour
{
    // �̵� �ӵ�
    public float speed = 2f;
    private Transform targetPlayer;

    private void Start()
    {
        // ������ �÷��̾� �±׸� ���� ������Ʈ ã��
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
        }
    }

    private void Update()
    {
        if (GetComponent<ConfusionStatus>() != null) return;

        // �÷��̾ �����ϸ� �� �������� �̵�
        if (targetPlayer != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, speed * Time.deltaTime);
        }
    }
}