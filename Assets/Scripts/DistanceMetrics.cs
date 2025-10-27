

using Unity.VisualScripting;
using UnityEngine;

public static class DistanceMetrics
{
    // dynamic time warping algorithm
    // https://en.wikipedia.org/wiki/Dynamic_time_warping
    // basically we compare two sequences of points and find minimum accumulated distance
    // after aligning them in time
    public static float DTW(Vector3[] a, Vector3[] b)
    {
        int lengthA = a.Length, lengthB = b.Length;

        // stores cumulative cost in matrix A+1 X B+1
        float[,] distanceMatrix = new float[lengthA + 1, lengthB + 1];

        // populate matrix with base case
        for (int i = 0; i < lengthA; i++) distanceMatrix[i, 0] = 1e9f;
        for (int j = 0; j < lengthB; j++) distanceMatrix[0, j] = 1e9f;

        distanceMatrix[0, 0] = 0f;
        for(int i = 1; i <= lengthA; i++)
        {
            for(int j = 1; j <= lengthB; j++)
            {
                // cost between current pair of points
                float pointDistance = Vector3.Distance(a[i - 1], b[j - 1]);

                // choose lowest cost previous alignment
                float bestPath = Mathf.Min(distanceMatrix[i - 1, j],
                    Mathf.Min(distanceMatrix[i, j - 1], distanceMatrix[i - 1, j - 1]));

                distanceMatrix[i, j] = pointDistance + bestPath;
            }
        }

        return distanceMatrix[lengthA, lengthB] / (lengthA + lengthB);
    }
    

    // discrete frechet distance
    // measures geometric similarity between two curves while respecting their order.
    // person walking a dog, each follows a path, frechet distance is the shortest leash
    // length needed for both to move from start to end.
    public static float DiscreteFrechet(Vector3[] a, Vector3[] b)
    {
        int lengthA = a.Length, lengthB = b.Length;
        float[,] cache = new float[lengthA, lengthB];

        for(int i = 0; i < lengthA; i++)
        {
            for (int j = 0; j < lengthB; j++)
            {
                cache[i, j] = -1f;
            }
        }

        return ComputeFrechet(lengthA - 1, lengthB - 1, a, b, cache);
    }

    static float ComputeFrechet(int indexA, int indexB, Vector3[] a, Vector3[] b, float[,] cache)
    {
        // if already computed return cache value
        if (cache[indexA, indexB] > -0.5f)
            return cache[indexA, indexB];

        float currentPointDistance = Vector3.Distance(a[indexA], b[indexB]);

        if (indexA == 0 && indexB == 0) // start of both sequences
        {
            cache[indexA, indexB] = currentPointDistance;
        }
        else if(indexA > 0 && indexB == 0) // if a has points and b doesnt
        {
            cache[indexA, indexB] = Mathf.Max(
                ComputeFrechet(indexA - 1, 0, a, b, cache), currentPointDistance
            );
        }
        else if (indexA == 0 && indexB > 0) // if a has points and b doesnt
        {
            cache[indexA, indexB] = Mathf.Max(
                ComputeFrechet(0, indexB - 1, a, b, cache), currentPointDistance
            );
        }
        else // both have points
        {
            float minPrev = Mathf.Min(
                ComputeFrechet(indexA - 1, indexB, a, b, cache),
                Mathf.Min(
                    ComputeFrechet(indexA - 1, indexB - 1, a, b, cache),
                    ComputeFrechet(indexA, indexB - 1, a, b, cache)
                )
            );

            cache[indexA, indexB] = Mathf.Max(minPrev, currentPointDistance);
        }

        return cache[indexA, indexB];
    }
}
