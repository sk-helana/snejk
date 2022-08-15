using System;
using System.Linq;
using System.Collections.Generic;
public class LineMapper
{
    public const int NORTH = 1;
    public const int SOUTH = 2;
    public const int EAST = 4;
    public const int WEST = 8;

    private static Dictionary<int, char> _CharCodes = null;
    private static Dictionary<int, char> CharCodes
    {
        get
        {
            if(_CharCodes == null)
            {
                (int key, char symbol)[] codes = new (int key, char symbol)[] {
                    (NORTH, (char)0x2575),
                    (SOUTH, (char)0x2577),
                    (EAST, (char)0x2576),
                    (WEST, (char)0x2574),

                    (NORTH|SOUTH, (char)0x2502),
                    (EAST|WEST, (char)0x2500),

                    (SOUTH|WEST, (char)0x2510),
                    (NORTH|EAST, (char)0x2514),
                    (NORTH|EAST|WEST,(char)0x2534),
                    (SOUTH|EAST|WEST,(char)0x252C),
                    (NORTH|SOUTH|EAST, (char)0x251C),
                    (NORTH|SOUTH|WEST, (char)0x2524),
                    (NORTH|SOUTH|EAST|WEST, (char)0x253C),
                    (NORTH|WEST, (char)0x2518),
                    (SOUTH|EAST, (char)0x250C)
                };

                _CharCodes = codes.ToDictionary(i => i.key, i => i.symbol);
            }

            return _CharCodes;
        }
    }

    public static char GetChar(int directions)
    {
        return CharCodes[directions];
    }
}

public class Segment : IEquatable<Segment>
{
    public (int X, int Y) Point = (0, 0);
    public int Directions = 0;
    public void Paint()
    {
        Console.SetCursorPosition(Point.X, Point.Y);
        Console.Write(LineMapper.GetChar(Directions));
    }

    public Segment(int X, int Y, int Directions) : this((X,Y), Directions)
    {
    }

    public Segment((int X, int Y) Point, int Directions)
    {
        this.Point = Point;
        this.Directions = Directions;
    }

    bool IEquatable<Segment>.Equals(Segment other)
    {
        if (other == null)
        {
            return false;
        }
        
        return Point == other.Point;
    }
}

public class Drawing
{
    public enum LineOrientation
    {
        None,
        Horizontal,
        Vertical
    }
    public Segment[] Segments = new Segment[0];
    public Dictionary<(int X, int Y), int> AsDictionary() => Segments.ToDictionary(s => s.Point, s => s.Directions);
    public static Drawing Line(int x, int y, int length, LineOrientation orientation)
    {

        Segment[] segments = new Segment[length];

        if (orientation == LineOrientation.Horizontal)
        {
            segments[0] = new Segment(x, y, LineMapper.EAST);
            for(int i = 1;i < length - 1; i++)
                segments[i] = new Segment(x + i, y, LineMapper.EAST | LineMapper.WEST);
            segments[length - 1] = new Segment(x + length - 1, y, LineMapper.WEST);
        }

        else if (orientation == LineOrientation.Vertical)
        {                
            segments[0] = new Segment(x, y, LineMapper.SOUTH);
            for(int i = 1;i < length - 1; i++)
                segments[i] = new Segment(x, y + i, LineMapper.NORTH | LineMapper.SOUTH);
            segments[length - 1] = new Segment(x, y + length - 1, LineMapper.NORTH);

        }

        return new Drawing() {Segments = segments};
    }

    public static Drawing Box(int x, int y, int w, int h) 
    {
        return Drawing.Line(x, y, w, LineOrientation.Horizontal)
            .Merge(Drawing.Line(x, y, h, LineOrientation.Vertical))
            .Merge(Drawing.Line(x + w - 1, y, h, LineOrientation.Vertical))
            .Merge(Drawing.Line(x, y + h - 1, w,LineOrientation.Horizontal));
    }
    public Drawing Merge(Drawing other)
    {
        var mine = AsDictionary();
        var theirs = other.AsDictionary();

        List<Segment> segments = new List<Segment>();

        foreach(var key in mine.Keys)
        {
            if(theirs.ContainsKey(key))
            {
                segments.Add(new Segment(key, mine[key] | theirs[key]));
                theirs.Remove(key);
                continue;
            }

            int flags = 0;
            if((mine[key] & LineMapper.EAST) == 0 && theirs.ContainsKey((key.X + 1, key.Y)))
                flags = flags | LineMapper.EAST;                    
            if((mine[key] & LineMapper.NORTH) == 0 && theirs.ContainsKey((key.X, key.Y - 1)))
                flags = flags | LineMapper.NORTH;                    
            if((mine[key] & LineMapper.WEST) == 0 && theirs.ContainsKey((key.X - 1, key.Y)))
                flags = flags | LineMapper.WEST;                    
            if((mine[key] & LineMapper.SOUTH) == 0 && theirs.ContainsKey((key.X, key.Y + 1)))
                flags = flags | LineMapper.SOUTH;

            segments.Add(new Segment(key, mine[key] | flags));               
        }

        foreach(var key in theirs.Keys)
        {
            int flags = 0;
            if((theirs[key] & LineMapper.EAST) == 0 && mine.ContainsKey((key.X + 1, key.Y)))
                flags = flags | LineMapper.EAST;                    
            if((theirs[key] & LineMapper.NORTH) == 0 && mine.ContainsKey((key.X, key.Y - 1)))
                flags = flags | LineMapper.NORTH;                    
            if((theirs[key] & LineMapper.WEST) == 0 && mine.ContainsKey((key.X - 1, key.Y)))
                flags = flags | LineMapper.WEST;                    
            if((theirs[key] & LineMapper.SOUTH) == 0 && mine.ContainsKey((key.X, key.Y + 1)))
                flags = flags | LineMapper.SOUTH;

            segments.Add(new Segment(key, theirs[key] | flags));               
        }

        return new Drawing() { Segments = segments.ToArray() };
    }

    public void offset((int x, int y) offset)
    {
        
    }

    public void Paint()
    {
        foreach(var s in Segments)
            s.Paint();
    }
}
