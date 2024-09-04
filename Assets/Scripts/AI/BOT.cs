using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BOT : MonoBehaviour
{
    public Transform[] patrolPoints; // Массив точек патрулирования
    public float moveSpeed = 3f;
    public float jumpForce = 8f; // Сила прыжка вверх
    public float forwardJumpForce = 3f; // Сила прыжка вперед
    public Transform player; // Ссылка на игрока
    public Vector3 overlapBoxSize = new Vector3(1f, 1f, 2f); // Размер зоны для OverlapBox
    public LayerMask playerLayer; // Слой, на котором находится игрок
    public float jumpCooldown = 2f; // Задержка между прыжками

    private NavMeshAgent agent;
    private Rigidbody rb;
    private int currentPatrolIndex;
    private bool isPatrolling;
    private bool hasJumped; // Флаг для отслеживания выполнения прыжка

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        agent.speed = moveSpeed;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        isPatrolling = true;
        hasJumped = false;
    }

    private void Update()
    {
        if (IsPlayerInJumpZone() && !hasJumped)
        {
            HandlePlayerEncounter();
        }
        else if (isPatrolling)
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        Debug.Log("Патруль начат!");
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void StartPatrolling()
    {
         Debug.Log("Продолжаю патруль!");
        if (patrolPoints.Length > 0)
        {
            isPatrolling = true;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private bool IsPlayerInJumpZone()
    {
        Vector3 boxCenter = transform.position + transform.forward * (overlapBoxSize.z / 2);
        Quaternion boxRotation = transform.rotation;
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, overlapBoxSize / 2, boxRotation, playerLayer);

        foreach (Collider collider in hitColliders)
        {
            if (collider.transform == player)
            {
                Debug.Log("Ой, Игрок!");
                return true;
            }
        }

        return false;
    }

    private void HandlePlayerEncounter()
    {
        Debug.Log("Бот прыгает!");

        hasJumped = true; // Устанавливаем флаг, что прыжок выполнен
        isPatrolling = false;
        agent.enabled = false; // Отключаем NavMeshAgent, чтобы не мешал прыжку

        if (rb != null)
        {
            rb.isKinematic = false; // Отключаем кинематику для применения силы
            rb.velocity = Vector3.zero;

            // Добавляем силу прыжка вперед и вверх
            Vector3 jumpDirection = Vector3.up * jumpForce + transform.forward * forwardJumpForce;
            // Vector3 jumpDirection = Vector3.up * jumpForce;
            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("Rigidbody не найден!");
        }

        // Возвращаемся к патрулированию после прыжка
        StartCoroutine(ResetAgentAfterJump(1.6f)); // Задержка перед включением агента обратно
    }

    private IEnumerator ResetAgentAfterJump(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (agent != null)
        {
            agent.enabled = true;
            isPatrolling = true; // Возвращаемся к патрулированию
            StartPatrolling(); // Перезапускаем патрулирование
        }

        hasJumped = false; // Сбрасываем флаг, чтобы позволить прыжок в будущем
    }

    private void OnDrawGizmos()
    {
        if (patrolPoints.Length > 1)
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform startPoint = patrolPoints[i];
                Transform endPoint = patrolPoints[(i + 1) % patrolPoints.Length];

                if (startPoint != null && endPoint != null)
                {
                    Gizmos.DrawLine(startPoint.position, endPoint.position);
                }
            }
        }

        // Рисуем коробку для визуализации зоны действия OverlapBox
        Gizmos.color = Color.red;
        Vector3 boxCenter = transform.position + transform.forward * (overlapBoxSize.z / 2);
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, overlapBoxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
