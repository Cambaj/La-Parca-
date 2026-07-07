using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSDL2.Engine
{
    // Atributos
    public class Sound
    {
        // Atributos
        IntPtr pointer;

        // Operaciones

        // Constructor a partir de un nombre de fichero
        public Sound(string nombreFichero)
        {
            pointer = SDL_mixer.Mix_LoadMUS(nombreFichero);

            if (pointer == IntPtr.Zero)
            {
                Console.WriteLine("Error al cargar música: " + SDL.SDL_GetError());
            }
        }

        // Reproducir una vez
        public void Play()
        {
            SDL_mixer.Mix_PlayMusic(pointer, 1);
        }

        // Reproducir continuo (musica de fondo)
        public void PlayLooping()
        {
            SDL_mixer.Mix_PlayMusic(pointer, -1);
        }

        // Interrumpir toda la reproducción de sonido
        public void Stop()
        {
            SDL_mixer.Mix_HaltMusic();
        }

    }
}
