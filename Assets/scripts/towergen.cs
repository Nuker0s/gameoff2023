using System.CodeDom.Compiler;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;



public class towergen : MonoBehaviour
{
    public GameObject roomgenprefab;
    public RoomGenerator roomgenopt;
    public Transform player;
    public static Vector3Int[] directions = new Vector3Int[] {Vector3Int.up,/*Vector3Int.down,*/Vector3Int.left,Vector3Int.right,Vector3Int.forward,Vector3Int.back};
    public GameObject cubePrefab;
    public List<roomdata> rooms = new List<roomdata>();//list of all rooms
    public int height = 10;
    public float cubeSize = 1f;
    public float sidewaysChance = 50;
    private Vector3Int lastdir = -directions[0];
    private roomdata lastroom = new roomdata(new Vector3Int(0, 0, 0));
    public roompack roompack;
    public bool delete = false;
    public int roomstoplace;
    public bool regenerate;
    public GameObject lvl0;
    public GameObject lastgenlevel;
    public LayerMask holelayer;

    void Start()
    {

        if (roomgenopt == null)
        {
            roomgenopt = roomgenprefab.GetComponent<RoomGenerator>();
        }
        for (int i = 0; i < 3; i++)
        {
            roomplacer(1, sidewaysChance);
            
        }
        generaterooms();
        Instantiate(lvl0,transform.GetChild(0).position,Quaternion.identity, transform);
        player.position=transform.GetChild(0).position+Vector3.up*3;
        

    }
    private void Update()
    {
        Transform curroom = transform.GetChild(0);
        foreach (Transform room in transform)
        {
            float roomsize = roomgenopt.size * roomgenopt.quadsize;
            Bounds roombound = new Bounds(room.position,new Vector3(roomsize,roomsize,roomsize));
            
            if (roombound.Contains(player.position))
            {
                curroom=room; break;
            }
        }
        if (curroom.GetSiblingIndex() != 0)
        {
            Debug.Log(curroom.GetSiblingIndex());
            
        }

        for (int i = 0; i < curroom.GetSiblingIndex() - 3; i++)
        {
            roomplacer(1, sidewaysChance);
            generaterooms();
            deleteroom();
        }

        if (regenerate)
        {
            //rooms.Clear();
            regenerate = false;
            roomplacer(roomstoplace, sidewaysChance);
            generaterooms();

        }
        if (delete)
        {
            delete = false;
            deleteroom();

        }

    }
    public void generaterooms() 
    {
        


        for (int o = 0; o < rooms.Count - 1; o++)
        {
            roomdata room = rooms[o];

            if (!room.Generated)
            {

                if (true)
                {

                    if (Vector3.Dot(room.lastroom - room.room, room.room - room.nextroom) == 0)
                    {
                        //print("corner " + room.room);
                        RoomGenerator romgen = Instantiate(roomgenprefab, room.room, Quaternion.identity, transform).GetComponent<RoomGenerator>();
                        romgen.transform.position = romgen.transform.position * romgen.size * romgen.quadsize;

                        romgen.kolanogen();
                        //Debug.Log(room.nextroom - room.room);
                        for (int i = 0; i < romgen.transform.childCount; i++)
                        {
                            if (romgen.transform.GetChild(i).forward == room.nextroom - room.room)
                            {
                                Destroy(romgen.transform.GetChild(i).gameObject);
                            }
                            if (romgen.transform.GetChild(i).forward == room.lastroom - room.room)
                            {
                                Destroy(romgen.transform.GetChild(i).gameObject);
                            }
                        }
                        randomholegen(romgen.transform.position, 8);
                        placemainfeatures(room, romgen.transform);
                        room.Generated = true;

                    }
                    else
                    {


                        RoomGenerator romgen = Instantiate(roomgenprefab, room.room, Quaternion.identity, transform).GetComponent<RoomGenerator>();
                        romgen.transform.position = romgen.transform.position * romgen.size * romgen.quadsize;

                        romgen.roomgen();
                        //Debug.Log(room.nextroom - room.room);

                        romgen.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(room.nextroom - room.room).eulerAngles - new Vector3(90, 0, 0));
                        randomholegen(romgen.transform.position, 8);
                        
                        placemainfeatures(room, romgen.transform);
                        room.Generated = true;
                    }
                }
            }
        }
    }
    public void placemainfeatures(roomdata room,Transform roomgen) 
    {
        GameObject currentlevel=roomgen.gameObject;
        if (!(room.lastroom == new Vector3Int(0,0,0)))
        {

            if ((room.room - room.lastroom).y == 0 && (room.nextroom - room.room)!= Vector3Int.up)
            {

                GameObject toplace = roompack.hrooms[Random.Range((int)0, roompack.hrooms.Length)];
                currentlevel = Instantiate(toplace, roomgen.position, Quaternion.LookRotation(room.nextroom - room.room), roomgen);

            }
            else if ((room.lastroom - room.room).y == 0)
            {


                GameObject toplace = roompack.vrooms[Random.Range((int)0, roompack.vrooms.Length)];
                currentlevel = Instantiate(toplace, roomgen.position, Quaternion.identity,roomgen);
                
            }
            else if (true)
            {
                GameObject toplace = roompack.deepvrooms[Random.Range((int)0, roompack.deepvrooms.Length)];
                currentlevel = Instantiate(toplace, roomgen.position, Quaternion.identity, roomgen);
            }
        }
        /*if (lastgenlevel != null)
        {
            if ((currentlevel.transform.position-lastgenlevel.transform.position).y == 0)
            {
                OffMeshLink link = currentlevel.AddComponent<OffMeshLink>();
                
                link.startTransform = lastgenlevel.transform;
                link.endTransform = currentlevel.transform;
                
                link.biDirectional = true;
                link.UpdatePositions();
                link.autoUpdatePositions = true;

                OffMeshLink link2 = lastgenlevel.AddComponent<OffMeshLink>();

                link2.startTransform = currentlevel.transform;
                link2.endTransform = lastgenlevel.transform;

                link2.biDirectional = true;
                link2.UpdatePositions();
                link2.autoUpdatePositions = true;
                
                lastgenlevel = currentlevel;
            }
        }
        else
        {
            lastgenlevel = currentlevel;
        }*/
    }
    public void deleteroom() 
    {
        rooms.RemoveAt(0);
        Destroy(transform.GetChild(0).gameObject);
    }
    public void roomplacer(int number,float sidewayschance) 
    {
        for (int i = 0; i < number; i++)
        {
            Vector3Int nextdir;
            Vector3Int newroompos;
            while (true)
            {
                if (Random.Range(0, 100) < sidewayschance)
                {
                    nextdir = randirex(-lastdir);
                }
                else
                {
                    nextdir = lastdir;
                }


                newroompos = nextdir + lastroom.room;
                if (IsPositionTaken(newroompos))
                {

                }
                else
                {
                    break;
                }
            }
            roomdata roomdata = new roomdata(newroompos);

            lastdir = nextdir;
            roomdata.lastroom = lastroom.room;
            lastroom.nextroom = roomdata.room;
            lastroom = roomdata;
            rooms.Add(roomdata);
        }
    }
    private void OnDrawGizmos()
    {
        
        if (rooms.Count > 1)
        {
            for (int i = 0; i < rooms.Count - 4; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rooms[i].room, rooms[i + 1].room);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(rooms[i].room, 0.1f);
            }
        }
        
    }
    public class roomdata
    {
        public Vector3Int lastroom;
        public Vector3Int room;
        public Vector3Int nextroom;
        public bool Generated = false;
        public roomdata() { }
        public roomdata(Vector3Int newroom)
        {
            room = newroom;
        }
        public roomdata(Vector3Int newroom,Vector3Int prevroom) 
        {
            lastroom = prevroom;
            room = newroom;
        }
        public roomdata( Vector3Int newroom, Vector3Int prevroom, Vector3Int newnewroom)
        {
            lastroom = prevroom;
            room = newroom;
            nextroom = newnewroom;
        }


    }
    public static Vector3Int randirex(Vector3Int exclude)
    {
        List<Vector3Int> tempDirections = new List<Vector3Int>(directions);
        /*if (exclude != -directions[0])
        {
            tempDirections.Remove(exclude);
        }*/
        

        return tempDirections[Random.Range(0, tempDirections.Count)];
    }
    public bool IsPositionTaken(Vector3Int position)
    {
        foreach (var room in rooms)
        {
            if (room.room == position)
            {
                return true;
            }
        }
        return false;
    }
    public void randomholegen(Vector3 center,int shots) 
    {
        for (int i = 0; i < shots; i++)
        {
            Vector3 randdir = new Vector3(Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            
            if (Physics.Raycast(center, randdir, out RaycastHit hit, 48, holelayer))
            {
                Debug.Log("rayhit");
                RaycastHit[] hits = Physics.SphereCastAll(hit.point, 4.5f, -hit.normal, 100, holelayer);
                foreach (RaycastHit item in hits)
                {
                    Debug.Log("ballhit");
                    Destroy(item.collider.gameObject);
                    if (item.collider.gameObject.layer == holelayer)
                    {
                        
                        
                    }
                    
                }
            }
        }
    }

}
