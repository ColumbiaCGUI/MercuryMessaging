using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Random = System.Random;
using RandomRange = UnityEngine.Random;
using Math = System.Math;
using System;


public static class Utils
{
    public static byte[] CombineBytes(byte[] first, byte[] second)
    {
        byte[] bytes = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
        return bytes;
    }

    private static Random rng = new Random();

    public static void ShuffleFisherYates<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    public static void Shuffle<T>(this IList<T> list)
    {
        Random rnd = new Random();

        for (var i = list.Count; i > 0; i--)
            list.Swap(0, rnd.Next(0, i));
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        Random rnd = new Random();

        for (var i = list.Count; i > 0; i--)
            list.Swap(0, rnd.Next(0, i));
        return list;
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    public static Color ColorFromValue(float value, float minValue, float maxValue)
    {
        // normalize the value
        float v_norm = (value - minValue) / (maxValue - minValue);
        float h = (1 - v_norm);
        float s = 1f;
        float v = v_norm;
        return Color.HSVToRGB(h, s, v);
    }

    public static void CreateEyeHeadTracker()
    {
        GameObject headTracker = GameObject.Instantiate(Resources.Load("HeadTracker", typeof(GameObject))) as GameObject;
        GameObject viveSRRuntime = GameObject.Instantiate(Resources.Load("ViveSRRuntime", typeof(GameObject))) as GameObject;
        Debug.Log("Instantiated HeadTracker and ViveSRRuntime gameobjects");
    }

    /*    public Static void get_yaw(Vector3 v1, Vector3 v2)
        {
            Quaternion q = transform.rotation;
            float Pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
            float Yaw = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
            float Roll = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);

        }*/
    public static float RandomNormal(float mean, float std)
    {
        Random rand = new Random(); //reuse this if you are generating many
        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        float num = System.Convert.ToSingle(mean + std * randStdNormal); //random normal(mean,stdDev^2)

        return num;
    }

    public static float RandomNormal(float mean, float std, float min, float max)
    {
        Random rand = new Random(); //reuse this if you are generating many
        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        float num = System.Convert.ToSingle(mean + std * randStdNormal); //random normal(mean,stdDev^2)

        return Mathf.Min(Mathf.Max(min, num), max);
    }

    public static Vector2 RandomSampleAngle(float rad1, float rad2)
    {
        // Randomly select an angle between ran1 and rad2 ( in radians)
        float angle = RandomRange.Range(rad1, rad2);

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    // this function returns a random object from '/Resource/dirPath" 
    public static GameObject GetRandObjFromDir(string dir)
    {
        GameObject randObj;
        GameObject[] objListArray;
        objListArray = Resources.LoadAll<GameObject>(dir);
        int randIndex = RandomRange.Range(0, objListArray.Length);
        randObj = objListArray[randIndex];
        return randObj;
    }

    public static Quaternion GetRotatationCurrentFacingToObject(GameObject rotatingGameObject, GameObject targetGameObject)
    {
        Vector3 lookPos = rotatingGameObject.transform.position - targetGameObject.transform.position;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        return rotation * rotatingGameObject.transform.rotation;
    }

    public static Vector3 NormalizeCenterToPosition(Vector3 target, float distanceToCenter, Vector3 pos)
    {
        return target.normalized * distanceToCenter + pos;
    }


    public static float CalculateAngle(Vector3 from, Vector3 to, Vector3 reference)
    {
        return Quaternion.FromToRotation(reference, to - from).eulerAngles.z;
    }

    public static bool IsObjectInCamera(GameObject go, Camera cam)
    {
        Vector3 viewPos = cam.WorldToViewportPoint(go.transform.position);
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
        {
            return true;
        }
        else return false;
    }

    public static Rect GUIRectWithObjectForCam(GameObject go, Camera cam, float cameraTextureHeight)
    {
        Vector3 cen = go.GetComponent<Renderer>().bounds.center;
        Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z), cam, cameraTextureHeight),
               WorldToGUIPointWithTextureHeight(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z), cam, cameraTextureHeight)
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public static Vector2 WorldToGUIPointWithTextureHeight(Vector3 world, Camera cam, float cameraTextureHeight)
    {
        Vector2 screenPoint = cam.WorldToScreenPoint(world);
        //screenPoint.y = (float)Screen.height - screenPoint.y;
        screenPoint.y = cameraTextureHeight - screenPoint.y;
        return screenPoint;
    }

