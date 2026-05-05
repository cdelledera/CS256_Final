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

    // --- NEW: Track whoever is currently inside! ---
    private GameObject currentActiveCustomer;

    void Start()
    {
        SpawnNextCustomer();
    }

    void Update()
    {
        // --- NEW: The Bouncer Logic ---
        // If the active customer disappeared (left the tavern) AND there are people left in line...
        if (currentActiveCustomer == null && currentCustomerIndex < dailyCustomers.Count)
        {
            SpawnNextCustomer();
        }
    }

    public void SpawnNextCustomer()
    {
        if (currentCustomerIndex < dailyCustomers.Count)
        {
            // 1. Spawn them AT THE DOOR and save them as the active customer
            currentActiveCustomer = Instantiate(customerPrefab, doorLocation.position, Quaternion.identity);

            CustomerController controller = currentActiveCustomer.GetComponent<CustomerController>();

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