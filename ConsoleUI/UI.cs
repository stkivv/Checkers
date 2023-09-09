namespace ConsoleUI;

public class UI
{

    public void DrawGameBoard(ESquareState[,] board)
    {
        var rows = board.GetLength(0);
        var cols = board.GetLength(1);
        
        Console.Clear();
        
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine("");
        }
        
        MakeSpacesToCenterText(cols * 7);
        MakeLetterCoords(cols);

        ConsoleColor current = ConsoleColor.White;
        int rowCounter = 0;
        while (rowCounter < rows)
        {
            MakeRow(cols, rowCounter, current, false, board);
            MakeRow(cols, rowCounter, current, true, board);
            MakeRow(cols, rowCounter, current, false, board);

            current = SwapConsoleColor(current);
            rowCounter++;
        }

        Console.BackgroundColor = ConsoleColor.Black;

    }

    private void MakeRow(int cols, int row, ConsoleColor current, bool pieceRow, ESquareState[,] board)
    {
        MakeSpacesToCenterText(cols * 7);
        if (pieceRow)
        {
            MakeNumberCoord(row);
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("       ");
        }
        
        ConsoleColor color = current;
        for (int i = 0; i < cols; i++)
        {
            Console.BackgroundColor = color;
            if (pieceRow)
            {
                if (board[row, i] == ESquareState.BlackPiece)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  ■■■  ");
                    Console.ForegroundColor = ConsoleColor.White;
                } 
                else if (board[row, i] == ESquareState.BlackKing)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("  ■■■  ");
                }
                else if (board[row, i] == ESquareState.WhitePiece)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  ■■■  ");
                }
                else if (board[row, i] == ESquareState.WhiteKing)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("  ■■■  ");
                }
                else
                {
                    Console.Write("       ");
                }
            }
            else
            {
                Console.Write("       ");
            }
            color = SwapConsoleColor(color);
        }
        Console.WriteLine("");
    }

    private ConsoleColor SwapConsoleColor(ConsoleColor current)
    {
        return current == ConsoleColor.Black ? ConsoleColor.White : ConsoleColor.Black;
    }

    private static void MakeNumberCoord(int row)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Cyan;
        string nrText = $"   {row + 1}";
        while (nrText.Length < 7)
        {
            nrText += " ";
        }
        Console.Write(nrText);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void MakeLetterCoords(int cols)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        Console.Write("       ");
        for (int i = 0; i < cols; i++)
        {
            Console.Write("   ");
            Console.Write(alphabet[i]);
            Console.Write("   ");
        }

        Console.WriteLine("");

        for (int i = 0; i < cols; i++)
        {
            Console.Write("       ");
        }

        Console.ResetColor();
        Console.WriteLine("");
    }
    
    private void MakeSpacesToCenterText(int length)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        int spacesToMake = Console.WindowWidth / 2 - length / 2;
        for (int i = 0; i < spacesToMake; i++)
        {
            Console.Write(" ");
        }
    }
}