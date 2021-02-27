using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace BraveNewWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            int respawnX = 5, respawnY = 16;
            int playerX = 5, playerY = 5;
            int playerDX = 0, playerDY = 1;

            int allCoins = 0, collectCoins = 0;

            int[,] arrayOfTraps = new int[4, 4] { { 7, 14, 8, 12 }, { 7, 11, 7, 7 }, { 7, 4, 8, 4 }, { 9, 10, 10, 10 } }; //массив, который содержит в себе координаты переключателя и ловушки

            bool isSelectionMenuRun = true;

            Console.CursorVisible = false;

            char[,] map = ReadMap("mainMap.txt");
            DrawMap(map, ref allCoins);

            Console.WriteLine("\n<<<МЕНЮ ВЫБОРА>>>");
            Console.WriteLine("\n[1] - войти в режим создание карты\n[2] - играть в змейку\n[3] - Выход");
            Console.Write("Enter: ");

            while (isSelectionMenuRun)
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Respawn(ref respawnX, ref respawnY, ref playerX, ref playerY, map); // необязательный 5 параметр для первичной инициализации, впоследствии исп. для того, чтобы не перетирал @
                        EditMap(map, ref playerX, ref playerY, ref playerDX, ref playerDY, ref arrayOfTraps, ref allCoins, ref respawnX, ref respawnY);
                        break;
                    case "2":
                        Respawn(ref respawnX, ref respawnY, ref playerX, ref playerY, map);

                        bool isGameRun = true;
                        while (isGameRun)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (key.KeyChar == 'e') //экстренный выход
                            {
                                return;
                            }

                            ChangeDirection(key, ref playerDX, ref playerDY);
                            MovementOnMap('V', map, ref playerX, ref playerY, ref playerDX, ref playerDY); // V - иконка игрока

                            CheckPosition(ref map, ref playerX, ref playerY, ref collectCoins, respawnX, respawnY, arrayOfTraps);

                            EndGame(allCoins, collectCoins, ref isGameRun, ref isSelectionMenuRun);
                            System.Threading.Thread.Sleep(150);
                        }
                        break;
                    case "3":
                        Console.Write("Выход...");
                        return;
                }
            }
        }

        static char[,] ReadMap(string mapName)
        {
            string[] newFile = File.ReadAllLines($"Maps/{mapName}.txt");
            char[,] map = new char[newFile.Length, newFile[0].Length];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = newFile[i][j];
                    if (map[i, j] == ' ')
                    {
                        map[i, j] = ' ';
                    }
                }
            }
            return map;
        }

        static void DrawMap(char[,] map, ref int allCoins)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j]);
                    if (map[i, j] == '$')
                    {
                        allCoins++;
                    }
                }
                Console.WriteLine();
            }
        }

        static void EditMap(char[,] map, ref int playerX, ref int playerY, ref int playerDX, ref int playerDY, ref int[,] arrayOfTraps, ref int allCoins, ref int respawnX, ref int respawnY)
        {
            bool isEditMenuRun = true;

            Console.SetCursorPosition(2, 20);
            Console.WriteLine("<<<РЕДАКТОР КАРТЫ>>>");
            Console.WriteLine("[e] - выход из редактора, [f] - выбрать символ");

            while (isEditMenuRun)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.KeyChar == 'f')
                {
                    Console.SetCursorPosition(0, 22);
                    Console.WriteLine("Доступные символы:\n[#] - стена\n[$] - монетка\n[@] - ловушка\n[>] или [<] - гора, [I] - переключатель ловушки   [L] - связать переключатель" +
                        "\n[R] - точка возрождения");

                    ConsoleKeyInfo charKey = Console.ReadKey(true);
                    switch (charKey.KeyChar)
                    {
                        case '#':
                            SetSymbol(ref map, '#', playerX, playerY); //стенка
                            break;
                        case '$':
                            SetSymbol(ref map, '$', playerX, playerY); //монетка
                            allCoins++;
                            break;
                        case '@':
                            SetSymbol(ref map, '@', playerX, playerY); //ловушка
                            break;
                        case 'I':
                            SetSymbol(ref map, 'I', playerX, playerY); //переключатель ловушки
                            int toggleX = playerX; //координаты переключателя
                            int toggleY = playerY; //toggle - переключатель

                            while (true)
                            {
                                key = Console.ReadKey(true);

                                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow)
                                {
                                    ChangeDirection(key, ref playerDX, ref playerDY);
                                    MovementOnMap('V', map, ref playerX, ref playerY, ref playerDX, ref playerDY);
                                }
                                else if (key.KeyChar == 'e')
                                {
                                    break;
                                }
                                else if (key.KeyChar == 'L' && (map[playerX, playerY] == '@' || map[playerX, playerY] == '*'))
                                {
                                    BindTrap(playerX, playerY, ref arrayOfTraps, toggleX, toggleY);
                                    break;
                                }

                            }
                            break;
                        case 'R':
                            respawnX = playerX;
                            respawnY = playerY;
                            SetSymbol(ref map, 'R', playerX, playerY);
                            break;
                        case 'S':
                            SetSymbol(ref map, ' ', playerX, playerY);
                            break;
                    }
                }
                else if (key.KeyChar == 'e')
                {
                    Console.SetCursorPosition(7, 18);
                    Console.Write(" ");
                    break;
                }
                else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow)
                {
                    ChangeDirection(key, ref playerDX, ref playerDY);
                    MovementOnMap('V', map, ref playerX, ref playerY, ref playerDX, ref playerDY);
                }
            }
        }

        static void MovementOnMap(char symbol, char[,] map, ref int X, ref int Y, ref int DX, ref int DY)
        {
            Console.SetCursorPosition(Y, X);
            Console.Write(map[X, Y]);

            if (map[X + DX, Y + DY] != '#')
            {
                X += DX;
                Y += DY;
            }

            Console.SetCursorPosition(Y, X);
            Console.Write(symbol);
        }

        static void ChangeDirection(ConsoleKeyInfo key, ref int DX, ref int DY)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    DX = -1; DY = 0;
                    break;
                case ConsoleKey.DownArrow:
                    DX = 1; DY = 0;
                    break;
                case ConsoleKey.LeftArrow:
                    DX = 0; DY = -1;
                    break;
                case ConsoleKey.RightArrow:
                    DX = 0; DY = 1;
                    break;
            }
        }

        static void SetSymbol(ref char[,] map, char symbol, int playerX, int playerY)
        {
            Console.SetCursorPosition(playerY, playerX);
            map[playerX, playerY] = symbol;
            Console.Write(symbol);
        }

        static void CheckPosition(ref char[,] map, ref int playerX, ref int playerY, ref int collectCoins, int respawnX, int respawnY, int[,] arrayOfTraps)
        {
            if (map[playerX, playerY] == '$')
            {
                CollectCoins(ref map, playerX, playerY, ref collectCoins);
            }
            else if (map[playerX, playerY] == '@')
            {
                Respawn(ref respawnX, ref respawnY, ref playerX, ref playerY, map, '@');
            }
            else if (map[playerX, playerY] == 'I')
            {
                SetSymbol(ref map, 'i', playerX, playerY);
                for (int i = 0; i < arrayOfTraps.GetLength(0); i++)
                {
                    if (playerX == arrayOfTraps[i, 0] && playerY == arrayOfTraps[i, 1])
                    {
                        SetSymbol(ref map, '*', arrayOfTraps[i, 2], arrayOfTraps[i, 3]);
                    }
                }

            }
            else if (map[playerX, playerY] == 'i')
            {
                SetSymbol(ref map, 'I', playerX, playerY);

                for (int i = 0; i < arrayOfTraps.GetLength(0); i++)
                {
                    if (playerX == arrayOfTraps[i, 0] && playerY == arrayOfTraps[i, 1])
                    {
                        SetSymbol(ref map, '@', arrayOfTraps[i, 2], arrayOfTraps[i, 3]);
                    }
                }
            }

        }

        static void CollectCoins(ref char[,] map, int playerX, int playerY, ref int collectCoins)
        {
            collectCoins++;
            map[playerX, playerY] = ' ';
        }

        static void Respawn(ref int respawnX, ref int respawnY, ref int playerX, ref int playerY, char[,] map, char symbol = ' ')
        {
            SetSymbol(ref map, symbol, playerX, playerY);

            playerX = respawnX;
            playerY = respawnY;
            Console.SetCursorPosition(respawnY, respawnX);

            Console.Write('V');
        }

        static void BindTrap(int playerX, int playerY, ref int[,] arrayOfTraps, int toggleX, int toggleY)
        {
            int[,] tempArrayOfTraps = new int[arrayOfTraps.GetLength(0) + 1, arrayOfTraps.GetLength(1)]; //динамически расширяем многомерный массив

            for (int i = 0; i < arrayOfTraps.GetLength(0); i++)
            {
                for (int j = 0; j < arrayOfTraps.GetLength(1); j++)
                {
                    tempArrayOfTraps[i, j] = arrayOfTraps[i, j];
                }
            }

            tempArrayOfTraps[tempArrayOfTraps.GetLength(0) - 1, 0] = toggleX; //запоминаем местоположение переключателя
            tempArrayOfTraps[tempArrayOfTraps.GetLength(0) - 1, 1] = toggleY;

            tempArrayOfTraps[tempArrayOfTraps.GetLength(0) - 1, 2] = playerX; //запоминаем местоположение ловушки
            tempArrayOfTraps[tempArrayOfTraps.GetLength(0) - 1, 3] = playerY;

            arrayOfTraps = tempArrayOfTraps;
        }

        static void EndGame(int allCoins, int colectCoins, ref bool isGameRun, ref bool isSelectionMenuRun)
        {
            if (allCoins == colectCoins)
            {
                Console.Clear();
                Console.WriteLine("Победа!");
                isGameRun = false;
                isSelectionMenuRun = false;
                return;
            }
        }

    }
}