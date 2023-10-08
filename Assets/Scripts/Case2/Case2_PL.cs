using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Case2
{
    public class Case2_PL : MonoBehaviour
    {
        Case2_PG pg;
        (int d, int x, int y) pos;

        public void Initialize(Case2_PG _pg, (int d, int x, int y) _pos)
        {
            pg = _pg;
            pos = _pos;
        }

        void Update()
        {
            if (pg == null)
                return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (pos.x <= 0)
                    return;
                if (pg.Map[pos.d, pos.x - 1, pos.y] != null && pg.Map[pos.d, pos.x - 1, pos.y].Type != StructureType.None)
                {
                    transform.Translate(-Vector2.right);
                    pos.x -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (pos.x >= pg.Size)
                    return;
                if (pg.Map[pos.d, pos.x + 1, pos.y] != null && pg.Map[pos.d, pos.x + 1, pos.y].Type != StructureType.None)
                {
                    transform.Translate(Vector2.right);
                    pos.x += 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (pos.y >= pg.Size)
                    return;
                if (pg.Map[pos.d, pos.x, pos.y + 1] != null && pg.Map[pos.d, pos.x, pos.y + 1].Type != StructureType.None)
                {
                    transform.Translate(Vector2.up);
                    pos.y += 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (pos.y <= 0)
                    return;

                if (pg.Map[pos.d, pos.x, pos.y - 1] != null && pg.Map[pos.d, pos.x, pos.y - 1].Type != StructureType.None)
                {
                    transform.Translate(-Vector2.up);
                    pos.y -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (pg.Map[pos.d, pos.x, pos.y].Type == StructureType.Stair)
                {
                    if (pos.d > 0)
                    {
                        if (pg.Map[pos.d - 1, pos.x, pos.y].Type == StructureType.Stair)
                        {
                            transform.Translate(Vector3.forward * 5);
                            pos.d -= 1;
                            return;
                        }
                    }
                    if (pos.d < pg.Size - 1)
                    {
                        if (pg.Map[pos.d + 1, pos.x, pos.y].Type == StructureType.Stair)
                        {
                            transform.Translate(-Vector3.forward * 5);
                            pos.d += 1;
                            return;
                        }
                    }
                }
            }
        }
    }

}