using UnityEngine;

public class CameraChange : MonoBehaviour
{
    public Vector3 opponentViewPosition = new Vector3(14.5f, 21.9f, 0.3f);
    public Vector3 opponentViewRotation = new Vector3(7f, -90.24f, 0f);

    public Vector3 tableViewPosition = new Vector3(-7.73f, 29.1f, 0.35f);
    public Vector3 tableViewRotation = new Vector3(90f, -90f, 0f);

    Vector3 targetPosition;
    Quaternion targetRotation;

    public float moveSpeed = 5f;

    bool lookingAtTable = false;

    private Vector3 basePosition;
    public float breathAmplitude = 0.001f;
    public float breathSpeed = 1f;

    void Start()
    {
        targetPosition = opponentViewPosition;
        targetRotation = Quaternion.Euler(opponentViewRotation);
        basePosition = targetPosition;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);

        if (!lookingAtTable)
        {
            float yOffset = Mathf.Sin(Time.time * breathSpeed) * breathAmplitude;
            transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        }
    }

    public void SwitchToPlayerView()
    {
        lookingAtTable = false;
        targetPosition = opponentViewPosition;
        targetRotation = Quaternion.Euler(opponentViewRotation);
        basePosition = targetPosition;
    }

    public void SwitchToBoardView()
    {
        lookingAtTable = true;
        targetPosition = tableViewPosition;
        targetRotation = Quaternion.Euler(tableViewRotation);
        basePosition = targetPosition;
    }

    public void CameraButton() {
        if (lookingAtTable) { 
            SwitchToPlayerView();
        }else SwitchToBoardView();
    }
}
