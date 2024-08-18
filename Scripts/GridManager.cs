using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 6;
    public int columns = 5;
    public float blockSize = 2.0f;
    public GameObject blockPrefab;
    public GameObject board;
    public List<Block> selectedBlocks = new List<Block>();

    public List<Color> blockColors = new List<Color>
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan
    };

    private UnityEngine.Vector3 boardSize;
    private GameObject[,] gridArray;  //two dimensional array

    //create a haspmap of row column index and position of the block
    private Dictionary<Vector2Int, Block> blockPositions = new Dictionary<Vector2Int, Block>();

    void Start()
    {
        Renderer boardRenderer = board.GetComponent<Renderer>();

        // Assign a new color to the board
        boardRenderer.material.color = Color.white;
        gridArray = new GameObject[rows, columns];
        CreateGrid();
    }


    void Update()
    {

    }
    void RefillGrid()
    {
        UnityEngine.Vector3 startPos = board.transform.position - boardSize / 2;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (blockPositions[new Vector2Int(row, col)] == null)
                {
                    UnityEngine.Vector3 position = new UnityEngine.Vector3(startPos.x + 1 + col * blockSize, startPos.y + 1 + row * blockSize, startPos.z - 0.01f);
                    GameObject newBlock = Instantiate(blockPrefab, position, UnityEngine.Quaternion.identity);
                    newBlock.transform.Rotate(-90, 0, 0);
                    newBlock.transform.SetParent(transform);

                    // Assign a random color to the new block
                    Color selectedColor = blockColors[Random.Range(0, blockColors.Count)];
                    newBlock.GetComponent<Renderer>().material.color = selectedColor;

                    // Add the new block to the gridArray and blockPositions
                    gridArray[row, col] = newBlock;
                    blockPositions[new Vector2Int(row, col)] = newBlock.GetComponent<Block>();

                    // Ensure the Block component is properly initialized
                    Block blockScript = newBlock.GetComponent<Block>();
                    if (blockScript != null)
                    {
                        blockScript.isSelected = false;
                        blockScript.isMatched = false;
                        // Any additional initialization needed for the block
                    }

                    Debug.Log("Refilled grid with a new block at position: " + row + ", " + col);
                }
            }
        }
    }

    void CreateGrid()
    {
        boardSize = board.GetComponent<Renderer>().bounds.size;
        UnityEngine.Vector3 startPos = board.transform.position - boardSize / 2;

        // First, populate the grid with random colors
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                UnityEngine.Vector3 blockPos = new UnityEngine.Vector3(startPos.x + 1 + j * blockSize, startPos.y + 1 + i * blockSize, startPos.z - 0.01f);
                GameObject block = Instantiate(blockPrefab, blockPos, UnityEngine.Quaternion.identity);
                block.transform.Rotate(-90, 0, 0);
                block.transform.SetParent(transform);

                Color selectedColor = blockColors[Random.Range(0, blockColors.Count)];
                block.GetComponent<Renderer>().material.color = selectedColor;

                gridArray[i, j] = block;
                blockPositions[new Vector2Int(i, j)] = block.GetComponent<Block>();
            }
        }

        // Now, remove matches
        while (RemoveMatches()) { }
    }

    bool RemoveMatches()
    {
        bool matchFound = false;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (IsPartOfMatch(i, j))
                {
                    // Change the color of this block
                    Color newColor;
                    do
                    {
                        newColor = blockColors[Random.Range(0, blockColors.Count)];
                    } while (newColor == gridArray[i, j].GetComponent<Renderer>().material.color);

                    gridArray[i, j].GetComponent<Renderer>().material.color = newColor;
                    matchFound = true;
                }
            }
        }
        return matchFound;
    }


    bool IsPartOfMatch(int row, int col)
    {
        Color currentColor = gridArray[row, col].GetComponent<Renderer>().material.color;

        // Check horizontal match
        if (col >= 2 &&
            gridArray[row, col - 1] != null &&
            gridArray[row, col - 2] != null &&
            gridArray[row, col - 1].GetComponent<Renderer>().material.color == currentColor &&
            gridArray[row, col - 2].GetComponent<Renderer>().material.color == currentColor)
        {
            return true;
        }

        // Check vertical match
        if (row >= 2 &&
            gridArray[row - 1, col] != null &&
            gridArray[row - 2, col] != null &&
            gridArray[row - 1, col].GetComponent<Renderer>().material.color == currentColor &&
            gridArray[row - 2, col].GetComponent<Renderer>().material.color == currentColor)
        {
            return true;
        }

        return false;
    }
    public void BlockSelected(Block block)
    {
        Debug.Log("Block selected in GridManager is called");
        if (selectedBlocks.Contains(block))
        {
            selectedBlocks.Remove(block);
            Debug.Log("Block deselected");
            block.Deselect();
        }
        else
        {
            selectedBlocks.Add(block);
            if (selectedBlocks.Count == 2)
            {
                Debug.Log("Two blocks selected");
                if (AreAdjacent(selectedBlocks[0], selectedBlocks[1]))
                {
                    StartCoroutine(SwapAndCheck(selectedBlocks[0], selectedBlocks[1]));
                }
                else
                {
                    Debug.Log("not adjacent");
                    DeselectBlocks();
                }
            }
        }
    }

    void DeselectBlocks()
    {
        foreach (Block block in selectedBlocks)
        {
            if (block != null && block.gameObject != null)
            {
                block.isSelected = false;
                block.Deselect();
            }
        }
        selectedBlocks.Clear();
    }

    bool AreAdjacent(Block block1, Block block2)
    {
        Vector2Int pos1 = GetBlockPosition(block1);
        Vector2Int pos2 = GetBlockPosition(block2);
        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) ||
               (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);
    }

    IEnumerator SwapAndCheck(Block block1, Block block2)
    {
        //wait a bit
        yield return new WaitForSeconds(1f);
        SwapBlocks(block1, block2);
        yield return new WaitForSeconds(1f); // Wait for the swap animation

        if (CheckForMatches())
        {
            BlastMatches();
            yield return new WaitForSeconds(0.5f); // Wait for the blast animation
            RefillGrid();
        }
        else
        {
            Debug.Log("No matches found. Swapping back.");
            SwapBlocks(block1, block2); // Swap back if no matches
        }

        DeselectBlocks();
    }

    void SwapBlocks(Block block1, Block block2)
    {
        Vector2Int pos1 = GetBlockPosition(block1);
        Vector2Int pos2 = GetBlockPosition(block2);

        gridArray[pos1.x, pos1.y] = block2.gameObject;
        gridArray[pos2.x, pos2.y] = block1.gameObject;

        blockPositions[pos1] = block2;
        blockPositions[pos2] = block1;

        UnityEngine.Vector3 tempPos = block1.transform.position;
        block1.transform.position = block2.transform.position;
        block2.transform.position = tempPos;

        block1.Deselect();
        block2.Deselect();

    }
    bool CheckForMatches()
    {
        HashSet<Block> matchedBlocks = new HashSet<Block>();

        // Check for horizontal matches
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++)
            {
                Block currentBlock = blockPositions[new Vector2Int(row, col)];
                Block nextBlock1 = blockPositions[new Vector2Int(row, col + 1)];
                Block nextBlock2 = blockPositions[new Vector2Int(row, col + 2)];

                if (currentBlock != null && nextBlock1 != null && nextBlock2 != null &&
                    currentBlock.GetComponent<Renderer>().material.color == nextBlock1.GetComponent<Renderer>().material.color &&
                    currentBlock.GetComponent<Renderer>().material.color == nextBlock2.GetComponent<Renderer>().material.color)
                {
                    Debug.Log("Horizontal match found");
                    matchedBlocks.Add(currentBlock);
                    matchedBlocks.Add(nextBlock1);
                    matchedBlocks.Add(nextBlock2);
                }
            }
        }

        // Check for vertical matches
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                Block currentBlock = blockPositions[new Vector2Int(row, col)];
                Block nextBlock1 = blockPositions[new Vector2Int(row + 1, col)];
                Block nextBlock2 = blockPositions[new Vector2Int(row + 2, col)];

                if (currentBlock != null && nextBlock1 != null && nextBlock2 != null &&
                    currentBlock.GetComponent<Renderer>().material.color == nextBlock1.GetComponent<Renderer>().material.color &&
                    currentBlock.GetComponent<Renderer>().material.color == nextBlock2.GetComponent<Renderer>().material.color)
                {
                    Debug.Log("Vertical match found");
                    matchedBlocks.Add(currentBlock);
                    matchedBlocks.Add(nextBlock1);
                    matchedBlocks.Add(nextBlock2);
                }
            }
        }

        // If there are matched blocks, return true
        if (matchedBlocks.Count > 0)
        {
            // Store matched blocks for blasting
            foreach (Block block in matchedBlocks)
            {
                block.isMatched = true; // Assuming Block class has an isMatched property
            }
            return true;
        }

        return false;
    }

    void BlastMatches()
    {
        Debug.Log("Blasting matches");
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2Int position = new Vector2Int(row, col);
                Block block = blockPositions[position];
                if (block != null && block.isMatched)
                {
                    Destroy(block.gameObject);
                    blockPositions[position] = null;
                }
            }
        }
    }






    Vector2Int GetBlockPosition(Block block)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridArray[row, col] != null && gridArray[row, col].GetComponent<Block>() == block)
                {
                    return new Vector2Int(row, col);
                }
            }
        }
        return Vector2Int.zero;
    }



}


