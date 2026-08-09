using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public static CrosswordGrid Current { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private LetterGridSquare letterGridSquarePrefab;
    [SerializeField] private DefinitionGridSquareFull definitionGridSquareFullPrefab;
    [SerializeField] private DefinitionGridSquareShared definitionGridSquareSharedPrefab;

    public int GridNb { get; private set; }
    public GridSquare[,] GridSquares { get; private set; }

    public GridSquare LastClicked { get; set; }
    public Direction CurrentDirection { get; private set; }
    public LetterGridSquare CurrentlySelected { get; set; }

    public bool Initialized { get; private set; }

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        Current = null;
    }

    protected virtual void Awake()
    {
        if (Current != null)
            throw new System.Exception($"{this.GetType()} Singleton already exists in the scene");
        Current = this;
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

    public virtual void Initialize(int gridNb)
    {
        this.Initialized = true;
        this.gameObject.SetActive(true);
        this.GridNb = gridNb;
        this.nameText.text = $"Grille n°{gridNb + 1}";
        int rowCount = YAMLLoader.Instance.Grids[gridNb].grid.Length;
        int colCount = YAMLLoader.Instance.Grids[gridNb].grid[0].Length;
        this.gridLayout.constraintCount = colCount;
        this.GridSquares = new GridSquare[rowCount, colCount];
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
        UI.Instance.UpdatePowerUI();
    }

    public void Reinitialize()
    {
        foreach (GridSquare square in this.GridSquares)
        {
            Destroy(square.gameObject);
        }
        this.Initialize(this.GridNb);

        if (this.CurrentlySelected != null)
        {
            this.Select(this.CurrentlySelected);
        }
    }

    public void Reinitialize(int gridNb)
    {
        foreach (GridSquare square in this.GridSquares)
        {
            Destroy(square.gameObject);
        }
        this.CurrentlySelected = null;
        this.Initialize(gridNb);
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
        UI.Instance.UpdatePowerUI();
    }

    private void SelectWord(int startR, int startC, Direction direction)
    {
        this.IterateLetters(startR, startC, direction, (LetterGridSquare square) =>
        {
            square.SecondarySelect();
            return true;
        });
        UI.Instance.UpdatePowerUI();
    }

    private void DeselectAll()
    {
        this.CurrentlySelected = null;
        foreach (GridSquare square in this.GridSquares)
        {
            square.Deselect();
        }
        UI.Instance.UpdatePowerUI();
    }

    public bool CheckSelectedWordLockedIn(out string word)
    {
        word = string.Empty;
        if (this.CurrentlySelected == null)
            return false;

        return this.CheckWordLockedIn(this.CurrentlySelected, this.CurrentDirection, out word);
    }

    public bool CheckWordLockedIn(LetterGridSquare letterInWord, Direction direction, out string word)
    {
        string fullWord = string.Empty;
        bool allCorrect = true;
        LetterGridSquare firstLetter = this.FindFirstLetterOfWord(letterInWord, direction);
        this.IterateLetters(firstLetter.R, firstLetter.C, direction, (LetterGridSquare square) =>
        {
            if (!square.LockedIn)
            {
                allCorrect = false;
                return false;
            }

            fullWord += square.Character;
            return true;
        });

        word = fullWord;
        if (allCorrect)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckSelectedWordFull()
    {
        if (this.CurrentlySelected == null)
            return false;

        bool allCorrect = true;
        LetterGridSquare firstLetter = this.FindFirstLetterOfWord(this.CurrentlySelected, this.CurrentDirection);
        this.IterateLetters(firstLetter.R, firstLetter.C, this.CurrentDirection, (LetterGridSquare square) =>
        {
            if (square.Character == '\0')
            {
                allCorrect = false;
                return false;
            }

            return true;
        });

        return allCorrect;
    }

    public bool CheckSelectedWordCorrect()
    {
        if (this.CurrentlySelected == null)
            return false;

        bool allCorrect = true;
        LetterGridSquare firstLetter = this.FindFirstLetterOfWord(this.CurrentlySelected, this.CurrentDirection);
        this.IterateLetters(firstLetter.R, firstLetter.C, this.CurrentDirection, (LetterGridSquare square) =>
        {
            if (square.Character != YAMLLoader.Instance.Grids[this.GridNb].grid[square.R][square.C])
            {
                allCorrect = false;
                return false;
            }

            return true;
        });

        return allCorrect;
    }

    public void LockInSelectedWord()
    {
        if (this.CurrentlySelected == null)
            return;

        LetterGridSquare firstLetter = this.FindFirstLetterOfWord(this.CurrentlySelected, this.CurrentDirection);
        this.IterateLetters(firstLetter.R, firstLetter.C, this.CurrentDirection, (LetterGridSquare square) =>
        {
            square.LockIn();
            return true;
        });
    }

    public void CheckJustFinishedWord(LetterGridSquare justLockedIn)
    {
        foreach (Direction direction in Utility.GetValues<Direction>())
        {
            if (this.IsValidDirection(justLockedIn, direction))
            {
                if (this.CheckWordLockedIn(justLockedIn, direction, out string word))
                {
                    YAMLLoader.Instance.Grids[this.GridNb].GetDefinition(word, out int index);
                    ServerConnector.Instance.SendLocationCheck($"Complete Word n°{index + 1} in Grid n°{this.GridNb + 1}");
                }
            }
        }
    }

    public void CheckGridFinished()
    {
        bool allCorrect = true;
        foreach (GridSquare square in this.GridSquares)
        {
            LetterGridSquare letterSquare = square as LetterGridSquare;
            if (letterSquare != null)
            {
                if (letterSquare.Character != YAMLLoader.Instance.Grids[this.GridNb].grid[letterSquare.R][letterSquare.C])
                {
                    allCorrect = false;
                    break;
                }
            }
        }

        // TODO particles and shit
        if (allCorrect)
        {
            foreach (GridSquare gridSquare in this.GridSquares)
            {
                LetterGridSquare letterSquare = gridSquare as LetterGridSquare;
                if (letterSquare != null)
                {
                    letterSquare.LockIn();
                }
            }

            for (int i = 0; i < YAMLLoader.Instance.YAML.Archipelamots.nb_of_checks_per_grid; i++)
            {
                ServerConnector.Instance.SendLocationCheck($"Complete Grid n°{this.GridNb + 1} ({i + 1})");
            }
        }
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
        Debug.Log($"FindFirstLetterOfWord (r{r}, c{c})");
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
