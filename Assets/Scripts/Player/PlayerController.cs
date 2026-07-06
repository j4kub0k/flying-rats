using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    public float speed = 10.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -25f;
    public float mouseSensitivity = 2.0f;
    public float reachDistance = 5.0f;
    float timeToMine = 0.0f;
    bool hasTargetBlock = false;
    Vector3Int currentMineTarget;
    CharacterController controller;
    public Inventory inventory;
    Camera playerCamera;
    public World world;



    Controls controls;
    Vector2 moveInput;
    Vector2 lookInput;
    bool jumpPressed;
    bool destroyIsHeld;

    GameObject buildPreview;
    public Material wireframeMaterial;   




    float verticalVelocity;
    float cameraPitch = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new Controls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;
        
        controls.Player.Mine.performed += ctx => destroyIsHeld = true;
        controls.Player.Mine.canceled += ctx => destroyIsHeld = false;

        controls.Player.Build.performed += ctx => inventory.GetSelectedItem()?.Use();
        controls.Player.SelectItem.performed += ctx => inventory.SelectSlot((int)ctx.ReadValue<float>() - 1);
        controls.Player.ScrollItem.performed += ctx =>
        {
            float v = ctx.ReadValue<float>();
            if (v != 0) inventory.Scroll(v > 0 ? 1 : -1);
        };

        controls.Player.Save.performed += ctx => world.SaveWorldData();


    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Start()
    {
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GetComponent<Inventory>();
        CreateBuildPreview();
    }
    // Update is called once per frame
    void Update()
    {
        Movement();
        Look();
        Mine();
        UpdateBuildPreview();
    }


    void Movement() { 
    
        Vector3 dir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f; 
            if (jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpPressed = false;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        Vector3 move= dir * speed + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }
    void Look()
    {
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }




    void Mine()
    {
        if (!destroyIsHeld)
        {
            timeToMine = 0.0f;
            hasTargetBlock = false;
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            timeToMine = 0.0f;
            hasTargetBlock = false;
            return;
        }

        Vector3 targetBlockPos = hit.point - hit.normal * 0.5f;
        Vector3Int blockPos = Vector3Int.FloorToInt(targetBlockPos);

        if (!hasTargetBlock || currentMineTarget != blockPos)
        {
            hasTargetBlock = true;
            currentMineTarget = blockPos;
            timeToMine = 0.0f;
        }

        if (blockPos.y < WorldSettings.MinMineableHeight)
        {
            timeToMine = 0.0f;
            return;
        }

        ChunkGenerator chunkGenerator = hit.collider.GetComponentInParent<ChunkGenerator>();
        if (chunkGenerator == null) return;

        Chunk chunk = chunkGenerator.chunkData;
        if (chunk == null) return;

        Vector3Int localPos = WorldSettings.WorldToLocalCoord(blockPos);
        BlockType blockType = chunk.GetBlock(localPos);

        // Unbreakable(Air) == true, takze toto vylucuje aj mierenie do prazdna
        if (BlockTypeHelper.Unbreakable(blockType))
        {
            timeToMine = 0.0f;
            return;
        }

        timeToMine += Time.deltaTime;

        if (timeToMine >= BlockTypeHelper.TimeToDestroy(blockType))
        {
            chunk.SetBlock(localPos, BlockType.Air);
            chunk.IsModified = true;
            chunkGenerator.GenerateMesh(chunk, chunkGenerator.GetComponent<MeshRenderer>().sharedMaterial);
            timeToMine = 0.0f;
            hasTargetBlock = false;
            BlockItem blockItem = new BlockItem(this, blockType);
            blockItem.AddToInventory();
        }
    }

    public bool Build(BlockType selectedBlockType)
    {
        if (!TryGetBuildTarget(out BuildTarget target))
            return false;

        target.chunk.SetBlock(target.localPos, selectedBlockType);
        target.chunk.IsModified = true;
        world.RebuildChunk(target.chunkCoord);
        return true;
    }

    struct BuildTarget
    {
        public Vector3Int blockPos;
        public Vector3Int chunkCoord;
        public Vector3Int localPos;
        public Chunk chunk;
    }

    bool TryGetBuildTarget(out BuildTarget target)
    {
        target = default;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, reachDistance)) return false;

        Vector3Int blockPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);
        if (blockPos.y >= WorldSettings.MaxBuildHeight) return false;

        Vector3Int chunkCoord = WorldSettings.WorldToChunkCoord(blockPos);
        Chunk chunk = world.GetChunk(chunkCoord);
        if (chunk == null) return false;

        Vector3Int localPos = WorldSettings.WorldToLocalCoord(blockPos);
        if (chunk.GetBlock(localPos) != BlockType.Air) return false;

        Bounds blockBounds = new Bounds((Vector3)blockPos + Vector3.one * 0.5f, Vector3.one * 0.98f);
        if (blockBounds.Intersects(controller.bounds)) return false;

        target = new BuildTarget { blockPos = blockPos, chunkCoord = chunkCoord, localPos = localPos, chunk = chunk };
        return true;
    }


    Mesh BuildWireframeCubeMesh()
    {
        Vector3[] baseVerts = {
        new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1),
        new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,1,1),
    };
        int[] baseEdges = {
        0,1, 1,2, 2,3, 3,0,
        4,5, 5,6, 6,7, 7,4,
        0,4, 1,5, 2,6, 3,7,
    };

        float d = 0.005f;
        Vector3[] offsets = {
        Vector3.zero,
        new Vector3(d, d, d),
        new Vector3(-d, -d, -d),
    };

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        foreach (Vector3 off in offsets)
        {
            int start = verts.Count;
            foreach (Vector3 v in baseVerts) verts.Add(v + off);
            foreach (int i in baseEdges) indices.Add(start + i);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        return mesh;
    }


    void CreateBuildPreview()
    {
        buildPreview = new GameObject("BuildPreview");
        buildPreview.AddComponent<MeshFilter>().mesh = BuildWireframeCubeMesh();

        buildPreview.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;


        buildPreview.SetActive(false);
    }

    void UpdateBuildPreview()
    {

        if(destroyIsHeld) 
        {
            buildPreview.SetActive(false);
            return;
        }

        if(inventory.GetSelectedItem() is  not BlockItem)
        {
            buildPreview.SetActive(false);
            return;
        }
        if (TryGetBuildTarget(out BuildTarget target))
        {
            buildPreview.transform.position = target.blockPos;
            buildPreview.SetActive(true);
        }
        else
        {
            buildPreview.SetActive(false);
        }
    }
}
