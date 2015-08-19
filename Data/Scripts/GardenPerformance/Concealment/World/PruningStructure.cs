using System;
using System.Collections.Generic;
using VRageMath;

using Sandbox.Common;
using Sandbox.Game.Entities;
using Sandbox.Engine.Utils;

using MyDynamicAABBTree = VRageMath.MyDynamicAABBTree;


///<remarks>
/// Taken from the static SE class MyGamePruningStructure, adapted for flexible use
///</remarks>
namespace SEGarden.Math {

    public class PruningStructure {
        // A tree for each query type.
        // If you query for a specific type, consider adding a new QueryFlag and AABBTree (so that you don't have to filter the result afterwards).
        private MyDynamicAABBTreeD m_aabbTree;

        public PruningStructure() {
            Init();
        }

        private MyDynamicAABBTreeD GetPrunningStructure() {
            return m_aabbTree;
        }

        private Vector3D GAME_PRUNING_STRUCTURE_AABB_EXTENSION = new Vector3D(3.0f);

        private void Init() {
            m_aabbTree = new MyDynamicAABBTreeD(GAME_PRUNING_STRUCTURE_AABB_EXTENSION);
        }

        public BoundingBoxD GetEntityAABB(MyEntity entity) {
            BoundingBoxD bbox = entity.PositionComp.WorldAABB;

            //Include entity velocity to be able to hit fast moving objects
            if (entity.Physics != null) {
                bbox = bbox.Include(entity.WorldMatrix.Translation + entity.Physics.LinearVelocity * MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS * 5);
            }

            return bbox;
        }

        public void Add(MyEntity entity) {
            BoundingBoxD bbox = GetEntityAABB(entity);
            if (bbox.Size == Vector3D.Zero) return;  // don't add entities with zero bounding boxes

            entity.GamePruningProxyId = m_aabbTree.AddProxy(ref bbox, entity, 0);
        }

        public const int PRUNING_PROXY_ID_UNITIALIZED = -1;

        public void Remove(MyEntity entity) {
            if (entity.GamePruningProxyId != PRUNING_PROXY_ID_UNITIALIZED) {
                m_aabbTree.RemoveProxy(entity.GamePruningProxyId);
                entity.GamePruningProxyId = PRUNING_PROXY_ID_UNITIALIZED;
            }
        }

        public void Clear() {
            Init();
            m_aabbTree.Clear();
        }

        public void Move(MyEntity entity) {
            if (entity.GamePruningProxyId != PRUNING_PROXY_ID_UNITIALIZED) {
                BoundingBoxD bbox = GetEntityAABB(entity);

                if (bbox.Size == Vector3D.Zero)  // remove entities with zero bounding boxes
                {
                    Remove(entity);
                    return;
                }

                m_aabbTree.MoveProxy(entity.GamePruningProxyId, ref bbox, Vector3D.Zero);
            }
        }

        public void GetAllEntitiesInBox<T>(ref BoundingBoxD box, List<T> result) {
            m_aabbTree.OverlapAllBoundingBox<T>(ref box, result, 0, false);
        }

        public void GetAllEntitiesInSphere<T>(ref BoundingSphereD sphere, List<T> result) {
            m_aabbTree.OverlapAllBoundingSphere<T>(ref sphere, result, false);
        }

        public void GetAllEntitiesInRay<T>(ref LineD ray, List<MyLineSegmentOverlapResult<T>> result) {
            m_aabbTree.OverlapAllLineSegment<T>(ref ray, result);
        }

    }
}