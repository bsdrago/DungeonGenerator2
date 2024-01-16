using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int minRooms = 10;

    private int roomWidth = 20;
    private int roomHeight = 12;

    [SerializeField] private int gridSizeX = 10;
    [SerializeField] private int gridSizeY = 10;

    [SerializeField]private List<GameObject> roomObjects = new();
    [SerializeField]private Queue<Vector2Int> roomQueue = new();

    private int[,] roomGrid;
    private int roomCount;
    private bool generationComplete;

    private void Start()
    {
        roomGrid = new int[gridSizeX, gridSizeY];
        roomQueue = new Queue<Vector2Int>();

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    private void Update()
    {
        if (roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
        {
            Vector2Int roomIndex = roomQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        else if (roomCount < minRooms)
        {
            Debug.Log("RoomCount was less than the minimum amount of rooms. Trying again.");
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            Debug.Log($"Generation Complete, {roomCount} rooms created.");
            generationComplete = true;
        }


        //Regerar as salas
        if (Input.GetKey(KeyCode.R))
        {
            RegenerateRooms();
        }
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        roomQueue.Enqueue(roomIndex);
        int x = roomIndex.x;
        int y = roomIndex.y;
        roomGrid[x, y] = 1;
        roomCount++;
        var initialRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{roomIndex.ToString()}";
        initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
        roomObjects.Add(initialRoom);
    }

    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        if (roomCount >= maxRooms) return false;
        if (Random.value < 0.5f && roomIndex != Vector2Int.zero) return false;
        // Evita deixar salas muito juntas, é possivel calibrar isso no numero magico 1 abaixo
        if (CountAdjacentRooms(roomIndex) > 1) return false;

        roomQueue.Enqueue(roomIndex);
        roomGrid[x, y] = 1;
        roomCount++;

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
        roomGrid = new int[gridSizeX, gridSizeY];
        roomQueue.Clear();
        roomCount = 0;
        generationComplete = false;

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
        if (x > 0 && roomGrid[x - 1, y] != 0)
        {
            // Sala vizinha a esquerda
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
        }

        if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
        }

        if (y > 0 && roomGrid[x, y - 1] != 0)
        {
            newRoomScript.OpenDoor(Vector2Int.down);
            bottonRoomScript.OpenDoor(Vector2Int.up);
        }

        if (y < gridSizeY - 1 && roomGrid[x, y + 1] != 0)
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

        if (x > 0 && roomGrid[x - 1, y] != 0) count++; // Left
        if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0) count++; // Right
        if (y > 0 && roomGrid[x, y - 1] != 0) count++; // Bellow
        if (x < gridSizeY - 1 && roomGrid[x - 1, y + 1] != 0) count++; // Up Neighbour

        return count;
    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        return new Vector3(roomWidth * (gridX - gridSizeX / 2),
            roomHeight * (gridY - gridSizeY / 2)
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
                Gizmos.DrawWireCube(position, new Vector3(roomWidth, roomHeight, 1));
            }
        }
    }
}