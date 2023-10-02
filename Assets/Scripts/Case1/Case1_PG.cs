using System.Collections.Generic;
using UnityEngine;

namespace Case1
{
    public enum StructureType { None, Room, Stair }

    public class Case1_PG : MonoBehaviour
    {
        public class Structure
        {
            public int d, x, y, a;
            public StructureType t;

            public Structure()
            {
                d = 0;
                x = 0;
                y = 0;
                t = StructureType.None;
                a = 0;
            }

            public Structure(int _d, int _x, int _y, StructureType _t, int _a = -1)
            {
                d = _d;
                x = _x;
                y = _y;
                t = _t;
                a = _a;
            }
        }

        [SerializeField] Case1_PL PlayerPrefab;
        [SerializeField] GameObject RoomPrefab, StairPrefab, backGroundPrefab;
        [SerializeField] int size, depth;

        Structure[,,] map;
        int roomCount, areaNum;
        List<Structure> roomList;
        List<List<Structure>> areaList;
        List<(int x, int y)> areaCenter;

        public int Size { get { return size; } }
        public Structure[,,] Map { get { return map; } }

        void Awake()
        {
            if(size < 10) size = 10;
            if(depth < 1) depth = 1;

            roomCount = (size * size) >> 4;
            areaNum = 0;

            map = new Structure[depth, size, size];
            roomList = new();
            areaList = new();
            areaCenter = new();
        }

        void Start()
        {
            // 단일 방 생성
            for(int i = 0; i < depth; i++)
            {
                RoomSetting(i, 0, roomCount);
            }

            // 1차 방 연결
            InitialRoadSetting();

            // 2차 방 연결
            AdditialRoadSetting();

            // 계단 설정
            StairSetting();

            // 프리팹 생성
            InstantiateObjects();
        }

