using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class WordSearchGenerator
{
    private static Random _random = new Random();
    private static int BoardSize = 8;
    private const int MaxAttempts = 100;

    public static void Main(string[] args)
    {
        // Ensure the "Boards" directory exists
        Directory.CreateDirectory("Boards");

        // Read words from file
        List<string> words = ReadWordsFromFile("newwords.txt");
        int boardsGenerated = 0;
        List<WordSearchBoard> boards = new List<WordSearchBoard>();

        while (boardsGenerated < 350)
        {
            Console.WriteLine($"Generating board #{boardsGenerated + 1}...");
            var board = GenerateBoard(words);
            if (board != null)
            {
                // Save boards to JSON file
                string fileName = $"Boards/wordFind-Board_{DateTime.Now:yyyyMMdd_HHmmss}__{boardsGenerated + 1}.json";
                SaveBoardsToJson(board, fileName);
                boardsGenerated++;
                PrintBoard(board, boardsGenerated);
            }
            else
            {
                Console.WriteLine($"Skipping board #{boardsGenerated + 1} due to failed generation.");
            }
        }
    }

    private static List<string> ReadWordsFromFile(string filePath)
    {
        List<string> words = new List<string>();
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                words.Add(line.Trim().ToUpper());
            }
        }
        return words;
    }

    private static WordSearchBoard GenerateBoard(List<string> words)
    {
        char[,] board = new char[BoardSize, BoardSize];
        InitializeBoard(board);
        List<string> placedWords = new List<string>();

        var threeLetterWords = words.Where(w => w.Length == 3).ToList();
        var fourLetterWords = words.Where(w => w.Length == 4).ToList();
        var fiveLetterWords = words.Where(w => w.Length == 5).ToList();
        var sixLetterWords = words.Where(w => w.Length == 6).ToList();
        var sevenLetterWords = words.Where(w => w.Length == 7).ToList();
        var eightLetterWords = words.Where(w => w.Length == 8).ToList();

        // Place 5 to 6 words that are three, four, or five letters long
        int shortWordsCount = _random.Next(5, 7);
        List<string> shortWords = new List<string>();
        shortWords.AddRange(threeLetterWords);
        shortWords.AddRange(fourLetterWords);
        shortWords.AddRange(fiveLetterWords);

        shortWords = shortWords.OrderBy(w => _random.Next()).Take(shortWordsCount).ToList();

        foreach (var word in shortWords)
        {
            if (!PlaceWord(board, word, (WordDirection)_random.Next(3)))
                return null; // If placement fails, skip this board
            placedWords.Add(word);
        }

        // Place the remaining words that are six, seven, or eight letters long
        List<string> longWords = new List<string>();
        longWords.AddRange(sixLetterWords);
        longWords.AddRange(sevenLetterWords);
        longWords.AddRange(eightLetterWords);

        foreach (var word in longWords.OrderBy(w => _random.Next()))
        {
            if (placedWords.Contains(word)) continue;

            if (PlaceWord(board, word, (WordDirection)_random.Next(3)))
            {
                placedWords.Add(word);
            }
        }

        // Fill the remaining empty cells with random letters
        FillEmptyCells(board);

        return new WordSearchBoard
        {
            Board = ConvertBoardToList(board),
            Words = placedWords
        };
    }

    private static void InitializeBoard(char[,] board)
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                board[i, j] = ' ';
            }
        }
    }

    private static bool PlaceWord(char[,] board, string word, WordDirection direction)
    {
        int attempts = MaxAttempts; // Try to place the word within MaxAttempts

        while (attempts > 0)
        {
            int row = _random.Next(BoardSize);
            int col = _random.Next(BoardSize);

            if (CanPlaceWord(board, word, row, col, direction))
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (direction == WordDirection.Horizontal) board[row, col + i] = word[i];
                    else if (direction == WordDirection.Vertical) board[row + i, col] = word[i];
                    else if (direction == WordDirection.Diagonal) board[row + i, col + i] = word[i];
                }
                return true;
            }

            attempts--;
        }

        return false;
    }

    private static bool CanPlaceWord(char[,] board, string word, int row, int col, WordDirection direction)
    {
        if (direction == WordDirection.Horizontal)
        {
            if (col + word.Length > BoardSize) return false;

            for (int i = 0; i < word.Length; i++)
            {
                if (board[row, col + i] != ' ' && board[row, col + i] != word[i]) return false;
            }
        }
        else if (direction == WordDirection.Vertical)
        {
            if (row + word.Length > BoardSize) return false;

            for (int i = 0; i < word.Length; i++)
            {
                if (board[row + i, col] != ' ' && board[row + i, col] != word[i]) return false;
            }
        }
        else if (direction == WordDirection.Diagonal)
        {
            if (row + word.Length > BoardSize || col + word.Length > BoardSize) return false;

            for (int i = 0; i < word.Length; i++)
            {
                if (board[row + i, col + i] != ' ' && board[row + i, col + i] != word[i]) return false;
            }
        }

        return true;
    }

    private static void FillEmptyCells(char[,] board)
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (board[i, j] == ' ')
                {
                    board[i, j] = (char)('A' + _random.Next(26));
                }
            }
        }
    }

    private static List<BoardCell> ConvertBoardToList(char[,] board)
    {
        var boardList = new List<BoardCell>();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                boardList.Add(new BoardCell
                {
                    Letter = board[row, col],
                    Row = row,
                    Col = col
                });
            }
        }

        return boardList;
    }

    private static void PrintBoard(WordSearchBoard board, int boardNumber)
    {
        Console.WriteLine($"Board #{boardNumber}:");
        char[,] grid = new char[BoardSize, BoardSize];

        foreach (var cell in board.Board)
        {
            grid[cell.Row, cell.Col] = cell.Letter;
        }

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                Console.Write(grid[row, col] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static void SaveBoardsToJson(WordSearchBoard board, string fileName)
    {
        var json = JsonConvert.SerializeObject(board, Formatting.Indented);
        File.WriteAllText(fileName, json);
    }
}

public class WordSearchBoard
{
    public List<BoardCell> Board { get; set; }
    public List<string> Words { get; set; }
}

public class BoardCell
{
    public char Letter { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}

public enum WordDirection
{
    Horizontal,
    Vertical,
    Diagonal
}
