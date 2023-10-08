using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Case2
{
    public enum StructureType { None, Room, Road, Stair }

    public class Structure
    {
        int depth;
        (int x, int y) position;
        StructureType type;

        public int Depth { get { return depth; } }
        public (int x, int y) Position { get { return position; } }
        public StructureType Type { get { return type; } set { type = value; } }

        public Structure(int _depth, (int, int) _position, StructureType _type = StructureType.None)
        {
            depth = _depth;
            position = _position;
            type = _type;
        }

        public int GetAbsDistance(Structure other)
        {
            int xDiff = position.x - other.position.x < 0 ? other.position.x - position.x : position.x - other.position.x;
            int yDiff = position.y - other.position.y < 0 ? other.position.y - position.y : position.y - other.position.y;
            return xDiff + yDiff;
        }

        public int GetDistance(Structure other)
        {
            return other.position.x - position.x + other.position.y - position.y;
        }
    }

    class PGNode
    {
        PGNode parent;
        PGNode[] children;
        List<Structure> rooms;

        public PGNode Parent { get { return parent; } }
        public PGNode[] Children { get { return children; } set { children = value; } }
        public List<Structure> Rooms { get { return rooms; } set { rooms = value; } }
        public bool IsRoot { get { return parent == null; } }
        public bool IsLeaf { get { return children[0] == null && rooms.Count > 0; } }
        public Structure CenterRoom { get { return (rooms[rooms.Count >> 1]); } }

        public PGNode(PGNode _parent = null)
        {
            parent = _parent;
            children = new PGNode[2];
            rooms = new List<Structure>();
        }

        public int GetRoomCount()
        {
            if(IsLeaf)
                return rooms.Count;

            return children[0].GetRoomCount() + children[1].GetRoomCount();
        }
    }

    public class Case2_PG : MonoBehaviour
    {
        [SerializeField] GameObject RoomPrefab, RoadPrefab, StairPrefab;

        Structure[,,] map;
        PGNode[] roots;
        public Structure[,,] Map { get { return map; } }

        [SerializeField][Range(1, 10)] int depth;
        [SerializeField][Range(100, 200)] int size;
        [SerializeField][Range(1, 10)] int quantity;
        [SerializeField][Range(100, 200)] int maxRoomSize;
        float calcQuantity;
        int areaCount;

        void Awake()
        {
            map = new Structure[depth, size, size];
            roots = new PGNode[depth];

            calcQuantity = maxRoomSize - maxRoomSize * quantity * 0.1f;
            areaCount = 0;
        }

        void Start()
        {
            Generate();
        }

        void Generate()
        {
            for(int deep = 0; deep < depth; deep++)
            {
                roots[deep] = new PGNode();
                RoomGenerate(deep, 0, size, 0, size, roots[deep]);
                RoadGenerate(deep);
            }

            StairGenerate();

            CreateMap();
        }

        void RoomGenerate(int deep, int x1, int x2, int y1, int y2, PGNode parent)
        {
            int area = GetArea(x1, x2, y1, y2);

            if (area < calcQuantity || x1 >= x2 - 1 || y1 >= y2 - 1)
                return;

            if (area <= maxRoomSize)
            {
                for (int xi = x1; xi < x2; xi++)
                {
                    for(int  yi = y1; yi < y2; yi++)
                    {
                        map[deep, xi, yi] = new Structure(deep, (xi, yi), StructureType.Room);
                        parent.Rooms.Add(map[deep, xi, yi]);
                    }
                }
                areaCount++;
                return;
            }

            PGNode child1 = new(parent), child2 = new(parent);
            parent.Children[0] = child1; parent.Children[1] = child2;

            if (x2 - x1 >= y2 - y1)
            {
                int nextX = Random.Range(x1, x2);
                RoomGenerate(deep, nextX + 1, x2, y1, y2, child1);
                RoomGenerate(deep, x1, nextX - 1, y1, y2, child2);
            }
            else
            {
                int nextY = Random.Range(y1, y2);
                RoomGenerate(deep, x1, x2, nextY + 1, y2, child1);
                RoomGenerate(deep, x1, x2, y1, nextY - 1, child2);
            }
        }

        void RoadGenerate(int deep)
        {
            PGNode now = roots[deep];
            List<PGNode> area = new List<PGNode>(areaCount);
            Queue<PGNode> findQueue = new Queue<PGNode>(areaCount * 2);

            findQueue.Enqueue(now);

            // 영역만을 선택하여 추가
            while(findQueue.Count > 0)
            {
                now = findQueue.Dequeue();

                if(now.IsLeaf)
                {
                    area.Add(now);
                    continue;
                }

                for(int i = 0; i < 2; i++)
                {
                    if (now.Children[i] != null)
                        findQueue.Enqueue(now.Children[i]);
                }
            }

            // 영역 간의 간선 테이블
            int[,] table = new int[areaCount, areaCount];
            for(int i = 0; i <  areaCount; i++)
            {
                Structure currentCenterRoom = area[i].CenterRoom;
                for (int j = i + 1; j < areaCount; j++)
                {
                    table[i, j] = currentCenterRoom.GetAbsDistance(area[j].CenterRoom);
                }
            }

            // 최소 거리 간선 연결
            for (int i = 0; i < areaCount; i++)
            {
                (int x, int y) currentRoomPosition = area[i].CenterRoom.Position;
                int minDistance = int.MaxValue;
                int minDistanceIndex = 0;
                for (int j = i + 1; j < areaCount; j++)
                {
                    if (table[i, j] < minDistance)
                    {
                        minDistance = table[i, j];
                        minDistanceIndex = j;
                    }
                }
                (int x, int y) minDistanceRoomPosition = area[minDistanceIndex].CenterRoom.Position;
                int xDiff = minDistanceRoomPosition.x - currentRoomPosition.x;
                int yDiff = minDistanceRoomPosition.y - currentRoomPosition.y;
                int xModifier = xDiff < 0 ? -1 : 1;
                int yModifier = yDiff < 0 ? -1 : 1;

                if(xDiff != 0 && yDiff != 0)
                {
                    float grad = yDiff / xDiff;
                    for(int x = 0; x != xDiff; x += xModifier)
                    {
                        if (map[deep, currentRoomPosition.x + x, currentRoomPosition.y] == null)
                            map[deep, currentRoomPosition.x + x, currentRoomPosition.y] = new Structure(deep, (currentRoomPosition.x + x, currentRoomPosition.y), StructureType.Room);
                        for(int y = 0; y < grad && y > -grad && y >= 0 && y < size; y += yModifier)
                        {
                            if (map[deep, currentRoomPosition.x + x, currentRoomPosition.y + y] == null)
                                map[deep, currentRoomPosition.x + x, currentRoomPosition.y + y] = new Structure(deep, (currentRoomPosition.x + x, currentRoomPosition.y + y), StructureType.Room);
                        }
                    }
                }
                else if(xDiff == 0 && yDiff != 0)
                {
                    for (int y = currentRoomPosition.y; y < minDistanceRoomPosition.y && y > -minDistanceRoomPosition.y; y += yModifier)
                    {
                        if (map[deep, currentRoomPosition.x, y] == null)
                            map[deep, currentRoomPosition.x, y] = new Structure(deep, (currentRoomPosition.x, y), StructureType.Room);
                    }
                }
                else if(yDiff == 0)
                {
                    for(int x = currentRoomPosition.x; x < minDistanceRoomPosition.x && x > -minDistanceRoomPosition.x; x += xModifier)
                    {
                        if (map[deep, x, currentRoomPosition.y] == null)
                            map[deep, x, currentRoomPosition.y] = new Structure(deep, (x, currentRoomPosition.y), StructureType.Room);
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        void StairGenerate()
        {

        }

        void CreateMap()
        {
            for (int deep = 0; deep < depth; deep++)
            {
                for (int xi = 0; xi < size; xi++)
                {
                    for (int yi = 0; yi < size; yi++)
                    {
                        if (map[deep, xi, yi] == null)
                            continue;

                        switch (map[deep, xi, yi].Type)
                        {
                            default:
                            case StructureType.None:
                                break;
                            case StructureType.Room:
                                Instantiate(RoomPrefab, new Vector2(xi, yi), Quaternion.identity, transform);
                                break;
                            case StructureType.Road:
                                Instantiate(RoadPrefab, new Vector2(xi, yi), Quaternion.identity, transform);
                                break;
                            case StructureType.Stair:
                                Instantiate(StairPrefab, new Vector2(xi, yi), Quaternion.identity, transform);
                                break;
                        }
                    }
                }
            }
        }

        int GetArea(int x1, int x2, int y1, int y2)
        {
            int xDiff = x1 <= x2 ? x2 - x1 : x1 - x2;
            int yDiff = y1 <= y2 ? y2 - y1 : y1 - y2;

            return xDiff * yDiff;
        }
    }

}