        void RoomSetting(int depth, int count, int limit)
        {
            Queue<(int, int, int, int)> roomStack = new();
            roomStack.Enqueue((0, size, 0, size));

            while (roomStack.Count > 0 && count < limit)
            {
                (int xLow, int xHigh, int yLow, int yHigh) = roomStack.Dequeue();

                // 생성 예정 방 위치
                int nextX = Random.Range(xLow, xHigh);
                nextX = nextX >= size ? size - 1 : nextX;
                int nextY = Random.Range(yLow, yHigh);
                nextY = nextY >= size ? size - 1 : nextY;

                // 생성 위치가 비어있다면 방 생성
                if (map[depth, nextX, nextY] == null)
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

        void InitialRoadSetting()
        {
            int initialRoomCount = roomList.Count;
            for(int i = 0; i < initialRoomCount; i++)
            {
                bool findMerge = false;
                Queue<Structure> queue = new();
                Stack<Structure> merge = new();
                Dictionary<Structure, Structure> parents = new();
                bool[,] visited = new bool[size, size];

                queue.Enqueue(roomList[i]);
                parents.Add(roomList[i], roomList[i]);
                visited[roomList[i].x, roomList[i].y] = true;

                while (queue.Count > 0)
                {
                    Structure find = queue.Dequeue();

                    if (find.t != StructureType.None && !find.Equals(roomList[i]))
                    {
                        findMerge = true;
                        while (!parents[find].Equals(find))
                        {
                            merge.Push(find);
                            find = parents[find];
                        }
                        break;
                    }

                    for (int j = -1; j <= 1; j += 2)
                    {
                        if (j < 0 ? find.x > 0 : find.x < size - 1)
                        {
                            Structure next = map[find.d , find.x + j, find.y];
                            if(next == null)
                            {
                                map[find.d, find.x + j, find.y] = new Structure(find.d, find.x + j, find.y, StructureType.None);
                                next = map[find.d, find.x + j, find.y];
                            }
                            if (!visited[next.x, next.y])
                            {
                                queue.Enqueue(next);
                                parents.Add(next, find);
                                visited[next.x, next.y] = true;
                            }
                        }
                        if (j < 0 ? find.y > 0 : find.y < size - 1)
                        {
                            Structure next = map[find.d, find.x, find.y + j];
                            if (next == null)
                            {
                                map[find.d, find.x, find.y + j] = new Structure(find.d, find.x, find.y + j, StructureType.None);
                                next = map[find.d, find.x, find.y + j];
                            }
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
                    List<Structure> area = new List<Structure>();
                    int thisAreaNum = -1;
                    while (merge.Count > 0)
                    {
                        Structure pop = merge.Pop();
                        if(pop.t == StructureType.None)
                            pop.t = StructureType.Room;
                        roomList.Add(pop);
                        area.Add(pop);
                        if (pop.a >= 0)
                            thisAreaNum = pop.a;
                    }

                    if (thisAreaNum >= 0)
                    {
                        for(int j = 0; j < area.Count; j++)
                        {
                            if (area[j].a >= 0)
                                continue;
                            area[j].a = thisAreaNum;
                            areaList[thisAreaNum].Add(area[j]);
                            areaCenter.Add((area[j].x, area[j].y));
                        }
                    }
                    else
                    {
                        for (int j = 0; j < area.Count; j++)
                        {
                            area[j].a = areaNum;
                            areaCenter.Add((area[j].x, area[j].y));
                        }
                        areaNum++;
                        areaList.Add(area);
                    }
                }
            }
        }

        void AdditialRoadSetting()
        {
            int totalXDifferenceSum = 0, totalYDifferenceSum = 0;
            float totalXDifference = 1f, totalYDifference = 1f;
            for(int i = 0; i < areaCenter.Count; i++)
            {
                totalXDifferenceSum += areaCenter[i].x;
                totalYDifferenceSum += areaCenter[i].y;
            }
            totalXDifference = 1 / totalXDifferenceSum;
            totalYDifference = 1 / totalYDifferenceSum;
            float[,] distancePortionTable = new float[areaNum, areaNum];
            for(int i = 0; i < areaNum; i++)
            {
                for(int j = i + 1; j < areaNum; j++)
                {
                    distancePortionTable[i, j] = ((areaCenter[i].x - areaCenter[j].x) >= 0 ? (areaCenter[i].x - areaCenter[j].x) : (areaCenter[j].x - areaCenter[i].x)) * totalXDifference
                                                + ((areaCenter[i].y - areaCenter[j].y) >= 0 ? (areaCenter[i].y - areaCenter[j].y) : (areaCenter[j].y - areaCenter[i].y)) * totalYDifference;
                }
            }


            for (int baseAreaIndex = 0; baseAreaIndex < areaNum; baseAreaIndex++)
            {
                float minDistance = float.MaxValue;
                int minDistanceArea = 0;
                for (int j = baseAreaIndex + 1; j < areaNum; j++)
                {
                    if(distancePortionTable[baseAreaIndex, j] < minDistance)
                    {
                        minDistance = distancePortionTable[baseAreaIndex, j];
                        minDistanceArea = j;
                    }
                }

                bool findMerge = false;
                Queue<Structure> queue = new();
                Stack<Structure> merge = new();
                Dictionary<Structure, Structure> parents = new();
                bool[,] visited = new bool[size, size];

                queue.Enqueue(areaList[baseAreaIndex][0]);
                parents.Add(areaList[baseAreaIndex][0], areaList[baseAreaIndex][0]);
                visited[areaList[baseAreaIndex][0].x, areaList[baseAreaIndex][0].y] = true;

                while (queue.Count > 0)
                {
                    Structure find = queue.Dequeue();

                    if (find.a == minDistanceArea)
                    {
                        findMerge = true;
                        while (!parents[find].Equals(find))
                        {
                            merge.Push(find);
                            find = parents[find];
                        }
                        break;
                    }

                    for (int j = -1; j <= 1; j += 2)
                    {
                        if (j < 0 ? find.x > 0 : find.x < size - 1)
                        {
                            Structure next = map[find.d, find.x + j, find.y];
                            if (next == null)
                            {
                                map[find.d, find.x + j, find.y] = new Structure(find.d, find.x + j, find.y, StructureType.None);
                                next = map[find.d, find.x + j, find.y];
                            }
                            if (!visited[next.x, next.y])
                            {
                                queue.Enqueue(next);
                                parents.Add(next, find);
                                visited[next.x, next.y] = true;
                            }
                        }
                        if (j < 0 ? find.y > 0 : find.y < size - 1)
                        {
                            Structure next = map[find.d, find.x, find.y + j];
                            if (next == null)
                            {
                                map[find.d, find.x, find.y + j] = new Structure(find.d, find.x, find.y + j, StructureType.None);
                                next = map[find.d, find.x, find.y + j];
                            }
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
                    List<Structure> area = new List<Structure>();
                    while (merge.Count > 0)
                    {
                        Structure pop = merge.Pop();
                        if(pop.t == StructureType.None)
                            pop.t = StructureType.Room;
                        roomList.Add(pop);
                        area.Add(pop);
                    }

                    for (int j = 0; j < area.Count; j++)
                    {
                        if (area[j].a >= 0)
                            continue;
                        area[j].a = baseAreaIndex;
                        areaList[baseAreaIndex].Add(area[j]);
                    }
                }
            }
        }

        void StairSetting()
        {
            for(int d = 0; d < depth - 1; d++)
            {
                bool done = false;
                do
                {
                    int x = Random.Range(0, size);
                    int y = Random.Range(0, size);
                    if (map[d, x, y] != null && map[d + 1, x, y] != null &&
                        map[d, x, y].t == StructureType.Room && map[d + 1, x, y].t == StructureType.Room)
                    {
                        map[d, x, y].t = StructureType.Stair;
                        map[d + 1, x, y].t = StructureType.Stair;
                        done = true;
                    }
                }
                while (!done);
            }
        }

        void InstantiateObjects()
        {
            for(int d = 0; d < depth; d++)
            {
                Instantiate(backGroundPrefab, new Vector3(size >> 1, size >> 1, d * -5 + 1), Quaternion.identity, transform).transform.localScale *= size * 2;
                for (int x = 0; x < size; x++)
                {
                    for(int y = 0; y < size; y++)
                    {
                        if (map[d, x, y] == null)
                            continue;
                        switch(map[d, x, y].t)
                        {
                            case StructureType.Room:
                                Instantiate(RoomPrefab, new Vector3(x, y, d * -5), Quaternion.identity, transform);
                                break;
                            case StructureType.Stair:
                                Instantiate(StairPrefab, new Vector3(x, y, d * -5), Quaternion.identity, transform);
                                break;
                        }
                    }
                }
            }

            int playerSpawn = Random.Range(0, roomList.Count);
            Case1_PL player = Instantiate(PlayerPrefab, new Vector3(roomList[playerSpawn].x, roomList[playerSpawn].y, roomList[playerSpawn].d * -5 - 1), Quaternion.identity);
            player.Initialize(this, (roomList[playerSpawn].d, roomList[playerSpawn].x, roomList[playerSpawn].y));
        }
    }

}