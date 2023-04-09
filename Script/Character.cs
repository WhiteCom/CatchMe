using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public event System.Action OnReachedEndOfLevel;

    public float moveSpeed = 7.0f;
    public float smoothMoveTime = 0.1f;
    public float turnSpeed = 8.0f;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rigidbody;
    bool disabled = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Guard.OnGuardHasSpottedPlayer += Disable;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = Vector3.zero;
        if(!disabled)
        {
            // 방향키 입력에 따른 방향벡터 구하기
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"),
                                         0,
                                         Input.GetAxisRaw("Vertical")).normalized;
        }
        float inputMagnitude = inputDirection.magnitude; // 방향벡터 정규화 
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        // 입력 방향에 맞게 회전할 각도 구하기
        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude); // 캐릭터가 회전 후 바라보는 방향이 0도가 되는 문제 해결
        transform.eulerAngles = Vector3.up * angle;

        velocity = transform.forward * moveSpeed * smoothInputMagnitude;
    }

    // Rigidbody 관련 충돌처리는 FixedUpdate 를 이용하는게 효율적이다. 
    private void FixedUpdate()
    {
        // MoveRotation : 오일러 각도는 짐벌락 문제가 있어서 잘 쓰기위해 쿼터니언 체계로 변경한다.
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
    }

    // OnTrigger를 사용하려면 인자로 들어오는 오브젝트가 IsTrigger가 true 상태여야한다.
    void OnTriggerEnter(Collider hitCollider)
    {
        if (hitCollider.tag == "Finish")
        {
            Disable();
            if(OnReachedEndOfLevel != null)
            {
                OnReachedEndOfLevel();
            }
        }
    }
    void Disable()
    {
        disabled = true;
    }

    void OnDestroy()
    {
        Guard.OnGuardHasSpottedPlayer -= Disable;
    }
}
