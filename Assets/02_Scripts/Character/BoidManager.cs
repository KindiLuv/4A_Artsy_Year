using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidPrefab; // Prefab du boid à instancier
    public int numberOfBoids = 10; // Nombre de boids à générer
    public float speed = 5f; // Vitesse de déplacement des boids
    public float rotationSpeed = 1f; // Vitesse de rotation des boids
    public float neighborRadius = 10f; // Rayon à l'intérieur duquel un boid considère les autres boids comme voisins
    public float separationDistance = 3f; // Distance de séparation entre les boids
    public Vector3 boundingBoxSize = new Vector3(20f, 20f, 20f); // Taille de la zone de bounding box

    private Rigidbody[] boids;

    private void Start()
    {
        boids = new Rigidbody[numberOfBoids];

        // Instancier les boids
        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-boundingBoxSize.x / 2, boundingBoxSize.x / 2),
                                                                     Random.Range(-boundingBoxSize.y / 2, boundingBoxSize.y / 2),
                                                                     Random.Range(-boundingBoxSize.z / 2, boundingBoxSize.z / 2));
            GameObject boid = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            boid.transform.SetParent(transform);
            boids[i] = boid.AddComponent<Rigidbody>();
            boids[i].velocity = Random.insideUnitSphere * speed;
        }
    }

    private void Update()
    {
        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector3 separationVector = Vector3.zero;
            Vector3 alignmentVector = Vector3.zero;
            Vector3 cohesionVector = Vector3.zero;

            int neighborCount = 0;

            for (int j = 0; j < numberOfBoids; j++)
            {
                if (j != i)
                {
                    Vector3 neighborPosition = boids[j].position;
                    float distance = Vector3.Distance(neighborPosition, boids[i].position);

                    if (distance <= separationDistance)
                    {
                        separationVector += (boids[i].position - neighborPosition).normalized / distance;
                    }

                    alignmentVector += boids[j].velocity;
                    cohesionVector += neighborPosition;

                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                alignmentVector /= neighborCount;
                alignmentVector = alignmentVector.normalized;

                cohesionVector /= neighborCount;
                cohesionVector = (cohesionVector - boids[i].position).normalized;
            }

            Vector3 newDirection = (separationVector + alignmentVector + cohesionVector).normalized;
            Quaternion rotation = Quaternion.LookRotation(newDirection);

            boids[i].rotation = Quaternion.Slerp(boids[i].rotation, rotation, rotationSpeed * Time.deltaTime);
            boids[i].velocity = boids[i].transform.forward * speed;

            Vector3 position = boids[i].position;
            position.y = Mathf.Clamp(position.y, transform.position.y - boundingBoxSize.y / 2, transform.position.y + boundingBoxSize.y / 2);

            position += boids[i].velocity * Time.deltaTime;
            boids[i].position = position;
        }
    }


    private bool IsInsideBoundingBox(Vector3 position)
    {
        Vector3 boxMin = transform.position - boundingBoxSize / 2;
        Vector3 boxMax = transform.position + boundingBoxSize / 2;
        return position.x >= boxMin.x && position.x <= boxMax.x &&
               position.y >= boxMin.y && position.y <= boxMax.y &&
               position.z >= boxMin.z && position.z <= boxMax.z;
    }
}
