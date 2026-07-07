using SDL2;


namespace ProyectoSDL2.Engine
{
    public class Image
    {
        public IntPtr Pointer { get; private set; }

        public Image(string imagePath)
        {
            LoadImage(imagePath);
        }

        private void LoadImage(string imagePath)
        {
            Pointer = SDL_image.IMG_LoadTexture(Engine.renderer, imagePath);
            if (Pointer == IntPtr.Zero)
            {
                Console.WriteLine("Imagen inexistente: {0}", imagePath);
                Environment.Exit(4);
            }
        }
    }
}
