using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum Direction
{
    Horizontal,
    Vertical
}

[System.Serializable]
public struct ArrowSprite
{
    public char character;
    public Sprite sprite;
}

public class CrosswordGrid : MonoBehaviour
{
    public static CrosswordGrid Instance { get; private set; }

    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private LetterGridSquare letterGridSquarePrefab;
    [SerializeField] private DefinitionGridSquareFull definitionGridSquareFullPrefab;
    [SerializeField] private DefinitionGridSquareShared definitionGridSquareSharedPrefab;

    [SerializeField] public List<ArrowSprite> arrowSprites = new List<ArrowSprite>();

    private GridSquare[,] gridSquares;

    public int GridNb { get; private set; }

    public GridSquare LastClicked { get; set; }
    public Direction CurrentDirection { get; private set; }
    public LetterGridSquare CurrentlySelected { get; set; }

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        Instance = null;

        // This is necessary only in the GameManager for Input to work correctly when domain reload is disabled
        // For some reason, without this line, no input is detected at all
        InputSystem.actions.Enable();
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.Exception($"{this.GetType()} Singleton already exists in the scene");
        Instance = this;
    }

    public void Initialize(int gridNb)
    {
        this.GridNb = gridNb;
        int rowCount = YAMLLoader.Instance.Grids[gridNb].grid.Length;
        int colCount = YAMLLoader.Instance.Grids[gridNb].grid[0].Length;
        this.gridLayout.constraintCount = colCount;
        this.gridSquares = new GridSquare[rowCount, colCount];

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                if (YAMLLoader.Instance.Grids[gridNb].grid[r][c] == '#')
                {
                    Archipelamots.DefCell defcell = YAMLLoader.Instance.Grids[gridNb].defCells.First(x => x.coords.c == c && x.coords.r == r);
                    if (defcell == null)
                        throw new System.Exception($"Could not find def cell at r:{r}, c:{c}");

                    if (defcell.definitions.Length == 1)
                    {
                        DefinitionGridSquareFull gridSquare = Instantiate(this.definitionGridSquareFullPrefab, this.gridLayout.transform);
                        gridSquare.Initialize(r, c, YAMLLoader.Instance.Grids[gridNb].GetDefinition(defcell.definitions[0].word), defcell.definitions[0].arrow);
                        this.gridSquares[r, c] = gridSquare;
                    }
                    else
                    {
                        DefinitionGridSquareShared gridSquare = Instantiate(this.definitionGridSquareSharedPrefab, this.gridLayout.transform);
                        gridSquare.Initialize(r, c, 
                            YAMLLoader.Instance.Grids[gridNb].GetDefinition(defcell.definitions[0].word), defcell.definitions[0].arrow,
                            YAMLLoader.Instance.Grids[gridNb].GetDefinition(defcell.definitions[1].word), defcell.definitions[1].arrow
                        );
                        this.gridSquares[r, c] = gridSquare;
                    }
                }
                else
                {
                    LetterGridSquare gridSquare = Instantiate(this.letterGridSquarePrefab, this.gridLayout.transform);
                    gridSquare.Initialize(r, c);
                    this.gridSquares[r, c] = gridSquare;
                }
            }
        }
    }
    
    public void SwitchDirection()
    {
        if (this.CurrentDirection == Direction.Horizontal)
            this.CurrentDirection = Direction.Vertical;
        else
            this.CurrentDirection = Direction.Horizontal;
    }

    public void Select(GridSquare square, Direction? direction = null)
    {

    }
}