    public static Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (upSideDown)
                {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                }
                else
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }

    /// <summary>
    /// Find the four corners of the bounds that are closest to the camea.
    /// </summary>
    public static List<Vector3> BoundingPointsInWorldSpaceClosesToCamera(Bounds bounds, Camera cam, float distance)
    {
        Vector3 center = bounds.center;
        Vector3 extent = new Vector3(bounds.size.x / 2f, bounds.size.y / 2f, bounds.size.z / 2f);  // just bounds.size * 0.5 gives incorrect results
        List<Vector3> boundingPoints = new List<Vector3>
        {
             new Vector3(center.x-extent.x, center.y-extent.y, center.z-extent.z),
             new Vector3(center.x+extent.x, center.y-extent.y, center.z-extent.z),
             new Vector3(center.x-extent.x, center.y-extent.y, center.z+extent.z),
             new Vector3(center.x+extent.x, center.y-extent.y, center.z+extent.z),
             new Vector3(center.x-extent.x, center.y+extent.y, center.z-extent.z),
             new Vector3(center.x+extent.x, center.y+extent.y, center.z-extent.z),
             new Vector3(center.x-extent.x, center.y+extent.y, center.z+extent.z),
             new Vector3(center.x+extent.x, center.y+extent.y, center.z+extent.z)
        };
        Vector2[] extentPoints = new Vector2[8]
        {
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y-extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y-extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y-extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y-extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y+extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y+extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y+extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y+extent.y, center.z+extent.z), cam)
        };


        //boundingPoints = boundingPoints.OrderBy(p => WorldToScreenCam(p, cam).x).ToList();
        //List<Vector3> xMins = new List<Vector3> { boundingPoints[0], boundingPoints[1] };
        //List<Vector3> xMaxs = new List<Vector3> { boundingPoints[boundingPoints.Count-1], boundingPoints[boundingPoints.Count - 2] };

        //Vector3 BottomLeft = xMins.MinBy(x => WorldToScreenCam(x, cam).y);
        //Vector3 TopLeft = xMins.MaxBy(x => WorldToScreenCam(x, cam).y);

        //Vector3 BottomRight = xMaxs.MinBy(x => WorldToScreenCam(x, cam).y);
        //Vector3 TopRight = xMaxs.MaxBy(x => WorldToScreenCam(x, cam).y);




        //var a = extentPoints.OrderBy(p => p.x).ToList();
        //List<Vector3> xMinsScreen = new List<Vector3> { a[0], a[1] };
        //List<Vector3> xMaxsScreen = new List<Vector3> { a[a.Count - 1], a[a.Count - 2] };

        //Vector2 BottomLeftScreen = xMinsScreen.MinBy(p => p.y);
        //Vector2 TopLeftScreen = xMinsScreen.MaxBy(p => p.y);

        //Vector3 BottomRightScreen = xMaxsScreen.MinBy(p => p.y);
        //Vector3 TopRightScreen = xMaxsScreen.MaxBy(p => p.y);



        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        Vector3 BottomLeft = cam.ScreenToWorldPoint(new Vector3(min.x, min.y, distance));
        Vector3 TopRight = cam.ScreenToWorldPoint(new Vector3(max.x, max.y, distance));
        Vector3 TopLeft = cam.ScreenToWorldPoint(new Vector3(min.x, max.y, distance));
        Vector3 BottomRight = cam.ScreenToWorldPoint(new Vector3(max.x, min.y, distance));


        return new List<Vector3>{ BottomLeft, TopLeft, BottomRight, TopRight };
    }

    public static Rect GUIRectWithBounds(Bounds bounds, Camera cam)
    {
        Vector3 center = bounds.center;
        Vector3 extent = bounds.size / 2f;
        Vector2[] extentPoints = new Vector2[8]
        {
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
             //HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))

             WorldToScreenCam(new Vector3(center.x-extent.x, center.y-extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y-extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y-extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y-extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y+extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y+extent.y, center.z-extent.z), cam),
             WorldToScreenCam(new Vector3(center.x-extent.x, center.y+extent.y, center.z+extent.z), cam),
             WorldToScreenCam(new Vector3(center.x+extent.x, center.y+extent.y, center.z+extent.z), cam)
        };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public static Vector2 WorldToScreenCam(Vector3 world, Camera cam)
    {
        Vector2 screenPoint = cam.WorldToScreenPoint(world);
        //screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    /// <summary>
    /// Cap the values of the rectangle to the screen (no negative values, not greated than screen size) in GUI coordinate system
    /// GUI coordinate system = origin is top left
    /// </summary>
    /// <param name="item"> Rectangle in GUI coordinate system </param>
    /// <returns></returns>
    public static int[] GetCappedGUI(Rect item)
    {
        int xmincap = Mathf.Max(0, (int)item.x);
        int ymincap = Mathf.Max(0, (int)item.y);

        int xmaxcap = ((int)item.x + (int)item.width);
        int ymaxcap = ((int)item.y + (int)item.height);

        int box_y_min = Camera.main.pixelHeight - ymaxcap;

        return new int[] { xmincap, box_y_min, xmaxcap, ymaxcap };
    }

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
    
    public static Queue<T> ShuffleQueue<T>(Queue<T> originalQueue)
    {
        // Convert the queue to a list
        List<T> list = new List<T>(originalQueue);

        // Shuffle the list using Fisher-Yates algorithm
        
        list.ShuffleFisherYates();

        // Convert the list back to a queue
        return new Queue<T>(list);
    }
}

public enum TSGuidanceType : ushort
{
    None = 4,
    GroundTruth = 8,
    Identifier = 9,
}

public static class CameraEx
{
    public static bool IsObjectVisible(this UnityEngine.Camera @this, Bounds bounds)
    {
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(@this), bounds);
    }
}

