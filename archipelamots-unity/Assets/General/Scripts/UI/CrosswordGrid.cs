using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
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

    public int GridNb { get; private set; }
    public GridSquare[,] GridSquares { get; private set; }

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

    private void Update()
    {
        if (this.CurrentlySelected == null)
            return;

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            this.CurrentlySelected.Erase();
            this.SelectPreviousLetter();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            this.CurrentlySelected.Erase();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            this.SelectNextLetter();
        }
        else if (Input.anyKeyDown)
        {
            if (Input.inputString.Length == 1)
            {
                char character = Input.inputString[0];
                if (character >= 'a' && character <= 'z')
                {
                    this.CurrentlySelected.Set(char.ToUpper(character));
                    this.SelectNextLetter();
                    this.CheckGridFinished();
                }
            }
        }
    }

    public void Initialize(int gridNb)
    {
        this.GridNb = gridNb;
        int rowCount = YAMLLoader.Instance.Grids[gridNb].grid.Length;
        int colCount = YAMLLoader.Instance.Grids[gridNb].grid[0].Length;
        this.gridLayout.constraintCount = colCount;
        this.GridSquares = new GridSquare[rowCount, colCount];
        this.gridLayout.transform.KillAllChildren();
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++)
            {
                if (YAMLLoader.Instance.Grids[gridNb].grid[r][c] == '#')
                {
                    YAML.DefCell defcell = YAMLLoader.Instance.Grids[gridNb].defCells.First(x => x.coords.c == c && x.coords.r == r);
                    if (defcell == null)
                        throw new System.Exception($"Could not find def cell at r:{r}, c:{c}");

                    if (defcell.definitions.Length == 1)
                    {
                        DefinitionGridSquareFull gridSquare = Instantiate(this.definitionGridSquareFullPrefab, this.gridLayout.transform);
                        gridSquare.Initialize(this, r, c, defcell.definitions[0]);
                        this.GridSquares[r, c] = gridSquare;
                    }
                    else
                    {
                        DefinitionGridSquareShared gridSquare = Instantiate(this.definitionGridSquareSharedPrefab, this.gridLayout.transform);
                        gridSquare.Initialize(this, r, c, defcell.definitions[0], defcell.definitions[1]);
                        this.GridSquares[r, c] = gridSquare;
                    }
                }
                else
                {
                    LetterGridSquare gridSquare = Instantiate(this.letterGridSquarePrefab, this.gridLayout.transform);
                    gridSquare.Initialize(this, r, c);
                    this.GridSquares[r, c] = gridSquare;
                }
            }
        }

        SavingUtility.LoadGridData(this);
    }
    
    public void SwitchDirection()
    {
        if (this.CurrentDirection == Direction.Horizontal)
            this.CurrentDirection = Direction.Vertical;
        else
            this.CurrentDirection = Direction.Horizontal;
    }

    public void Select(LetterGridSquare square, Direction? direction = null)
    {
        if (direction.HasValue)
        {
            this.CurrentDirection = direction.Value;
        }

        if (!this.IsValidDirection(square, this.CurrentDirection))
        {
            this.SwitchDirection();
        }

        this.SelectLetter(square, this.CurrentDirection);
    }

    public void Select(DefinitionSubSquare subSquare)
    {
        this.CurrentDirection = subSquare.Direction;
        this.SelectLetter(this.GridSquares[subSquare.StartingR, subSquare.StartingC] as LetterGridSquare, subSquare.Direction);
    }

    private void SelectNextLetter()
    {
        this.SelectAdjacentLetter(1);
    }


    private void SelectPreviousLetter()
    {
        this.SelectAdjacentLetter(-1);
    }

    private void SelectAdjacentLetter(int direction)
    {
        if (this.CurrentlySelected == null)
            throw new System.Exception("Cannot select next letter, there is none selected at the moment");

        LetterGridSquare initiallySelected = this.CurrentlySelected;

        int r = this.CurrentlySelected.R;
        int c = this.CurrentlySelected.C;

        if (this.CurrentDirection == Direction.Horizontal)
            c += direction;
        else
            r += direction;

        while (c >= 0 && c < this.GridSquares.GetLength(0) && r >= 0 && r < this.GridSquares.GetLength(1))
        {
            if (this.GridSquares[r, c] is LetterGridSquare)
            {
                if (!(this.GridSquares[r, c] as LetterGridSquare).LockedIn)
                {
                    this.SelectLetter((this.GridSquares[r, c] as LetterGridSquare), this.CurrentDirection);
                    return;
                }
            }
            else
            {
                return;
            }

            if (this.CurrentDirection == Direction.Horizontal)
                c += direction;
            else
                r += direction;
        }
    }

    private void SelectLetter(LetterGridSquare letterSquare, Direction direction)
    {
        bool isSquareLockedIn = letterSquare.LockedIn;
        this.DeselectAll();
        LetterGridSquare firstLetter = this.FindFirstLetterOfWord(letterSquare, direction);
        this.IterateLetters(firstLetter.R, firstLetter.C, direction, (LetterGridSquare square) =>
        {
            // Select the first non locked letter, or the letter that was clicked if it's not locked in
            if (!square.LockedIn && (square == letterSquare || isSquareLockedIn))
            {
                this.SelectWord(firstLetter.R, firstLetter.C, direction);
                square.MainSelect();
                this.CurrentlySelected = square;
                return false;
            }
            return true;
        });
    }

    private void SelectWord(int startR, int startC, Direction direction)
    {
        this.IterateLetters(startR, startC, direction, (LetterGridSquare square) =>
        {
            square.SecondarySelect();
            return true;
        });
    }

    private void DeselectAll()
    {
        this.CurrentlySelected = null;
        foreach (GridSquare square in this.GridSquares)
        {
            square.Deselect();
        }
    }

    public void CheckGridFinished()
    {
        // TODO
    }

    private bool IsValidDirection(LetterGridSquare square, Direction direction)
    {
        LetterGridSquare firstLetterSquare = this.FindFirstLetterOfWord(square, direction);
        int count = 0;
        this.IterateLetters(firstLetterSquare.R, firstLetterSquare.C, direction, (LetterGridSquare square) =>
        {
            count++;
            return true;
        });

        return count > 1;
    }

    private LetterGridSquare FindFirstLetterOfWord(LetterGridSquare gridSquare, Direction direction)
    {
        LetterGridSquare lastValidLetter = gridSquare;
        int r = gridSquare.R;
        int c = gridSquare.C;
        while (c >= 0 && r >= 0)
        {
            if (this.GridSquares[r, c] is LetterGridSquare)
            {
                lastValidLetter = this.GridSquares[r, c] as LetterGridSquare;
            }
            else
            {
                return lastValidLetter;
            }

            if (direction == Direction.Horizontal)
                c--;
            else
                r--;
        }

        return lastValidLetter;
    }

    private delegate bool LetterSquareHandler(LetterGridSquare gridSquare);
    private void IterateLetters(int startR, int startC, Direction direction, LetterSquareHandler function)
    {
        int r = startR;
        int c = startC;
        int length = direction == Direction.Horizontal ? this.GridSquares.GetLength(0) : this.GridSquares.GetLength(1);
        ref int coord = ref direction == Direction.Horizontal ? ref c : ref r;
        for (; coord < length; coord++)
        {
            if (this.GridSquares[r, c] is LetterGridSquare)
            {
                // If the function returns true, we continue; if false, we stop
                if (!function.Invoke(this.GridSquares[r, c] as LetterGridSquare))
                {
                    return;
                }
            }
            // If we reach another definition, our word has ended so we stop
            else
            {
                return;
            }
        }
    }
}
