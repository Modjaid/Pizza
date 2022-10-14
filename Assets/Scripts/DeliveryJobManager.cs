using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class DeliveryJobManager : MonoBehaviour
{
    public static DeliveryJobManager Instance { get; private set; }
    public List<DeliveryJob> jobs;
    public List<Transform> playerStartLocations;
    public GameObject destinationPrefab;
    public PlayerAxisController player;
    public float prefferedDeliveryTime;
    public float distanceBetweenPathCoins;
    public NavMeshSurface coinNavMesh;
    public GameObject coinPrefab;

    private float estimatedDeliveryTime;
    private float timePassedSinceDeliveryStart;
    private Transform coinContainer;
    private GameObject currentDestination;
    private IEnumerator deliveryTimeCountdownRoutine;

    public DeliveryJob CurrentJob { get; private set; }

    public delegate void jobCompleteDelegate();
    public event jobCompleteDelegate OnJobComplete;

    public delegate void jobStartedDelegate();
    public event jobStartedDelegate OnJobStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (CurrentJob == null)
        {
            UIManager.Instance.HideDeliveryUI();
        }

        coinContainer = new GameObject("Coin Container").transform;
    }

    public void SelectNewJob()
    {
        DeliveryJob job;
        job = SelectAvaibleJob();
        if (job == null)
        {
            Debug.LogError($"[{this.name}] No avaible jobs.");
            return;
        }
        else CurrentJob = job;

        timePassedSinceDeliveryStart = 0f;
        CreateDestinationGO(job.destination.position);
        estimatedDeliveryTime = EstimateTimeNeededForDelivery(player.transform.position, job.destination.position);
        StartDeliveryCountdown();
        OnJobStarted?.Invoke();
        CreateCoinPathToDestination();
    }

    public void DeletePreviousJobSigns()
    {
        foreach(Transform child in coinContainer)
        {
            Destroy(child.gameObject);
        }

        if(currentDestination != null)
        {
            Destroy(currentDestination.gameObject);
        }

        CurrentJob = null;
    }

    public void AddDeliveryTime(float amount)
    {
        estimatedDeliveryTime += amount;

        UpdateDeliveryTimeLeftText();
    }

    public void UpdateDeliveryTimeLeftText()
    {
        float timeLeftForDelivery = estimatedDeliveryTime - timePassedSinceDeliveryStart;

        string titleText = "";
        if (timeLeftForDelivery > 0) titleText = "Time left:";
        if (timeLeftForDelivery < 0) titleText = "Late by:";

        UIManager.Instance.UpdateDeliveryTimeLeftText((int)timeLeftForDelivery, titleText);
    }

    public bool HasJob()
    {
        return CurrentJob != null;
    }

    public void OnArrivedInDestination()
    {
        UIManager.Instance.HideDeliveryUI();
        GameManager.Instance.OnSuccesfulDelivery();
        OnJobComplete?.Invoke();
        UIManager.Instance.ShowFineOrTipText(CalculateFineOrTip());

        CurrentJob = null;
    }

    public void SelectRandomPlayerPosition()
    {
        int index = Random.Range(0, playerStartLocations.Count);

        Physics.SyncTransforms();
        player.transform.position = playerStartLocations[index].position;
        Physics.SyncTransforms();
    }

    private void CreateDestinationGO(Vector3 position)
    {
        currentDestination = Instantiate(destinationPrefab, position, Quaternion.identity);
    }

    private void StartDeliveryCountdown()
    {
        if(deliveryTimeCountdownRoutine != null)
        StopCoroutine(deliveryTimeCountdownRoutine);
        deliveryTimeCountdownRoutine = DeliveryTimeCountdown();
        StartCoroutine(deliveryTimeCountdownRoutine);
    }

    private DeliveryJob SelectAvaibleJob()
    {
        if (jobs.Count > 0)
        {
            DeliveryJob selectedJob = null;

            float prefferedJobDistance = prefferedDeliveryTime * player.MaximalForwardSpeed;
            // Randomize this value a bit.
            prefferedDeliveryTime *= Random.Range(0.8f, 1.2f);

            foreach (DeliveryJob job in jobs)
            {
                if (selectedJob != null)
                {
                    float distanceToSelectedJob = Vector3.Distance(selectedJob.destination.position, player.transform.position);
                    float distance = Vector3.Distance(job.destination.position, player.transform.position);

                    if (Mathf.Abs(distance - prefferedJobDistance) < Mathf.Abs(distanceToSelectedJob - prefferedJobDistance))
                    {
                        selectedJob = job;
                    }
                }
                else
                {
                    selectedJob = job;
                }
            }

            return selectedJob;
        }
        else
        {
            Debug.LogError("Job not found");
            
            return null;
        }
    }

    private IEnumerator DeliveryTimeCountdown()
    {
        UIManager.Instance.UpdateDeliveryTimeLeftText((int)estimatedDeliveryTime, "");

        while (CurrentJob != null)
        {
            if (GameManager.Instance.GameStatus != GameStatus.Paused)
            {
                yield return new WaitForSeconds(1f);

                timePassedSinceDeliveryStart += 1f;
                UpdateDeliveryTimeLeftText();
            }
            else
            {
                yield return null;
            }
        }
    }

    private void CreateCoinPathToDestination()
    {
        if (distanceBetweenPathCoins == 0) return;

        NavMeshPath path = new NavMeshPath();

        GameObject agentGO = new GameObject("Coin Path Agent");
        NavMeshAgent agent = agentGO.AddComponent<NavMeshAgent>();
        agentGO.transform.position = player.transform.position;
        agent.radius = 10f;
        agent.agentTypeID = coinNavMesh.agentTypeID;

        if (agent.CalculatePath(CurrentJob.destination.position, path))
        {
            float distanceTravelled = 0f;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                float dist = Vector3.Distance(path.corners[i], path.corners[i + 1]);

                // todo: coins are spawned at 0 Y.
                // if spawn them directly on path with Y offset, sometimes they just clip underground.
                // probably some navmesh path generation stuff.

                Vector3 dirToNext = (path.corners[i + 1] - path.corners[i]).normalized;
                dirToNext.y = 0;
                Vector3 spawnPosition = path.corners[i];
                spawnPosition.y = 0;

                // Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 1000f);

                while (true)
                {
                    float distanceThatCanBeTravelled = dist;

                    if(distanceThatCanBeTravelled >= distanceBetweenPathCoins - distanceTravelled)
                    {
                        spawnPosition += dirToNext * (distanceBetweenPathCoins - distanceTravelled);
                        Instantiate(coinPrefab, new Vector3(spawnPosition.x, 1.5f, spawnPosition.z), Quaternion.identity, coinContainer);
                        dist -= distanceBetweenPathCoins - distanceTravelled;
                        distanceTravelled = 0f;
                    }
                    else
                    {
                        distanceTravelled += distanceThatCanBeTravelled;

                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Path not found.");
        }
    }

private float CalculateFineOrTip()
{
    float value = 0;
    float maxTipSize = 100f;
    float maxFineSize = 150f;

    // If time deliverd in is +- 20% from estimated time for delivery, no fines or tips.
    if (timePassedSinceDeliveryStart > estimatedDeliveryTime * 0.8f &&
    timePassedSinceDeliveryStart < estimatedDeliveryTime * 1.2f)
        return 0f;

    // Calculate tip size.
    if (timePassedSinceDeliveryStart < estimatedDeliveryTime)
        value = timePassedSinceDeliveryStart / estimatedDeliveryTime * maxTipSize;

    // Calculate fine size.
    if (timePassedSinceDeliveryStart > estimatedDeliveryTime)
        value = -(estimatedDeliveryTime / timePassedSinceDeliveryStart * maxFineSize);

    return value;
}

private float EstimateTimeNeededForDelivery(Vector3 departure, Vector3 destination)
{
    float straightDistance = Vector3.Distance(departure, destination);
    float avaibleTime = Mathf.FloorToInt(straightDistance / player.MaximalForwardSpeed) * 4f;

    return avaibleTime;
}
}