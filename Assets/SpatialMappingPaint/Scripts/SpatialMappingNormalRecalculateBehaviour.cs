using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

/// <summary>
/// Shaderで床を判定するために、法線ベクトルが必要となるので、バックグラウンドで定期的にSpatialMappingのメッシュの法線ベクトルを再計算するBehaviourクラス
/// </summary>
public class SpatialMappingNormalRecalculateBehaviour : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// How much time (in sec), while running in the Unity Editor, to allow RemoveSurfaceVertices to consume before returning control to the main program.
    /// </summary>
    private static readonly float FrameTime = .016f;
#else
        /// <summary>
        /// How much time (in sec) to allow RemoveSurfaceVertices to consume before returning control to the main program.
        /// </summary>
        private static readonly float FrameTime = .008f;
#endif

    bool recalculatingNomals = false;
    public void Start()
    {
        this.recalculatingNomals = false;
    }

    public void Update()
    {
        if (!recalculatingNomals)
        {
            this.recalculatingNomals = true;
            StartCoroutine(this.UpdateNormals());
        }
    }

    private IEnumerator UpdateNormals()
    {
        yield return null;
        float start = Time.realtimeSinceStartup;

        List<MeshFilter> filters = SpatialMappingManager.Instance.GetMeshFilters();
        for (int index = 0; index < filters.Count; index++)
        {
            MeshFilter filter = filters[index];
            if (filter != null && filter.sharedMesh != null)
            {
                filter.sharedMesh.RecalculateNormals();
            }

            if ((Time.realtimeSinceStartup - start) > FrameTime)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        this.recalculatingNomals = false;
    }
}
