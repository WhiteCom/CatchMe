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
            // ����Ű �Է¿� ���� ���⺤�� ���ϱ�
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"),
                                         0,
                                         Input.GetAxisRaw("Vertical")).normalized;
        }
        float inputMagnitude = inputDirection.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude); // ĳ���Ͱ� ȸ�� �� �ٶ󺸴� ������ 0���� �Ǵ� ���� �ذ�
        transform.eulerAngles = Vector3.up * angle;

        velocity = transform.forward * moveSpeed * smoothInputMagnitude;
    }

    // Rigidbody ���� �浹ó���� FixedUpdate �� �̿��ϴ°� ȿ�����̴�. 
    private void FixedUpdate()
    {
        // MoveRotation : ���Ϸ� ������ ������ ������ �־ �� �������� ���ʹϾ� ü��� �����Ѵ�.
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
    }

    // OnTrigger�� ����Ϸ��� ���ڷ� ������ ������Ʈ�� IsTrigger�� true ���¿����Ѵ�.
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
