using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Daily Characters")]
    public List<CharacterData> dailyCustomers;
    private int currentCustomerIndex = 0;

    [Header("Scene References")]
    public GameObject customerPrefab;

    // THE NEW MOVEMENT MARKERS
    public Transform doorLocation;
    public Transform counterLocation;

    public PotionBrewing cauldron;

    void Start()
    {
        SpawnNextCustomer();
    }

    public void SpawnNextCustomer()
    {
        if (currentCustomerIndex < dailyCustomers.Count)
        {
            // 1. Spawn them AT THE DOOR
            GameObject newCustomer = Instantiate(customerPrefab, doorLocation.position, Quaternion.identity);

            CustomerController controller = newCustomer.GetComponent<CustomerController>();

            // 2. Hand them their profile and map
            controller.myProfile = dailyCustomers[currentCustomerIndex];
            controller.brewingSystem = cauldron;
            controller.doorLocation = doorLocation;
            controller.counterLocation = counterLocation;

            currentCustomerIndex++;
        }
        else
        {
            Debug.Log("No more customers waiting! The tavern is empty.");
        }
    }
}