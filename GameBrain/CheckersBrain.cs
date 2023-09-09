using ConsoleUI;
using Domain;

namespace GameBrain;

public class CheckersBrain
{
    public ESquareState[,] GameBoard = default!;

    public CheckersOptions? Options = default!;

    private int _boardWidth;
    private int _boardHeight;
    public bool WhiteTurn;
    public bool PieceTakenFlag;
    public Tuple<int, int>? PieceToRemove;

    public void InitializeGame()
    {
        LoadWidthAndHeight();
        WhiteTurn = Options!.WhiteStarts;
        GameBoard = new ESquareState[_boardHeight, _boardWidth];
        AddInitialPieces();
    }

    public void LoadWidthAndHeight()
    {
        _boardHeight = Options!.Height;
        _boardWidth = Options.Width;
    }

    private void AddInitialPieces() //this is for setting up the board at the start of the game
    {
        int pieceRows = Options!.PieceFilledRowsPerSide;
        for (var x = 0; x < _boardHeight; x++)
        {
            for (var y = 0; y < _boardWidth; y++)
            {
                //black pieces
                if (x < pieceRows)
                {
                    if (x % 2 == 1 && y % 2 == 0)
                    {
                        GameBoard[x, y] = ESquareState.BlackPiece;
                    }

                    if (x % 2 == 0 && y % 2 != 0)
                    {
                        GameBoard[x, y] = ESquareState.BlackPiece;
                    }
                }
                
                //white pieces
                if (x < _boardHeight - pieceRows)
                {
                    continue;
                }
                if (x % 2 == 0 && y % 2 != 0)
                {
                    GameBoard[x, y] = ESquareState.WhitePiece;
                }
                if (x % 2 == 1 && y % 2 == 0)
                {
                    GameBoard[x, y] = ESquareState.WhitePiece;
                }
            }
        }
    }

    public ESquareState[,] GetBoard()
    {
        var result = new ESquareState[GameBoard.GetLength(0), GameBoard.GetLength(1)];
        Array.Copy(GameBoard, result, GameBoard.Length);
        return result;
    }
    
    public ESquareState[,] ConvertJaggedArrayToBoard(ESquareState[][] array)
    {
        int firstDimension = array.Length;
        int secondDimension = array.GroupBy(row => row.Length).Single().Key;

        ESquareState[,] result = new ESquareState[firstDimension, secondDimension];
        for (int i = 0; i < firstDimension; ++i)
        {
            for (int j = 0; j < secondDimension; ++j)
            {
                result[i, j] = array[i][j];
            }
        }
        return result;
    }
    
