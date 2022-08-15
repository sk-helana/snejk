using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using static System.Console;
using static System.ConsoleColor;

char UPPER = Encoding.UTF8.GetChars(new byte[] {0xe2, 0x96, 0x80}) [0];
char LOWER = Encoding.UTF8.GetChars(new byte[] {0xe2, 0x96, 0x84}) [0];
char FULL =  Encoding.UTF8.GetChars(new byte[] {0xe2, 0x96, 0x88}) [0];
char PILLS = Encoding.UTF8.GetChars(new byte[] {0xe2, 0x96, 0x91}) [0];

Clear();
CursorVisible = false;
ForegroundColor = DarkMagenta;
Drawing.Box(0, 0, Console.WindowWidth, Console.WindowHeight).Paint();    

ForegroundColor = DarkYellow;
Drawing
    .Line(10, 20, 5, Drawing.LineOrientation.Horizontal)
    .Merge(Drawing.Line(10, 15, 5, Drawing.LineOrientation.Horizontal))
    .Merge(Drawing.Line(10, 10, 5, Drawing.LineOrientation.Horizontal))
    .Merge(Drawing.Line(10, 10, 5, Drawing.LineOrientation.Vertical))
    .Merge(Drawing.Line(15, 15, 6, Drawing.LineOrientation.Vertical))
    .Paint();

Console.Write("Press any key to play");
Console.ReadKey(true);
Clear();

Random rand = new((int)System.DateTime.Now.Ticks);

int score = 0;
int delay = 50;
int snakelen = 6;
LinkedList<(int x, int y)> Snake = new();

(int x, int y) direction = (1,0);
(int x, int y) steroids = (20,10);

//create snake
Snake.AddLast((10,10));
Snake.AddLast((10,10));
Snake.AddLast((10,10));
Snake.AddLast((10,10));
Snake.AddLast((10,10));
Snake.AddLast((10,10));

//paint
foreach(var part in Snake) //full init paint since only head and tail are repainted in loop
{
    DrawSnake(Snake, part);
}
Paint(steroids, PILLS);
PaintScore(score);
Drawing.Box(0, 1, Console.WindowWidth, Console.WindowHeight - 1).Paint();    

try {
    while(true)
    {
        //process input
        if (KeyAvailable) {
            var key = ReadKey(true).Key;

            if(key == ConsoleKey.LeftArrow) direction = (-1, 0);
            else if(key == ConsoleKey.RightArrow) direction = (1, 0);
            else if(key == ConsoleKey.DownArrow) direction = (0, 1);
            else if(key == ConsoleKey.UpArrow) direction = (0, -1);
            else if(key == ConsoleKey.Escape) {
                Snake = null;
            };
        }

        //move head of snake
        var head = Snake.Last.Value;

        (int x, int y) newHead = (head.x + direction.x, head.y + direction.y);

        //ate itself?
        if(Snake.Contains(newHead)) {
            Snake = null;
        }

        //draw the head
        Snake.AddLast(newHead);
        DrawSnake(Snake, Snake.Last.Value);

        //ate fruit?
        if (newHead.x - steroids.x == 0 && newHead.y / 2 - steroids.y / 2 == 0) {
            //up score
            score++;
            
            //grow snake
            
            for(int i = 0; i<snakelen;i++)
                Snake.AddFirst(Snake.First.Value);
            snakelen*=2;
            
            var tail2 = Snake.First;
            Snake.RemoveFirst();
            Erase(Snake, tail2.Value);
        
            //place new fruit
            (int x, int y) check = (0, 0);
            do
            {
                steroids = (rand.Next(2, Console.WindowWidth - 1),rand.Next(3, Console.WindowHeight * 2 - 2));
                check = (steroids.x, steroids.y + steroids.y % 2 == 0 ? 1 : -1);
            } while(Snake.Contains(steroids) || Snake.Contains(check));

            Paint(steroids, PILLS);

            if(delay > 5) delay -= 5; else delay = 0;
        }

        //(re)move tail
        var tail = Snake.First;
        Snake.RemoveFirst();
        Erase(Snake, tail.Value);

        //repaint
        PaintScore(score); //always paint score to move cursor
        Thread.Sleep(delay);
    }
} catch(Exception e) {
    Clear();
    ForegroundColor = Red;
    WriteLine($"You were killed by {e.GetType()} ({e.Message})");
    ForegroundColor = Yellow;
    WriteLine($"Score: {score}");
    ForegroundColor = Green;
    WriteLine("Game over");
    return;
}

void DrawSnake(LinkedList<(int x, int y)> Snake, (int x, int y) e)
{
    (int x, int y) = e;

    CursorLeft = x;
    CursorTop = y / 2;
    bool isUpper = y % 2 == 0;
    
    bool isShared = Snake.Contains((x, isUpper ? y + 1: y - 1));

    ForegroundColor = ConsoleColor.Green;

    if(isShared) Write(FULL);
    else if (isUpper) Write(UPPER);
    else Write(LOWER);    
}

void Erase(LinkedList<(int x, int y)> Snake, (int x, int y) e)
{
    if (Snake.Contains(e)) return;

    (int x, int y) = e;

    CursorLeft = x;
    CursorTop = y / 2;
    bool isUpper = y % 2 == 0;
    
    bool isShared = Snake.Contains((x, isUpper ? y + 1: y - 1));

    ForegroundColor = ConsoleColor.Green;

    if(isUpper && isShared) Write(LOWER);
    else if (isShared) Write(UPPER);
    else Write(' ');    
}

void Paint((int x, int y) c, char what = '*') {
    ForegroundColor = Cyan;

    CursorLeft = c.x;
    CursorTop = c.y / 2;
    Write(what);
}

void PaintScore(int score) {
    ForegroundColor = Yellow;

    CursorLeft = 0;
    CursorTop = 0;
    WriteLine($"Score: {score}");
}
