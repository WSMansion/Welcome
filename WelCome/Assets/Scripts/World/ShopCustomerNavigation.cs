using System;
using System.Collections.Generic;
using UnityEngine;
using WelCome.Core;

namespace WelCome.World
{
    /// <summary>
    /// Builds a small 2D walkability grid from scene colliders and supplies paths and browsing targets to customers.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class ShopCustomerNavigation : MonoBehaviour
    {
        [Header("顾客可行走范围")]
        [InspectorName("范围最小值")] public Vector2 navigationMin = new Vector2(5f, 3f);
        [InspectorName("范围最大值")] public Vector2 navigationMax = new Vector2(35f, 23f);
        [Min(0.2f), InspectorName("网格间距")] public float cellSize = 0.5f;
        [InspectorName("阻挡层")] public LayerMask obstacleMask = ~0;

        [Header("目标位置")]
        [InspectorName("室外活动范围最小值")] public Vector2 outdoorMin = new Vector2(13f, 5.7f);
        [InspectorName("室外活动范围最大值")] public Vector2 outdoorMax = new Vector2(27f, 8.4f);
        [InspectorName("进店门口位置")] public Vector2 doorwayPosition = new Vector2(20f, 9.6f);
        [InspectorName("柜台前停留位置")] public Vector2 counterPosition = new Vector2(20f, 17.6f);
        [Min(0.2f), InspectorName("室内外分界高度")] public float outdoorThresholdY = 9.25f;

        private bool[,] walkable;
        private int gridWidth;
        private int gridHeight;
        private readonly List<ShopDisplayCaseView> displayCases = new List<ShopDisplayCaseView>();
        private readonly List<ShopWaypointGizmo> routeMarkers = new List<ShopWaypointGizmo>();
        private readonly Dictionary<ShopDisplayCaseView, ShopCustomerWanderAI> browseReservations =
            new Dictionary<ShopDisplayCaseView, ShopCustomerWanderAI>();

        public bool IsReady => walkable != null;

        private void OnEnable()
        {
            EventBus.Subscribe<DisplayCasesChangedEvent>(OnDisplayCasesChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DisplayCasesChangedEvent>(OnDisplayCasesChanged);
        }

        private void Start()
        {
            RefreshNavigation();
        }

        public void RefreshNavigation()
        {
            Physics2D.SyncTransforms();
            BuildWalkabilityGrid();
            RefreshTargets();
        }

        public bool IsOutside(Vector2 position) => position.y < outdoorThresholdY;

        public bool TryGetRandomOutdoorPoint(out Vector2 point)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 candidate = new Vector2(
                    UnityEngine.Random.Range(outdoorMin.x, outdoorMax.x),
                    UnityEngine.Random.Range(outdoorMin.y, outdoorMax.y));
                if (TrySnapToWalkable(candidate, out point) && IsOutside(point))
                    return true;
            }

            Vector2 fallback = doorwayPosition;
            return TrySnapToWalkable(fallback, out point);
        }

