using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public static event System.Action OnGuardHasSpottedPlayer; // �̺�Ʈ�� �ൿ���, �׼��� �ൿ. �����Լ���� ��.

    public Transform pathHolder;
    Transform player;
    Color originalSpotlightColor;

    public float speed = 5.0f;
    public float waitTime = 0.3f;
    public float turnSpeed = 90; // 1�ʿ� 90�� ȸ��
    public float timeToSpotPlayer = 0.5f;

    public Light spotLight;
    public float viewDistance; // �� �þ߰Ÿ� 
    public LayerMask viewMask;
    float viewAngle;
    float playerVisibleTimer;

    void Awake()
    {
        // ����Ƽ���� Awake�� �����ڿ� �ش��ϴ� ���� ( ���� 1���� �ش��Լ� ȣ�� ) 
        speed = 5.0f;
        waitTime = 0.3f;
        viewDistance = 10.0f;
        viewAngle = spotLight.spotAngle;
        originalSpotlightColor = spotLight.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; ++i)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    // Update is called once per frame
    void Update()
    {
        if(CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
        }
        else
        {
            playerVisibleTimer -= Time.deltaTime;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotLight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if(playerVisibleTimer >= timeToSpotPlayer)
        {
            if(OnGuardHasSpottedPlayer != null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach(Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, 0.3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        // �̾����� ���� ������ �� �̾��ֱ� 
        Gizmos.DrawLine(previousPosition, startPosition);

        // �þ�ǥ��(����Ƽ �����ͻ�)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

    // �þ� �� ĳ���Ͱ� �ִ��� üũ. true : �߰� / false : �̹߰�
    bool CanSeePlayer()
    {
        if(Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if(angleBetweenGuardAndPlayer < viewAngle / 2.0f)
            {
                if(!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while(true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if(transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);

                StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }
    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        // ���⺤��
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;

        // Ÿ�ٰ��� ���� ���ϱ�
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        // �󸶳� ������ ������ �ִ��� �����ϱ� ���� DeltaAngle ���
        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    
}
