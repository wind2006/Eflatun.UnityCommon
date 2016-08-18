using System.Collections.Generic;
using System.Linq;
using UnityCSCommon.Utils.Common;
using UnityEngine;

namespace UnityCSCommon.Utils.ManualTracking2D
{
    /// <summary>
    /// Contains position and AABB data of a tracked GameObject.
    /// </summary>
    public class TrackedObjectData
    {
        public GameObject GameObject { get; private set; }

        public SpriteRenderer Renderer { get; private set; }

        private readonly IList<Vector2> _localConvexHull;
        public IList<Vector2> LocalConvexHull
        {
            get { return _localConvexHull; }
        }

        private readonly IList<Vector2> _worldConvexHull;
        public IList<Vector2> WorldConvexHull
        {
            get { return _worldConvexHull; }
        }

        public AABB AABB { get; private set; }

        private Vector2 _lastPosition;
        private Quaternion _lastRotation;

        public TrackedObjectData(GameObject toTrack, SpriteRenderer renderer, IList<Vector2> localConvexHull)
        {
            GameObject = toTrack;
            Renderer = renderer;
            _localConvexHull = localConvexHull;
            _worldConvexHull = localConvexHull.ToList();

            AABB = new AABB();

            Update();
        }

        public void Update()
        {
            Transform transform = GameObject.transform;
            Vector2 position = transform.Position2D();
            Quaternion rotation = transform.rotation;

            Matrix4x4 toWorldMatrix = transform.localToWorldMatrix;

            if (_lastPosition != position || _lastRotation != rotation)
            {
                UpdateWorldHull(toWorldMatrix);
                AABB.Update(Renderer.bounds);

                _lastPosition = position;
                _lastRotation = rotation;
            }
        }

        private void UpdateWorldHull(Matrix4x4 toWorld)
        {
            // Formulas used in transforming local to world have been directly taken from assmebly view of Matrix4x4.MultiplyPoint3x4.
            // I deleted everything related to Z axis, and this yielded a much more better performance.

            int count = _worldConvexHull.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 local = _localConvexHull[i];

                var x = toWorld.m00*local.x + toWorld.m01*local.y + toWorld.m03;
                var y = toWorld.m10*local.x + toWorld.m11*local.y + toWorld.m13;

                _worldConvexHull[i].Set(x, y);
            }
        }
    }
}