   public  ESquareState[][] ConvertBoardToJaggedArray()
    {
        ESquareState[,] board = GetBoard();
        ESquareState[][] result = new ESquareState[board.GetLength(0)][];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            result[i] = new ESquareState[board.GetLength(1)];
            for (int j = 0; j < board.GetLength(1); j++)
            {
                ESquareState value = board[i, j];
                result[i][j] = value;
            }
        }
        return result;
    }

    public void MakeAMove(int sourceX, int sourceY, int targetX, int targetY) //NB! this method does not change turn!
    {
        if (!MoveIsValid(sourceX, sourceY, targetX, targetY))
        {
            throw new ArgumentException("invalid move!");
        }
        
        GameBoard[targetX, targetY] = GameBoard[sourceX, sourceY];
        GameBoard[sourceX, sourceY] = ESquareState.Empty;
        if (PieceToRemove != null)
        {
            if (GameBoard[PieceToRemove.Item1, PieceToRemove.Item2] != ESquareState.Empty)
            {
                PieceTakenFlag = true;
                GameBoard[PieceToRemove.Item1, PieceToRemove.Item2] = ESquareState.Empty;
            }
            else
            {
                PieceToRemove = null;
            }
        }

        //check for new king pieces
        if (WhiteTurn && targetX == 0)
        {
            GameBoard[targetX, targetY] = ESquareState.WhiteKing;
        }

        if (!WhiteTurn && targetX == _boardHeight - 1)
        {
            GameBoard[targetX, targetY] = ESquareState.BlackKing;
        }
    }

    public Tuple<int, int> MakeAMoveAI()
    {
        //Prioritises captures, otherwise random. Will also avoid scenarios where a non-capturing move lands next to
        //the players piece and allows the player to get a 'free' capture.
        //Moves are kept in [sourceX, sourceY, targetX, targetY] format inside a list.
        //landing square coordinates are returned
        
        List<List<int>> captureMoves = new List<List<int>>();
        List<List<int>> movementMoves = new List<List<int>>();
        for (var x = 0; x < _boardHeight; x++)
        {
            for (var y = 0; y < _boardWidth; y++)
            {
                List<Tuple<int, int>> validTargets = GetAllAvailableMoves(x, y, true);
                foreach (var targetSet in validTargets)
                {
                    List<int> moveToAdd = new List<int>{ x, y, targetSet.Item1, targetSet.Item2 };
                    captureMoves.Add(moveToAdd);
                }
    
                //If at least one capture has been found, there is no reason to look at non-capturing moves.
                if (captureMoves.Count > 0) continue;
                
                validTargets = GetAllAvailableMoves(x, y);
                foreach (var targetSet in validTargets)
                {
                    List<int> moveToAdd = new List<int>{ x, y, targetSet.Item1, targetSet.Item2 };
                    movementMoves.Add(moveToAdd);
                }
                
            }
        }
        
        //if possible take out the moves that would "sacrifice" own piece
        FilterMovesForAI(movementMoves);

        //choose random move, with preference for captures
        Random random = new Random();
        int index;
        List<int> chosenMove;
        
        if (captureMoves.Count == 0 && movementMoves.Count > 0) // no captures, make regular move
        {
            index = random.Next(movementMoves.Count);
            chosenMove = movementMoves[index];
            MakeAMove(chosenMove[0], chosenMove[1], chosenMove[2], chosenMove[3]);
            return new Tuple<int, int>(chosenMove[2], chosenMove[3]);
        }

        index = random.Next(captureMoves.Count);
        chosenMove = captureMoves[index];
        MakeAMove(chosenMove[0], chosenMove[1], chosenMove[2], chosenMove[3]);
        return new Tuple<int, int>(chosenMove[2], chosenMove[3]);
    }

    private void FilterMovesForAI(List<List<int>> movementMoves)
    {
        var movementMovesCopy = movementMoves.ConvertAll(x => new List<int>(x));
        foreach (var move in movementMovesCopy)
        {
            if (movementMoves.Count <= 1) break;

            int diagonalX = WhiteTurn ? move[2] - 1 : move[2] + 1;
            int leftDiagonalY = move[3] - 1;
            int rightDiagonalY = move[3] + 1;

            //temporarily changing the board to simulate a move 
            ESquareState piece = GameBoard[move[0], move[1]];
            GameBoard[move[2], move[3]] = piece;
            GameBoard[move[0], move[1]] = ESquareState.Empty;

            //finding possible captures
            ChangeTurn();
            List<Tuple<int, int>> counterMovesLeft = GetAllAvailableMoves(diagonalX, leftDiagonalY, true);
            List<Tuple<int, int>> counterMovesRight = GetAllAvailableMoves(diagonalX, rightDiagonalY, true);
            ChangeTurn();

            //removing temporary piece
            GameBoard[move[2], move[3]] = ESquareState.Empty;
            GameBoard[move[0], move[1]] = piece;

            //remove move from list if it has a counter
            if (counterMovesLeft.Count > 0 || counterMovesRight.Count > 0)
            {
                bool found = false;
                for (int i = 0; i < movementMoves.Count; i++)
                {
                    for (int j = 0; j <= 3; j++)
                    {
                        if (movementMoves[i][j] != move[j]) break;

                        if (j != 3) continue;
                        movementMoves.RemoveAt(i);
                        found = true;
                    }
                    if (found) break;
                }
            }
        }
    }

    public void ChangeTurn()
    {
        WhiteTurn = !WhiteTurn;
    }
    
    public bool CoordinatesAreWithinBounds(int? x, int? y)
    {
        if (x is null || y is null)
        {
            return false;
        }
        return x >= 0 && y >= 0 && x < _boardHeight && y < _boardWidth;
    }

    private bool MoveIsValid(int sourceX, int sourceY, int targetX, int targetY)
    {
        if (!CoordinatesAreWithinBounds(targetX, targetY))
        {
            return false;
        }
        
        if (!CoordinatesAreWithinBounds(sourceX, sourceY))
        {
            return false;
        }
        
        if (GameBoard[targetX, targetY] != ESquareState.Empty)
        {
            return false;
        }

        List<Tuple<int, int>> possibleCapturesOnBoard = GetAllAvailableMoves(null, null, true);
        List<Tuple<int, int>> legalMoves;
        if (Options!.CaptureMandatory && possibleCapturesOnBoard.Count != 0)
        {
            legalMoves = GetAllAvailableMoves(sourceX, sourceY, true);
        }
        else
        {
            legalMoves = GetAllAvailableMoves(sourceX, sourceY);
        }
        
        if (legalMoves.Contains(new Tuple<int, int>(targetX, targetY)))
        {
            //marking a piece for capture
            if (Math.Abs(targetX - sourceX) >= 2)
            {
                bool directionIsTopForX = targetX < sourceX;
                bool directionIsLeftForY = targetY < sourceY;
                int coordX = directionIsTopForX ? targetX + 1 : targetX - 1;
                int coordY = directionIsLeftForY ? targetY + 1 : targetY - 1;
                PieceToRemove = new Tuple<int, int>(coordX, coordY);
            }
            return true;
        }
        return false;
    }
    
    public bool CanCaptureAgain(int sourceX, int sourceY)
    {
        if (PieceTakenFlag)
        {
            if (GameBoard[sourceX, sourceY] == ESquareState.BlackKing ||
                GameBoard[sourceX, sourceY] == ESquareState.WhiteKing)
            {
                return KingCanCaptureAgain(sourceX, sourceY);
            }
            List<Tuple<int, int>> availableCaptures = GetAllAvailableMoves(sourceX, sourceY, true);
            if (availableCaptures.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool KingCanCaptureAgain(int sourceX, int sourceY)
    {
        List<Tuple<int, int>> availableCaptures = GetAllAvailableMoves(sourceX, sourceY, true, true);
        if (availableCaptures.Count > 0)
        {
            return true;
        }
        return false;
    }

    public List<Tuple<int, int>> GetAllAvailableMoves(int? sourceX = null, int? sourceY = null, bool captureOnly = false,
        bool limitDistanceForKing = false)
    {
        //Each tuple is a set of coordinates that represent one of the possible landing squares
        //if there are no moves available, the returned list is empty.
        //source piece can be defined, in which case only moves for that specific piece will be returned.
        //if captureOnly is true, only available captures are returned and moves to empty squares are ignored.

        var result = new List<Tuple<int, int>>();
        
        ESquareState pieceNormal = WhiteTurn ? ESquareState.WhitePiece : ESquareState.BlackPiece;
        ESquareState pieceKing = WhiteTurn ? ESquareState.WhiteKing : ESquareState.BlackKing;
        
        ESquareState opponentPieceNormal = WhiteTurn ? ESquareState.BlackPiece : ESquareState.WhitePiece;
        ESquareState opponentPieceKing = WhiteTurn ? ESquareState.BlackKing : ESquareState.WhiteKing;

        bool sourcePieceExists = CoordinatesAreWithinBounds(sourceX, sourceY);

        for (var x = 0; x < _boardHeight; x++)
        {
            for (var y = 0; y < _boardWidth; y++)
            {
                if (sourcePieceExists && (x != sourceX || y != sourceY)) //skip if not the piece we're looking for
                {
                    continue;
                }
                
                //normal piece
                if (GameBoard[x, y] == pieceNormal)
                {
                    int diagonalX = WhiteTurn ? x - 1 : x + 1;
                    int secondDiagonalX = WhiteTurn ? x - 2 : x + 2;
                    
                    int leftDiagonalY = y - 1;
                    int secondLeftDiagonalY = y - 2;
                    
                    int rightDiagonalY = y + 1;
                    int secondRightDiagonalY = y + 2;
                    
                    int backDiagonalX = WhiteTurn ? x + 1 : x - 1;
                    int secondBackDiagonalX = WhiteTurn ? x + 2 : x - 2;

                    if (!captureOnly)
                    {
                        //move to empty left diagonal
                        GetMoveFromDiagonal(diagonalX, leftDiagonalY, null, null,
                            result, null, null);
                        
                        //move to empty right diagonal
                        GetMoveFromDiagonal(diagonalX, rightDiagonalY, null, null,
                            result, null, null);
                    }

                    //top left capture
                    GetMoveFromDiagonal(diagonalX, leftDiagonalY, secondDiagonalX, secondLeftDiagonalY,
                        result, opponentPieceNormal, opponentPieceKing);
                    
                    //top right capture
                    GetMoveFromDiagonal(diagonalX, rightDiagonalY, secondDiagonalX, secondRightDiagonalY,
                        result, opponentPieceNormal, opponentPieceKing);
                    
                    
                    if (Options!.CaptureBackwardsAllowed)
                    {
                        //bottom left capture
                        GetMoveFromDiagonal(backDiagonalX, leftDiagonalY, secondBackDiagonalX,
                            secondLeftDiagonalY, result, opponentPieceNormal, opponentPieceKing);
                        
                        //bottom right capture
                        GetMoveFromDiagonal(backDiagonalX, rightDiagonalY, secondBackDiagonalX,
                            secondRightDiagonalY, result, opponentPieceNormal, opponentPieceKing);
                    }
                }

                //king piece
                if (GameBoard[x, y] == pieceKing)
                {
                    TraverseDiagonalForKing(x, y, true, true, opponentPieceNormal,
                        opponentPieceKing, captureOnly, result, limitDistanceForKing);
                    
                    TraverseDiagonalForKing(x, y, true, false, opponentPieceNormal,
                        opponentPieceKing, captureOnly, result, limitDistanceForKing);
                    
                    TraverseDiagonalForKing(x, y, false, true, opponentPieceNormal,
                        opponentPieceKing, captureOnly, result, limitDistanceForKing);
                    
                    TraverseDiagonalForKing(x, y, false, false, opponentPieceNormal,
                        opponentPieceKing, captureOnly, result, limitDistanceForKing);
                }
            }
        }
        return result;
    }
    
    private void GetMoveFromDiagonal(int diagonalX, int diagonalY, int? secondDiagonalX, int? secondDiagonalY,
        List<Tuple<int, int>> result, ESquareState? opponentPieceNormal, ESquareState? opponentPieceKing)
    {
        //move to empty
        if (CoordinatesAreWithinBounds(diagonalX, diagonalY) && opponentPieceKing == null && opponentPieceNormal == null)
        {
            if (GameBoard[diagonalX, diagonalY] == ESquareState.Empty)
            {
                result.Add(new Tuple<int, int>(diagonalX, diagonalY));
            }
        }

        //capture piece
        if (CoordinatesAreWithinBounds(secondDiagonalX, secondDiagonalY))
        {
            if ((GameBoard[diagonalX, diagonalY] == opponentPieceNormal ||
                GameBoard[diagonalX, diagonalY] == opponentPieceKing) && 
                GameBoard[secondDiagonalX!.Value, secondDiagonalY!.Value] == ESquareState.Empty)
            {
                result.Add(new Tuple<int, int>(secondDiagonalX.Value, secondDiagonalY.Value));
            }
        }
    }

    private void TraverseDiagonalForKing(int x, int y, bool directionXisTop, bool directionYisLeft, 
        ESquareState opponentPieceNormal, ESquareState opponentPieceKing, bool captureOnly,
        List<Tuple<int, int>> result, bool limitDistance = false)
    {
        int currentDiagonalX = directionXisTop ? x - 1 : x + 1;
        int nextDiagonalX = directionXisTop ? x - 2 : x + 2;

        int currentDiagonalY = directionYisLeft ? y - 1 : y + 1;
        int nextDiagonalY = directionYisLeft ? y - 2 : y + 2;
        
        int loopCounter = 0;

        while (CoordinatesAreWithinBounds(currentDiagonalX, currentDiagonalY))
        {
            if (Options!.KingCanFly == false || limitDistance)
            {
                if (loopCounter >= 1)
                {
                    break;
                }
            }
            
            //empty square
            if (!captureOnly && GameBoard[currentDiagonalX, currentDiagonalY] == ESquareState.Empty)
            {
                result.Add(new Tuple<int, int>(currentDiagonalX, currentDiagonalY));

            }
            
            //capture and move to empty square after it
            if (CoordinatesAreWithinBounds(nextDiagonalX, nextDiagonalY))
            {
                if (GameBoard[currentDiagonalX, currentDiagonalY] == opponentPieceNormal ||
                    GameBoard[currentDiagonalX, currentDiagonalY] == opponentPieceKing)
                {
                    if (GameBoard[nextDiagonalX, nextDiagonalY] == ESquareState.Empty)
                    {
                        result.Add(new Tuple<int, int>(nextDiagonalX, nextDiagonalY));
                    }

                    break;
                }
            }

            //same side piece as king is in the way
            if (GameBoard[currentDiagonalX, currentDiagonalY] != ESquareState.Empty)
            {
                break;
            }

            loopCounter++;
            
            //updating coordinates
            currentDiagonalX = directionXisTop ? currentDiagonalX - 1 : currentDiagonalX + 1;
            nextDiagonalX = directionXisTop ? nextDiagonalX - 1 : nextDiagonalX + 1;

            currentDiagonalY = directionYisLeft ? currentDiagonalY - 1 : currentDiagonalY + 1;
            nextDiagonalY = directionYisLeft ? nextDiagonalY - 1 : nextDiagonalY + 1;
        }
    }
    
}