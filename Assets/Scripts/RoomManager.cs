using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int minRooms = 10;

    private int _roomWidth = 20;
    private int _roomHeight = 12;

    [SerializeField] private int gridSizeX = 10;
    [SerializeField] private int gridSizeY = 10;

    [SerializeField]private List<GameObject> roomObjects = new();
    private Queue<Vector2Int> _roomQueue = new();

    private int[,] _roomGrid;
    private int _roomCount;
    private bool _generationComplete;

    private void Start()
    {
        _roomGrid = new int[gridSizeX, gridSizeY];
        _roomQueue = new Queue<Vector2Int>();

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    private void Update()
    {
        if (_roomQueue.Count > 0 && _roomCount < maxRooms && !_generationComplete)
        {
            Vector2Int roomIndex = _roomQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        else if (_roomCount < minRooms)
        {
            Debug.Log("RoomCount was less than the minimum amount of rooms. Trying again.");
            RegenerateRooms();
        }
        else if (!_generationComplete)
        {
            Debug.Log($"Generation Complete, {_roomCount} rooms created.");
            _generationComplete = true;
        }


        //Regerar as salas
        if (Input.GetKey(KeyCode.R))
        {
            RegenerateRooms();
        }
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        _roomQueue.Enqueue(roomIndex);
        int x = roomIndex.x;
        int y = roomIndex.y;
        _roomGrid[x, y] = 1;
        _roomCount++;
        var initialRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{roomIndex.ToString()}";
        initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
        roomObjects.Add(initialRoom);
    }

    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        if (_roomCount >= maxRooms) return false;
        if (Random.value < 0.5f && roomIndex != Vector2Int.zero) return false;
        // Evita deixar salas muito juntas, é possivel calibrar isso no numero magico 1 abaixo
        
        if (CountAdjacentRooms(roomIndex) > 1) return false;

        _roomQueue.Enqueue(roomIndex);
        _roomGrid[x, y] = 1;
        _roomCount++;

        var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        newRoom.name = $"Room-{roomIndex.ToString()}";
        newRoom.GetComponent<Room>().RoomIndex = roomIndex;
        roomObjects.Add(newRoom);

        OpenDoors(newRoom, x, y);

        return true;
    }

    // Zera todas as salas e tenta de novo!
    private void RegenerateRooms()
    {
        roomObjects.ForEach(Destroy);
        roomObjects.Clear();
        _roomGrid = new int[gridSizeX, gridSizeY];
        _roomQueue.Clear();
        _roomCount = 0;
        _generationComplete = false;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    void OpenDoors(GameObject room, int x, int y)
    {
        Room newRoomScript = room.GetComponent<Room>();

        //pega vizinhos
        Room leftRoomScript = GetRoomScriptAt(new Vector2(x - 1, y));
        Room rightRoomScript = GetRoomScriptAt(new Vector2(x + 1, y));
        Room topRoomScript = GetRoomScriptAt(new Vector2(x, y + 1));
        Room bottonRoomScript = GetRoomScriptAt(new Vector2(x, y - 1));

        // Determina quais portas estao abertas baseado na direcao da sala
        if (x > 0 && _roomGrid[x - 1, y] != 0)
        {
            // Sala vizinha a esquerda
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
        }

        if (x < gridSizeX - 1 && _roomGrid[x + 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
        }

        if (y > 0 && _roomGrid[x, y - 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.down);
            bottonRoomScript.OpenDoor(Vector2Int.up);
        }

        if (y < gridSizeY - 1 && _roomGrid[x, y + 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.up);
            topRoomScript.OpenDoor(Vector2Int.down);
        }
    }

    private Room GetRoomScriptAt(Vector2 index)
    {
        GameObject roomObject = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
        if (roomObject != null) return roomObject.GetComponent<Room>();
        return null;
    }

    private int CountAdjacentRooms(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;
        int count = 0;

        if (x > 0 && _roomGrid[x - 1, y] != 0) count++; // Left
        if (x < gridSizeX - 1 && _roomGrid[x + 1, y] != 0) count++; // Right
        if (y > 0 && _roomGrid[x, y - 1] != 0) count++; // Bellow
        if (x < gridSizeY - 1 && _roomGrid[x - 1, y + 1] != 0) count++; // Up Neighbour

        return count;
    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        return new Vector3(_roomWidth * (gridX - gridSizeX / 2),
            _roomHeight * (gridY - gridSizeY / 2)
        );
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        Gizmos.color = gizmoColor;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                Gizmos.DrawWireCube(position, new Vector3(_roomWidth, _roomHeight, 1));
            }
        }
    }
}