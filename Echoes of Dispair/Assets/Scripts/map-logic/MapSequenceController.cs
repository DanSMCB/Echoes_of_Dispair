using System.Collections;
using UnityEngine;

public class MapSequenceController : MonoBehaviour
{
    [Header("Rolls")]
    public Transform leftRoll;
    public Transform rightRoll;

    [Header("Paper")]
    public Transform mapPaper;
    public float paperClosedScaleX = 0.02f;
    public float paperOpenScaleX = 1f;

    [Header("Roll Movement (Z Axis)")]
    public float rollStartOffsetFromCenter = 0.05f;
    public float rollEndOffset = 4f;

    [Header("Roll Radius")]
    public float rollStartRadius = 1f;
    public float rollEndRadius = 0.35f;

    [Header("Spin")]
    public float leftRollSpinSpeed = 500f;
    public float rightRollSpinSpeed = -500f;
    public Vector3 spinAxis = Vector3.right;

    [Header("Vertical Drop")]
    [Tooltip("How much the rolls should move downward as they shrink.")]
    public float rollDropAmount = 0.325f;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform cameraStartPoint;
    public Transform cameraEndPoint;
    public float cameraMoveStartNormalized = 0.55f;

    [Header("Camera Breathing")]
    public float breathAmplitude = 0.05f;
    public float breathSpeed = 1f;

    private Vector3 cameraBasePosition;

    [Header("Timing")]
    public float initialDelay = 0.2f;
    public float unrollDuration = 2f;
    public float nodeRevealDelay = 0.12f;

    [Header("Decoration Groups")]
    public GameObject[] groupLeft;
    public GameObject[] groupMid;
    public GameObject[] groupRight;

    [Header("Nodes")]
    public GameObject[] nodes;

    private Vector3 paperBaseScale;

    private bool leftShown;
    private bool midShown;
    private bool rightShown;

    private float leftRollLength;
    private float rightRollLength;

    private float leftBaseX;
    private float rightBaseX;

    private float leftBaseY;
    private float rightBaseY;

    private float leftBaseZ;
    private float rightBaseZ;

    private float leftStartZ;
    private float rightStartZ;
    private float leftEndZ;
    private float rightEndZ;

