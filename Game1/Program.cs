using System;

namespace Tetris
{

    public static class Program
    {
 
        [STAThread]
        static void Main()
        {

            AppDomain.CurrentDomain.AppendPrivatePath("x64");

            using (var game = new Board())
                game.Run();
        }
    }
}
