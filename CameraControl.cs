using UnityEngine;
using UnityEnginerController;

public class CameraControl: MonoBehaviour
{
    public float m_DampTime = 0.2f;                 
    public float m_ScreenEdgeBuffer = 4f;          
    public float m_MinSize = 6.5f;                 
    [HideInInspector] public Transform[] m_Targets; 

    private Camera m_Camera;                        
    private float m_ZoomSpeed;                   
    private Vector3 m_MoveVelocity;                
    private Vector3 m_DesiredPosition;           


    private void Awake (){
        m_Camera = GetComponentInChildren<Camera> ();
    }


    private void FixedUpdate (){
  
        Move ();

        Zoom ();
    }


    private void Move (){

        FindAveragePosition ();

        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition ()
    {
        Vector3 averagePos = new Vector3 ();
        
        int numTargets = 0;

        for (int i = 0; i < m_Targets.Length; i++)
        
        {

            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            averagePos += m_Targets[i].position;
            
            numTargets++;
        }


        if (numTargets > 0)
            averagePos /= numTargets;

        averagePos.y = transform.position.y;

        m_DesiredPosition = averagePos;
    }


    private void Zoom ()
    {

        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize ()
    {
        // Find the position the camera rig is moving towards in its local space.
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
       //////////
        float size = 0f;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            // ... and if they aren't active continue on to the next target.
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // Otherwise, find the position of the target in the camera's local space.
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

            // Find the position of the target from the desired position of the camera's local space.
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera.
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        }
        
        size += m_ScreenEdgeBuffer;

        size = Mathf.Max (size, m_MinSize);

        return size;
    }


    public void SetStartPositionAndSize ()
    {

        FindAveragePosition ();

        transform.position = m_DesiredPosition;

        m_Camera.orthographicSize = FindRequiredSize ();
    }
}
