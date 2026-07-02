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
    public BlockType selectedBlockType = BlockType.Green;
    CharacterController controller;
    Camera playerCamera;
    public World world;



    Controls controls;
    Vector2 moveInput;
    Vector2 lookInput;
    bool jumpPressed;
    bool destroyIsHeld;
    


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

        controls.Player.Build.performed += ctx => Build();


    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Start()
    {
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void Update()
    {
        Movement();
        Look();
        Mine();
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
            chunkGenerator.GenerateMesh(chunk, chunkGenerator.GetComponent<MeshRenderer>().sharedMaterial);
            timeToMine = 0.0f;
            hasTargetBlock = false;
        }
    }

    void Build()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            return;
        }

        Vector3 targetPos = hit.point + hit.normal * 0.5f;
        Vector3Int blockPos = Vector3Int.FloorToInt(targetPos);

        if (blockPos.y >= WorldSettings.MaxBuildHeight)
        {
            return;
        }

        Vector3Int chunkCoord = WorldSettings.WorldToChunkCoord(blockPos);
        Vector3Int localPos = WorldSettings.WorldToLocalCoord(blockPos);

        Chunk chunk = world.GetChunk(chunkCoord);
        if (chunk == null) return; // mimo vygenerovaneho sveta

        if (chunk.GetBlock(localPos) != BlockType.Air) return; // bunka uz obsadena

        chunk.SetBlock(localPos, selectedBlockType);
        world.RebuildChunk(chunkCoord);
    }
}
