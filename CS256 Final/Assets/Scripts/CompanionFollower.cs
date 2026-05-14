using UnityEngine;

public class CompanionFollower : MonoBehaviour
{
    public Transform leader;
    public Vector3 offset = new Vector3(-1.2f, 0.1f, 0);
    public float followSpeed;

   
    public bool isWalkingOut = false;

    void Update()
    {
        if (leader == null) return;

        // If walking out, just follow the leader to the door
        // Otherwise, stay at the offset position behind them
        Vector3 targetPos = isWalkingOut ? leader.position : leader.position + offset;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}