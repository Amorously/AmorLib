using AIGraph;
using AmorLib.Utils.Extensions;
using GTFO.API;
using GTFO.API.Extensions;
using LevelGeneration;
using UnityEngine;

namespace AmorLib.Utils;

[CallConstructorOnLoad]
public static class CourseNodeUtil // credits: Dinorush
{
    static CourseNodeUtil()
    {
        LevelAPI.OnAfterBuildBatch += OnAfterBuildBatch;
    }

    private readonly static Dictionary<eDimensionIndex, DimensionMap> _maps = new();

    private static void OnAfterBuildBatch(LG_Factory.BatchName batch)
    {
        if (batch != LG_Factory.BatchName.AIGraph_AirGraph_PostProcess) return;
        _maps.Clear();

        var clustersByDim = AIG_NodeCluster.AllNodeClusters.ToManaged().GroupBy(c => c.m_courseNode.m_dimension.DimensionIndex);
        foreach (var group in clustersByDim)
        {
            var dim = group.Key;
            var map = _maps.GetOrAddNew(dim);

            foreach (var cluster in group)
                foreach (var node in cluster.m_nodes)
                    map.UpdateBounds(node.Position);

            map.CreateNodeMap();

            foreach (var cluster in group)
            {
                var courseNode = cluster.CourseNode;
                var id = courseNode.NodeID;
                foreach (var node in cluster.m_nodes)
                {
                    var list = map.GetBuildNodeList(node.Position);
                    if (!list.Any(n => n.NodeID == id))
                        list.Add(courseNode);
                }
            }

            map.FinishBuild();
        }
        Logger.Warn("Built DimensionMaps");
    }

    public static AIG_CourseNode GetCourseNode(Vector3 position, eDimensionIndex dimensionIndex)
    {
        if (!_maps.TryGetValue(dimensionIndex, out var map))
        {
            Logger.Error($"No Position-To-Node map for dimension {dimensionIndex}!");
            return null!;
        }

        return map.GetNode(position);
    }

    class DimensionMap
    {
        private const float IndexSize = 4f;

        public List<AIG_CourseNode>?[,] BuildNodeMap;
        public AIG_CourseNode[,][] NodeMap;
        public (int x, int z) MinCellBound;
        public (int x, int z) MaxCellBound;
        public (int x, int z) MapSize;

        public DimensionMap()
        {
            BuildNodeMap = null!;
            NodeMap = null!;
            MapSize = (0, 0);
            MinCellBound = (int.MaxValue, int.MaxValue);
            MaxCellBound = (int.MinValue, int.MinValue);
        }

        // Expands the bounds to encapsulate the position
        internal void UpdateBounds(Vector3 position)
        {
            int x = (int)(position.x / IndexSize);
            int z = (int)(position.z / IndexSize);
            MinCellBound = (Math.Min(x, MinCellBound.x), Math.Min(z, MinCellBound.z));
            MaxCellBound = (Math.Max(x, MaxCellBound.x), Math.Max(z, MaxCellBound.z));
        }

        internal void CreateNodeMap()
        {
            MapSize = (MaxCellBound.x - MinCellBound.x + 1, MaxCellBound.z - MinCellBound.z + 1);
            BuildNodeMap = new List<AIG_CourseNode>[MapSize.x, MapSize.z];
            NodeMap = new AIG_CourseNode[MapSize.x, MapSize.z][];
        }

        internal (int x, int z) GetMapPos(Vector3 position)
        {
            return 
                (
                  Math.Clamp((int)(position.x / IndexSize) - MinCellBound.x, 0, MapSize.x - 1), 
                  Math.Clamp((int)(position.z / IndexSize) - MinCellBound.z, 0, MapSize.z - 1)
                );
        }

        internal List<AIG_CourseNode> GetBuildNodeList(Vector3 position)
        {
            var (x, z) = GetMapPos(position);
            var list = BuildNodeMap[x, z];
            list ??= BuildNodeMap[x, z] = new();
            return list;
        }

        internal void FinishBuild()
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                for (int z = 0; z < MapSize.z; z++)
                {
                    NodeMap[x, z] = BuildNodeMap[x, z]?.ToArray()!;
                }
            }
            BuildNodeMap = null!;
        }

        public AIG_CourseNode[] GetNodes(Vector3 position)
        {
            var (x, z) = GetMapPos(position);
            if (NodeMap[x, z] != null) return NodeMap[x, z];

            float maxRadius = Math.Max(MapSize.x, MapSize.z);
            // Check increasing rings to find a nearby node
            for (int radius = 1; radius < maxRadius; radius++)
            {
                int left = Math.Max(x - radius, 0);
                int right = Math.Min(x + radius, MapSize.x - 1);
                int top = Math.Max(z - radius, 0);
                int bottom = Math.Min(z + radius, MapSize.z - 1);

                for (int xPos = left; xPos <= right; xPos++)
                {
                    if (NodeMap[xPos, top] != null)
                        return NodeMap[xPos, top];
                    if (NodeMap[xPos, bottom] != null)
                        return NodeMap[xPos, bottom];
                }

                for (int zPos = top + 1; zPos <= bottom - 1; zPos++)
                {
                    if (NodeMap[left, zPos] != null)
                        return NodeMap[left, zPos];
                    if (NodeMap[right, zPos] != null)
                        return NodeMap[right, zPos];
                }
            }

            Logger.Error($"Unable to get any node for ({position})! How are you even playing the game?!");
            return null!;
        }

        public AIG_CourseNode GetNode(Vector3 position)
        {
            var list = GetNodes(position);
            if (list == null) return null!;
            else if (list.Length == 1) return list[0];

            // Retrieve the closest nodes in each course node
            (AIG_CourseNode, AIG_INode)[] bestNodes = new (AIG_CourseNode, AIG_INode)[list.Length];
            int index = 0;
            foreach (var courseNode in list)
            {
                float bestDistInNode = float.PositiveInfinity;
                AIG_INode bestNodeInNode = null!;
                foreach (var node in courseNode.m_nodeCluster.m_nodes)
                {
                    if (position.IsWithinSqrDistance(node.Position, bestDistInNode, out float sqrDist))
                    {
                        bestDistInNode = sqrDist;
                        bestNodeInNode = node;
                    }
                }
                bestNodes[index++] = (courseNode, bestNodeInNode);
            }

            // Determine the closest of the closest nodes
            // Prioritize below the position so hitting a roof uses the node with the roof
            bool bestIsValidHeight = false;
            float bestDist = float.PositiveInfinity;
            AIG_CourseNode bestNode = null!;
            foreach ((var courseNode, var node) in bestNodes)
            {
                bool validHeight = node.Position.y - 0.25f <= position.y;
                bool isWithinDist = position.IsWithinSqrDistance(node.Position, bestDist, out float sqrDist);
                if (!bestIsValidHeight && validHeight || bestIsValidHeight == validHeight && isWithinDist)
                {
                    bestIsValidHeight = validHeight;
                    bestDist = sqrDist;
                    bestNode = courseNode;
                }
            }

            return bestNode;
        }
    }
}
