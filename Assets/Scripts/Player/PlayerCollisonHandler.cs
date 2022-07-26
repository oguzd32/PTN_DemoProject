using DG.Tweening;
using UnityEngine;
using static Utilities;

public class PlayerCollisonHandler : MonoBehaviour
{
    // cached components
    private PlayerController controller;
    private PlayerMovement movement;

    // private variables
    private GameObject lastTriggeredObj = default;
    
    private void Start()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;

        // check same object multiple trigger
        if(lastTriggeredObj == otherObj) return;
        lastTriggeredObj = otherObj;
        
        switch (otherObj.tag)
        {
            case "Obstacle":
                
                controller.FailProcess();
                break;
            
            case "Finish":

                GameManager.Instance.EndGame(true);
                break;
            
        }
    }
}
