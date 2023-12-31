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

        public int GetDistance(Structure other)
        {
            int xDiff = position.x - other.position.x < 0 ? other.position.x - position.x : position.x - other.position.x;
            int yDiff = position.y - other.position.y < 0 ? other.position.y - position.y : position.y - other.position.y;
            return xDiff + yDiff;
        }
    }

    class PGNode
    {
        PGNode parent;
        PGNode[] children;
        List<Structure> rooms;
        int area;

        public PGNode Parent { get { return parent; } }
        public PGNode[] Children { get { return children; } set { children = value; } }
        public List<Structure> Rooms { get { return rooms; } set { rooms = value; } }
        public bool IsRoot { get { return parent == null; } }
        public bool IsLeaf { get { return children[0] == null && rooms.Count > 0; } }
        public Structure CenterRoom { get { return (rooms[rooms.Count >> 1]); } }
        public int Area { get { return area; } set { area = value; } }

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
        [SerializeField] Case2_PL player;

        Structure[,,] map;
        PGNode[] roots;
        public Structure[,,] Map { get { return map; } }
        public int Size { get { return size; } }

        [SerializeField][Range(1, 10)] int depth;
        [SerializeField][Range(100, 200)] int size;
        [SerializeField][Range(1, 10)] int quantity;
        [SerializeField][Range(100, 200)] int maxRoomSize;
        float calcQuantity;

        void Awake()
        {
            map = new Structure[depth, size, size];
            roots = new PGNode[depth];

            calcQuantity = maxRoomSize - ((maxRoomSize * quantity) >> 4);
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
                if(deep > 0)
                    StairGenerate(deep);
            }

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
            // 영역만을 선택하여 추가
            PGNode now = roots[deep];
            List<PGNode> area = new List<PGNode>(100);
            Queue<PGNode> findQueue = new Queue<PGNode>(100);
            findQueue.Enqueue(now);
            int count = 0;

            while(findQueue.Count > 0)
            {
                now = findQueue.Dequeue();

                if(now.IsLeaf)
                {
                    now.Area = count++;
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
            int[,] table = new int[count, count];
            for(int i = 0; i <  count; i++)
            {
                Structure currentCenterRoom = area[i].CenterRoom;
                for (int j = i + 1; j < count; j++)
                {
                    table[i, j] = currentCenterRoom.GetDistance(area[j].CenterRoom);
                    table[j, i] = table[i, j];
                }
            }

            // 최소 거리 간선 연결
            for (int i = 0; i < count; i++)
            {
                (int x, int y) currentRoomPosition = area[i].CenterRoom.Position;
                int minDistance = int.MaxValue;
                int minDistanceIndex = 0;
                int originArea = area[i].Area;
                for (int j = 0; j < count; j++)
                {
                    if (i == j || table[i, j] >= minDistance || originArea == area[j].Area)
                        continue;
                    minDistance = table[i, j];
                    minDistanceIndex = j;
                }

                int changeArea = area[minDistanceIndex].Area;
                if (changeArea == originArea)
                    continue;
                foreach (PGNode inList in area)
                {
                    if (inList.Area == changeArea)
                    {
                        inList.Area = originArea;
                    }
                }

                (int x, int y) minDistanceRoomPosition = area[minDistanceIndex].CenterRoom.Position;
                int xDiff = minDistanceRoomPosition.x - currentRoomPosition.x;
                int yDiff = minDistanceRoomPosition.y - currentRoomPosition.y;
                int xModifier = xDiff < 0 ? -1 : 1;
                int yModifier = yDiff < 0 ? -1 : 1;
                int x;

                for (x = 0; x != xDiff; x += xModifier)
                {
                    if (map[deep, currentRoomPosition.x + x, currentRoomPosition.y] == null)
                        map[deep, currentRoomPosition.x + x, currentRoomPosition.y] = new Structure(deep, (currentRoomPosition.x + x, currentRoomPosition.y), StructureType.Road);
                }
                for (int y = 0; y != yDiff; y += yModifier)
                {
                    if (map[deep, currentRoomPosition.x + x, currentRoomPosition.y + y] == null)
                        map[deep, currentRoomPosition.x + x, currentRoomPosition.y + y] = new Structure(deep, (currentRoomPosition.x + x, currentRoomPosition.y + y), StructureType.Road);
                }
            }
        }

        void StairGenerate(int deep)
        {
            int x, y;
            do
            {
                x = Random.Range(0, Size);
                y = Random.Range(0, Size);
            } while (map[deep - 1, x, y] == null || map[deep, x, y] == null);

            map[deep - 1, x, y].Type = StructureType.Stair;
            map[deep, x, y].Type = StructureType.Stair;
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
                                Instantiate(RoomPrefab, new Vector3(xi, yi, deep * -5), Quaternion.identity, transform);
                                break;
                            case StructureType.Road:
                                Instantiate(RoadPrefab, new Vector3(xi, yi, deep * -5), Quaternion.identity, transform);
                                break;
                            case StructureType.Stair:
                                Instantiate(StairPrefab, new Vector3(xi, yi, deep * -5), Quaternion.identity, transform);
                                break;
                        }
                    }
                }
            }

            (int deep, int x, int y) playerPosition = (0, 0, 0);
            playerPosition.deep = Random.Range(0, depth);
            do
            {
                playerPosition.x = Random.Range(0, size);
                playerPosition.y = Random.Range(0, size);
            } while (map[playerPosition.deep, playerPosition.x, playerPosition.y] == null);
            player.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.deep * -5);
            player.Initialize(this, playerPosition);
        }

        int GetArea(int x1, int x2, int y1, int y2)
        {
            int xDiff = x1 <= x2 ? x2 - x1 : x1 - x2;
            int yDiff = y1 <= y2 ? y2 - y1 : y1 - y2;

            return xDiff * yDiff;
        }
    }

}