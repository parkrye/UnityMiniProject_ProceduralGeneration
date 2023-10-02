using System.Collections.Generic;
using UnityEngine;

namespace Case1
{
    public enum StructureType { None, Room, Road, Stair }

    public class Case1_PG : MonoBehaviour
    {
        struct Structure
        {
            public int d, x, y, a;
            public StructureType t;

            public Structure(int _d, int _x, int _y, StructureType _t, int _a = -1)
            {
                d = _d;
                x = _x;
                y = _y;
                t = _t;
                a = _a;
            }
        }

        [SerializeField] GameObject PlayerPrefab, RoomPrefab, RoadPrefab, StairPrefab;
        [SerializeField] int width, height, depth;

        Structure[,,] map;
        int roomCount, areaNum;
        List<Structure> roomList;

        void Awake()
        {
            if(width < 10) width = 10;
            if(height < 10) height = 10;
            if(depth < 1) depth = 1;

            roomCount = (width * height) >> 3;
            areaNum = 0;

            map = new Structure[depth, width, height];
            roomList = new();
        }

        void Start()
        {
            // 단일 방 생성
            for(int i = 0; i < depth; i++)
            {
                RoomSetting(i, 0, roomCount);
            }

            // 방 연결하여 영역 구성
            AreaSetting();

            // 영역 간 통로 생성
            RoadSetting();

            // 프리팹 생성
            InstantiateObjects();
        }

        void RoomSetting(int depth, int count, int limit)
        {
            Queue<(int, int, int, int)> roomStack = new();
            roomStack.Enqueue((0, width, 0, height));

            while (roomStack.Count > 0 && count < limit)
            {
                (int xLow, int xHigh, int yLow, int yHigh) = roomStack.Dequeue();

                // 생성 예정 방 위치
                int nextX = Random.Range(xLow, xHigh);
                nextX = nextX >= width ? width - 1 : nextX;
                int nextY = Random.Range(yLow, yHigh);
                nextY = nextY >= height ? height - 1 : nextY;

                // 생성 위치가 비어있다면 방 생성
                if (map[depth, nextX, nextY].t == StructureType.None)
                {
                    Structure created = new Structure(depth, nextX, nextY, StructureType.Room);
                    map[depth, nextX, nextY] = created;
                    roomList.Add(created);
                    count++;
                }

                // 생성 위치를 기점으로 x, y 범위가 존재한다면 분할
                if (xHigh - xLow > 1 || yHigh - yLow > 1)
                {
                    roomStack.Enqueue((xLow,          nextX, yLow,          nextY));
                    roomStack.Enqueue((xLow,          nextX, yHigh - nextY, yHigh));
                    roomStack.Enqueue((xHigh - nextX, xHigh, yLow,          nextY));
                    roomStack.Enqueue((xHigh - nextX, xHigh, yHigh - nextY, yHigh));
                }
            }
        }

        void AreaSetting()
        {
            foreach(Structure room in roomList)
            {
                if (map[room.d, room.x, room.y].a >= 0)
                    continue;

                bool findMerge = false;
                Queue<(int x, int y)> queue = new();
                Stack<(int x, int y)> merge = new();
                Dictionary<(int x, int y), (int x, int y)> parents = new();
                bool[,] visited = new bool[width, height];

                queue.Enqueue((room.x, room.y));
                parents.Add((room.x, room.y), (-1, -1));
                visited[room.x, room.y] = true;

                while (queue.Count > 0)
                {
                    (int x, int y) find = queue.Dequeue();

                    if (map[room.d, find.x, find.y].t == StructureType.Room && find.x != room.x && find.y != room.y)
                    {
                        findMerge = true;
                        while (parents[(find.x, find.y)] != (-1, -1))
                        {
                            merge.Push((find.x, find.y));
                            find = parents[(find.x, find.y)];
                        }
                        break;
                    }

                    for (int i = -1; i <= 1; i += 2)
                    {
                        if (i < 0 ? find.x > 0 : find.x < width - 1)
                        {
                            (int x, int y) next = (find.x + i, find.y);
                            if (!visited[next.x, next.y])
                            {
                                queue.Enqueue(next);
                                parents.Add(next, find);
                                visited[next.x, next.y] = true;
                            }
                        }
                        if (i < 0 ? find.y > 0 : find.y < height - 1)
                        {
                            (int x, int y) next = (find.x, find.y + i);
                            if (!visited[next.x, next.y])
                            {
                                queue.Enqueue(next);
                                parents.Add(next, find);
                                visited[next.x, next.y] = true;
                            }
                        }
                    }
                }
                if (findMerge)
                {
                    while (merge.Count > 0)
                    {
                        (int x, int y) pop = merge.Pop();
                        roomList.Add(new Structure(room.d, pop.x, pop.y, StructureType.Room, areaNum));
                        map[room.d, pop.x, pop.y].t = StructureType.Room;
                        map[room.d, pop.x, pop.y].a = areaNum;
                    }
                }

                areaNum++;
            }
        }

        void RoadSetting()
        {
            bool[,] connectTable = new bool[areaNum, areaNum];
            bool[] visited = new bool[areaNum];


        }

        void InstantiateObjects()
        {
            for(int d = 0; d < depth; d++)
            {
                for(int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        switch(map[d, x, y].t)
                        {
                            case StructureType.Room:
                                Instantiate(RoomPrefab, new Vector2(x, y + height * 2 * d), Quaternion.identity, transform);
                                break;
                            case StructureType.Road:
                                Instantiate(RoadPrefab, new Vector2(x, y + height * 2 * d), Quaternion.identity, transform);
                                break;
                            case StructureType.Stair:
                                Instantiate(StairPrefab, new Vector2(x, y + height * 2 * d), Quaternion.identity, transform);
                                break;
                        }
                    }
                }
            }
        }
    }

}