public static class EnumerableExtensions
{
    public static TSource MaxBy<TSource, TProperty>(this IEnumerable<TSource> source,
        Func<TSource, TProperty> selector)
    {
        // check args        

        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new InvalidOperationException();

            var max = iterator.Current;
            var maxValue = selector(max);
            var comparer = Comparer<TProperty>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, maxValue) > 0)
                {
                    max = current;
                    maxValue = currentValue;
                }
            }

            return max;
        }
    }

    public static List<TSource> MaxsBy<TSource, TProperty>(this IEnumerable<TSource> source,
    Func<TSource, TProperty> selector)
    {
        // check args        

        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new InvalidOperationException();

            var max = iterator.Current;
            List<TSource> maxs = new List<TSource> { max };
            var maxValue = selector(max);
            var comparer = Comparer<TProperty>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, maxValue) > 0)
                {
                    max = current;
                    maxValue = currentValue;
                    maxs = new List<TSource> { max };
                }
                else if (comparer.Compare(currentValue, maxValue) == 0)
                {
                    maxs.Add(current);
                }
            }

            return maxs;
        }
    }

    public static TSource MinBy<TSource, TProperty>(this IEnumerable<TSource> source,
    Func<TSource, TProperty> selector)
    {
        // check args        

        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new InvalidOperationException();

            var min = iterator.Current;
            var minValue = selector(min);
            var comparer = Comparer<TProperty>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, minValue) < 0)
                {
                    min = current;
                    minValue = currentValue;
                }
            }

            return min;
        }
    }

    public static List<TSource> MinsBy<TSource, TProperty>(this IEnumerable<TSource> source, Func<TSource, TProperty> selector)
    {
        // check args        

        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                throw new InvalidOperationException();

            var min = iterator.Current;
            List<TSource> mins = new List<TSource> { min };
            var minValue = selector(min);
            var comparer = Comparer<TProperty>.Default;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentValue = selector(current);

                if (comparer.Compare(currentValue, minValue) < 0)
                {
                    min = current;
                    minValue = currentValue;
                    mins = new List<TSource> { min };
                }
                else if (comparer.Compare(currentValue, minValue) == 0)
                {
                    mins.Add(current);
                }
            }

            return mins;
        }
    }
}

