using Xenko.Engine;

namespace OffscreenRenderer.Windows
{
    class OffscreenRendererApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