    private void Start()
    {
        paperBaseScale = mapPaper.localScale;
        mapPaper.localScale = new Vector3(
            paperClosedScaleX,
            paperBaseScale.y,
            paperBaseScale.z
        );

        leftBaseX = leftRoll.localPosition.x;
        rightBaseX = rightRoll.localPosition.x;

        leftBaseY = leftRoll.localPosition.y;
        rightBaseY = rightRoll.localPosition.y;

        leftBaseZ = leftRoll.localPosition.z;
        rightBaseZ = rightRoll.localPosition.z;

        leftStartZ = mapPaper.localPosition.z - rollStartOffsetFromCenter;
        rightStartZ = mapPaper.localPosition.z + rollStartOffsetFromCenter;

        leftEndZ = mapPaper.localPosition.z - rollEndOffset;
        rightEndZ = mapPaper.localPosition.z + rollEndOffset;

        leftRoll.localPosition = new Vector3(leftBaseX, leftBaseY, leftStartZ);
        rightRoll.localPosition = new Vector3(rightBaseX, rightBaseY, rightStartZ);

        leftRollLength = leftRoll.localScale.y;
        rightRollLength = rightRoll.localScale.y;

        leftRoll.localScale = new Vector3(
            rollStartRadius,
            leftRollLength,
            rollStartRadius
        );

        rightRoll.localScale = new Vector3(
            rollStartRadius,
            rightRollLength,
            rollStartRadius
        );

        SetGroupActive(groupLeft, false);
        SetGroupActive(groupMid, false);
        SetGroupActive(groupRight, false);
        SetNodesActive(false);

        if (mainCamera != null && cameraStartPoint != null)
        {
            mainCamera.transform.position = cameraStartPoint.position;
            mainCamera.transform.rotation = cameraStartPoint.rotation;
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(initialDelay);


        Vector3 camStartPos = cameraStartPoint.position;
        Quaternion camStartRot = cameraStartPoint.rotation;

        cameraBasePosition = camStartPos;

        Vector3 camEndPos = cameraEndPoint.position;
        Quaternion camEndRot = cameraEndPoint.rotation;

        float time = 0f;

        while (time < unrollDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / unrollDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            float currentLeftZ = Mathf.Lerp(leftStartZ, leftEndZ, eased);
            float currentRightZ = Mathf.Lerp(rightStartZ, rightEndZ, eased);

            float currentRadius = Mathf.Lerp(rollStartRadius, rollEndRadius, eased);

            float radiusLost = rollStartRadius - currentRadius;
            float radiusTotalLoss = rollStartRadius - rollEndRadius;
            float dropT = radiusTotalLoss > 0.0001f ? radiusLost / radiusTotalLoss : 1f;
            float currentDrop = Mathf.Lerp(0f, rollDropAmount, dropT);

            leftRoll.localPosition = new Vector3(
                leftBaseX,
                leftBaseY - currentDrop,
                currentLeftZ
            );

            rightRoll.localPosition = new Vector3(
                rightBaseX,
                rightBaseY - currentDrop,
                currentRightZ
            );

            leftRoll.Rotate(spinAxis, leftRollSpinSpeed * Time.deltaTime, Space.Self);
            rightRoll.Rotate(spinAxis, rightRollSpinSpeed * Time.deltaTime, Space.Self);

            leftRoll.localScale = new Vector3(
                currentRadius,
                leftRollLength,
                currentRadius
            );

            rightRoll.localScale = new Vector3(
                currentRadius,
                rightRollLength,
                currentRadius
            );

            float currentPaperScaleX = Mathf.Lerp(
                paperClosedScaleX,
                paperOpenScaleX,
                eased
            );

            mapPaper.localScale = new Vector3(
                currentPaperScaleX,
                paperBaseScale.y,
                paperBaseScale.z
            );

            CheckDecorationReveal(currentRightZ);

            if (t >= cameraMoveStartNormalized)
            {
                float camT = Mathf.InverseLerp(cameraMoveStartNormalized, 1f, t);
                camT = Mathf.SmoothStep(0f, 1f, camT);

                Vector3 camPos = Vector3.Lerp(camStartPos, camEndPos, camT);

                float yOffset = Mathf.Sin(Time.time * breathSpeed) * breathAmplitude;

                mainCamera.transform.position = new Vector3(
                    camPos.x,
                    camPos.y + yOffset,
                    camPos.z
                );

                mainCamera.transform.rotation = Quaternion.Slerp(camStartRot, camEndRot, camT);
            }

            yield return null;
        }

        mapPaper.localScale = new Vector3(
            paperOpenScaleX,
            paperBaseScale.y,
            paperBaseScale.z
        );

        leftRoll.localScale = new Vector3(
            rollEndRadius,
            leftRollLength,
            rollEndRadius
        );

        rightRoll.localScale = new Vector3(
            rollEndRadius,
            rightRollLength,
            rollEndRadius
        );

        leftRoll.localPosition = new Vector3(
            leftBaseX,
            leftBaseY - rollDropAmount,
            leftEndZ
        );

        rightRoll.localPosition = new Vector3(
            rightBaseX,
            rightBaseY - rollDropAmount,
            rightEndZ
        );

        RevealGroup(groupLeft);
        RevealGroup(groupMid);
        RevealGroup(groupRight);

        yield return StartCoroutine(RevealNodesRoutine());

        leftRoll.gameObject.SetActive(false);
        rightRoll.gameObject.SetActive(false);

        float yAnimOffset = Mathf.Sin(Time.time * breathSpeed) * breathAmplitude;

        mainCamera.transform.position = new Vector3(
            camEndPos.x,
            camEndPos.y + yAnimOffset,
            camEndPos.z
        );

        mainCamera.transform.rotation = camEndRot;
    }

    private void CheckDecorationReveal(float currentRightZ)
    {
        if (!leftShown && currentRightZ > mapPaper.localPosition.z + 1f)
        {
            midShown = true;
            RevealGroup(groupMid);
        }

        if (!midShown && currentRightZ > mapPaper.localPosition.z + 1.8f)
        {
            leftShown = true;
            RevealGroup(groupLeft);
            
            rightShown = true;
            RevealGroup(groupRight);
        }
    }

    private void RevealGroup(GameObject[] group)
    {
        foreach (GameObject obj in group)
        {
            if (obj == null) continue;

            obj.SetActive(true);

            MapPopIn pop = obj.GetComponent<MapPopIn>();
            if (pop != null)
                pop.Play();
        }
    }

    private IEnumerator RevealNodesRoutine()
    {
        foreach (GameObject node in nodes)
        {
            if (node == null) continue;

            node.SetActive(true);

            MapPopIn pop = node.GetComponent<MapPopIn>();
            if (pop != null)
                pop.Play();

            yield return new WaitForSeconds(nodeRevealDelay);
        }
    }

    private void SetGroupActive(GameObject[] group, bool active)
    {
        foreach (GameObject obj in group)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void SetNodesActive(bool active)
    {
        foreach (GameObject node in nodes)
        {
            if (node != null)
                node.SetActive(active);
        }
    }
}