        public bool TryGetInteriorWanderPoint(out Vector2 point)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                if (routeMarkers.Count > 0)
                {
                    ShopWaypointGizmo marker = routeMarkers[UnityEngine.Random.Range(0, routeMarkers.Count)];
                    if (marker == null || marker.transform.position.y < outdoorThresholdY)
                        continue;
                    if (TrySnapToWalkable(marker.transform.position, out point) && !IsOutside(point))
                        return true;
                }
            }

            point = counterPosition;
            return TrySnapToWalkable(point, out point);
        }

        public bool TryGetBrowseTarget(ShopCustomerWanderAI customer, out ShopDisplayCaseView displayCase)
        {
            displayCase = null;
            ReleaseBrowseTarget(customer);

            List<ShopDisplayCaseView> candidates = new List<ShopDisplayCaseView>();
            foreach (ShopDisplayCaseView candidate in displayCases)
            {
                if (candidate == null || !candidate.isActiveAndEnabled || !candidate.IsUnlocked
                    || candidate.browsingPoint == null || browseReservations.ContainsKey(candidate))
                    continue;
                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
                return false;

            for (int attempt = 0; attempt < candidates.Count; attempt++)
            {
                int index = UnityEngine.Random.Range(0, candidates.Count);
                ShopDisplayCaseView candidate = candidates[index];
                candidates.RemoveAt(index);
                if (!TryFindPath(customer.transform.position, candidate.BrowsingPosition, new List<Vector2>()))
                    continue;

                browseReservations[candidate] = customer;
                displayCase = candidate;
                return true;
            }

            return false;
        }

        public void ReleaseBrowseTarget(ShopCustomerWanderAI customer)
        {
            if (customer == null)
                return;

            ShopDisplayCaseView reserved = null;
            foreach (KeyValuePair<ShopDisplayCaseView, ShopCustomerWanderAI> reservation in browseReservations)
            {
                if (reservation.Value == customer)
                {
                    reserved = reservation.Key;
                    break;
                }
            }

            if (reserved != null)
                browseReservations.Remove(reserved);
        }

        public bool TryFindPath(Vector2 start, Vector2 destination, List<Vector2> path)
        {
            if (path == null)
                return false;
            path.Clear();
            if (walkable == null)
                RefreshNavigation();
            if (walkable == null || gridWidth == 0 || gridHeight == 0)
                return false;

            if (!TryGetClosestWalkableCell(start, out Vector2Int startCell)
                || !TryGetClosestWalkableCell(destination, out Vector2Int goalCell))
                return false;

            if (startCell == goalCell)
            {
                path.Add(CellToWorld(goalCell));
                return true;
            }

            int[,] cost = new int[gridWidth, gridHeight];
            bool[,] closed = new bool[gridWidth, gridHeight];
            bool[,] hasParent = new bool[gridWidth, gridHeight];
            Vector2Int[,] parent = new Vector2Int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    cost[x, y] = int.MaxValue;

            MinHeap open = new MinHeap();
            cost[startCell.x, startCell.y] = 0;
            open.Push(new SearchNode(startCell, Heuristic(startCell, goalCell)));
            bool found = false;

            while (open.Count > 0)
            {
                SearchNode currentNode = open.Pop();
                Vector2Int current = currentNode.Cell;
                if (closed[current.x, current.y])
                    continue;
                closed[current.x, current.y] = true;

                if (current == goalCell)
                {
                    found = true;
                    break;
                }

                VisitNeighbor(current, current + Vector2Int.up, goalCell, cost, closed, hasParent, parent, open);
                VisitNeighbor(current, current + Vector2Int.down, goalCell, cost, closed, hasParent, parent, open);
                VisitNeighbor(current, current + Vector2Int.left, goalCell, cost, closed, hasParent, parent, open);
                VisitNeighbor(current, current + Vector2Int.right, goalCell, cost, closed, hasParent, parent, open);
            }

            if (!found)
                return false;

            List<Vector2> reversePath = new List<Vector2>();
            Vector2Int cursor = goalCell;
            while (cursor != startCell)
            {
                reversePath.Add(CellToWorld(cursor));
                if (!hasParent[cursor.x, cursor.y])
                    return false;
                cursor = parent[cursor.x, cursor.y];
            }
            reversePath.Reverse();
            path.AddRange(reversePath);

            if (IsWorldPointWalkable(destination)
                && (path.Count == 0 || (path[path.Count - 1] - destination).sqrMagnitude > 0.01f))
                path.Add(destination);

            return path.Count > 0;
        }

        private void BuildWalkabilityGrid()
        {
            float safeCellSize = Mathf.Max(0.2f, cellSize);
            gridWidth = Mathf.Max(1, Mathf.CeilToInt((navigationMax.x - navigationMin.x) / safeCellSize));
            gridHeight = Mathf.Max(1, Mathf.CeilToInt((navigationMax.y - navigationMin.y) / safeCellSize));
            walkable = new bool[gridWidth, gridHeight];
            Vector2 testSize = Vector2.one * safeCellSize * 0.72f;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2 cellCenter = CellToWorld(new Vector2Int(x, y));
                    Collider2D[] hits = Physics2D.OverlapBoxAll(cellCenter, testSize, 0f, obstacleMask.value);
                    bool blocked = false;
                    foreach (Collider2D hit in hits)
                    {
                        if (hit == null || hit.isTrigger)
                            continue;
                        if (hit.GetComponentInParent<ShopVisitorWander>() != null
                            || hit.GetComponentInParent<ShopPlayerController>() != null)
                            continue;

                        blocked = true;
                        break;
                    }
                    walkable[x, y] = !blocked;
                }
            }
        }

        private void RefreshTargets()
        {
            displayCases.Clear();
            displayCases.AddRange(FindObjectsOfType<ShopDisplayCaseView>(true));
            displayCases.Sort((left, right) => left.ConfigId.CompareTo(right.ConfigId));

            routeMarkers.Clear();
            routeMarkers.AddRange(FindObjectsOfType<ShopWaypointGizmo>());
        }

        private void OnDisplayCasesChanged(DisplayCasesChangedEvent evt)
        {
            Invoke(nameof(RefreshNavigation), 0f);
        }

        private void VisitNeighbor(Vector2Int current, Vector2Int neighbor, Vector2Int goal,
            int[,] cost, bool[,] closed, bool[,] hasParent, Vector2Int[,] parent, MinHeap open)
        {
            if (!IsInsideGrid(neighbor) || !walkable[neighbor.x, neighbor.y] || closed[neighbor.x, neighbor.y])
                return;

            int tentativeCost = cost[current.x, current.y] + 10;
            if (tentativeCost >= cost[neighbor.x, neighbor.y])
                return;

            cost[neighbor.x, neighbor.y] = tentativeCost;
            parent[neighbor.x, neighbor.y] = current;
            hasParent[neighbor.x, neighbor.y] = true;
            open.Push(new SearchNode(neighbor, tentativeCost + Heuristic(neighbor, goal)));
        }

        private bool TrySnapToWalkable(Vector2 position, out Vector2 snapped)
        {
            if (TryGetClosestWalkableCell(position, out Vector2Int cell))
            {
                snapped = CellToWorld(cell);
                return true;
            }

            snapped = position;
            return false;
        }

        private bool TryGetClosestWalkableCell(Vector2 position, out Vector2Int result)
        {
            Vector2Int center = WorldToCell(position);
            if (IsInsideGrid(center) && walkable[center.x, center.y])
            {
                result = center;
                return true;
            }

            for (int radius = 1; radius <= 12; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int remainingY = radius - Mathf.Abs(dx);
                    Vector2Int lower = new Vector2Int(center.x + dx, center.y - remainingY);
                    if (IsInsideGrid(lower) && walkable[lower.x, lower.y])
                    {
                        result = lower;
                        return true;
                    }

                    Vector2Int upper = new Vector2Int(center.x + dx, center.y + remainingY);
                    if (remainingY > 0 && IsInsideGrid(upper) && walkable[upper.x, upper.y])
                    {
                        result = upper;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private bool IsWorldPointWalkable(Vector2 position)
        {
            Vector2Int cell = WorldToCell(position);
            return IsInsideGrid(cell) && walkable[cell.x, cell.y];
        }

        private bool IsInsideGrid(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
        }

        private Vector2Int WorldToCell(Vector2 position)
        {
            float safeCellSize = Mathf.Max(0.2f, cellSize);
            return new Vector2Int(
                Mathf.RoundToInt((position.x - navigationMin.x) / safeCellSize - 0.5f),
                Mathf.RoundToInt((position.y - navigationMin.y) / safeCellSize - 0.5f));
        }

        private Vector2 CellToWorld(Vector2Int cell)
        {
            float safeCellSize = Mathf.Max(0.2f, cellSize);
            return new Vector2(
                navigationMin.x + (cell.x + 0.5f) * safeCellSize,
                navigationMin.y + (cell.y + 0.5f) * safeCellSize);
        }

        private static int Heuristic(Vector2Int from, Vector2Int to)
        {
            return (Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y)) * 10;
        }

        private readonly struct SearchNode
        {
            public readonly Vector2Int Cell;
            public readonly int Priority;

            public SearchNode(Vector2Int cell, int priority)
            {
                Cell = cell;
                Priority = priority;
            }
        }

        private sealed class MinHeap
        {
            private readonly List<SearchNode> items = new List<SearchNode>();
            public int Count => items.Count;

            public void Push(SearchNode node)
            {
                items.Add(node);
                int index = items.Count - 1;
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (items[parent].Priority <= items[index].Priority)
                        break;
                    Swap(parent, index);
                    index = parent;
                }
            }

            public SearchNode Pop()
            {
                SearchNode result = items[0];
                SearchNode last = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                if (items.Count == 0)
                    return result;

                items[0] = last;
                int index = 0;
                while (true)
                {
                    int left = index * 2 + 1;
                    int right = left + 1;
                    if (left >= items.Count)
                        break;
                    int smallest = right < items.Count && items[right].Priority < items[left].Priority
                        ? right
                        : left;
                    if (items[index].Priority <= items[smallest].Priority)
                        break;
                    Swap(index, smallest);
                    index = smallest;
                }

                return result;
            }

            private void Swap(int left, int right)
            {
                SearchNode temp = items[left];
                items[left] = items[right];
                items[right] = temp;
            }
        }
    }
}
