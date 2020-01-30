﻿using OpenTK.Graphics;

namespace Graphics
{
    public static class Program
    {
        private static void Main()
        {
            GraphicsMode mode = new GraphicsMode(new ColorFormat(24), 16, 8, 4, new ColorFormat(32), 2, false);
            using (var window = new Window(1200, 800, "ManipuS", mode))
            {
                window.Run(60.0);
            }
        }
    }
}