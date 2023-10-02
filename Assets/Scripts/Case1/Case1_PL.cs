using UnityEngine;

namespace Case1
{
    public class Case1_PL : MonoBehaviour
    {
        [SerializeField] Case1_PG pg;
        [SerializeField] (int d, int x, int y) pos;

        public void Initialize(Case1_PG _pg, (int d, int x, int y) _pos)
        {
            pg = _pg;
            pos = _pos;
            Debug.Log(pos);
        }

        void Update()
        {
            if (pg == null)
                return;

            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (pos.x <= 0)
                    return;

                if (pg.Map[pos.d, pos.x - 1, pos.y] != null && pg.Map[pos.d, pos.x - 1, pos.y].t != StructureType.None)
                {
                    transform.Translate(-Vector2.right);
                    pos.x -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (pos.x >= pg.Size)
                    return;

                if (pg.Map[pos.d, pos.x + 1, pos.y] != null && pg.Map[pos.d, pos.x + 1, pos.y].t != StructureType.None)
                {
                    transform.Translate(Vector2.right);
                    pos.x += 1;
                }
            }
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (pos.y >= pg.Size)
                    return;

                if (pg.Map[pos.d, pos.x, pos.y + 1] != null && pg.Map[pos.d, pos.x, pos.y + 1].t != StructureType.None)
                {
                    transform.Translate(Vector2.up);
                    pos.y += 1;
                }
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (pos.y <= 0)
                    return;

                if (pg.Map[pos.d, pos.x, pos.y - 1] != null && pg.Map[pos.d, pos.x, pos.y - 1].t != StructureType.None)
                {
                    transform.Translate(-Vector2.up);
                    pos.y -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (pg.Map[pos.d, pos.x, pos.y].t == StructureType.Stair)
                {
                    if(pos.d > 0)
                    {
                        if (pg.Map[pos.d - 1, pos.x, pos.y].t == StructureType.Stair)
                        {
                            transform.Translate(Vector3.forward * 5);
                            pos.d -= 1;
                            return;
                        }
                    }
                    if(pos.d < pg.Size - 1)
                    {
                        if (pg.Map[pos.d + 1, pos.x, pos.y].t == StructureType.Stair)
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