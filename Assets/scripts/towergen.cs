using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UIElements;

public class towergen : MonoBehaviour
{
    public GameObject roomgenprefab;

    public static Vector3Int[] directions = new Vector3Int[] {Vector3Int.up,/*Vector3Int.down,*/Vector3Int.left,Vector3Int.right,Vector3Int.forward,Vector3Int.back};
    public GameObject cubePrefab;
    public List<roomdata> rooms = new List<roomdata>();//list of all rooms
    public int height = 10;
    public float cubeSize = 1f;
    public float sidewaysChance = 50;
    private Vector3Int lastdir = -directions[0];
    private roomdata lastroom = new roomdata(new Vector3Int(0, 0, 0));
    public bool delete=false;

    public bool regenerate;
    void Start()
    {
        regenerate = true;
    }
    private void Update()
    {
        if (delete)
        {
            delete = false;
            rooms.RemoveAt(0);
            Destroy(transform.GetChild(0).gameObject);
        }
        if (regenerate)
        {
            //rooms.Clear();
            regenerate = false;

                roomplacer(5, sidewaysChance);
           

            for (int o = 0; o < rooms.Count-1; o++)
            {
                roomdata room = rooms[o];

                if (!room.Generated)
                {

                    if (true)
                    {

                        if (Vector3.Dot(room.lastroom - room.room, room.room - room.nextroom) == 0)
                        {
                            print("corner " + room.room);
                            RoomGenerator romgen = Instantiate(roomgenprefab, room.room, Quaternion.identity, transform).GetComponent<RoomGenerator>();
                            romgen.transform.position = romgen.transform.position * romgen.size * romgen.quadsize;

                            romgen.kolanogen();
                            Debug.Log(room.nextroom - room.room);
                            for (int i = 0; i < romgen.transform.childCount; i++)
                            {
                                if (romgen.transform.GetChild(i).forward == room.nextroom-room.room)
                                {
                                    Destroy(romgen.transform.GetChild(i).gameObject);
                                }
                                if (romgen.transform.GetChild(i).forward == room.lastroom - room.room)
                                {
                                    Destroy(romgen.transform.GetChild(i).gameObject);
                                }
                            }
                            room.Generated = true;
                            
                        }
                        else
                        {


                            RoomGenerator romgen = Instantiate(roomgenprefab, room.room, Quaternion.identity, transform).GetComponent<RoomGenerator>();
                            romgen.transform.position = romgen.transform.position * romgen.size * romgen.quadsize;

                            romgen.roomgen();
                            Debug.Log(room.nextroom - room.room);

                            romgen.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(room.nextroom - room.room).eulerAngles - new Vector3(90, 0, 0));
                            room.Generated = true;
                        }
                    }
                }
            }

        }
        for (int i = 1; i < transform.childCount - 1; i++)
        {
            if (transform.GetChild(i).name == "kolano")
            {



            }

        }
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
            for (int i = 0; i < rooms.Count - 1; i++)
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

}
