using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSDL2.Engine
{
    class Engine
    {
        static IntPtr screen;
        static int ancho, alto;
        static public IntPtr renderer;
        static public IntPtr window;



        public static void DrawText(string text, int x, int y, byte r, byte g, byte b, Font f)
        {
            IntPtr font = f.pointer;
            SDL.SDL_Color textColor = new SDL.SDL_Color { r = r, g = g, b = b, a = 255 };

            // Renderiza el texto en una superficie
            IntPtr textSurface = SDL_ttf.TTF_RenderText_Solid(font, text, textColor);
            if (textSurface == IntPtr.Zero)
            {
                Console.WriteLine("Error al crear superficie de texto: " + SDL.SDL_GetError());
                return;
            }

            // Convierte la superficie en una textura
            IntPtr textTexture = SDL.SDL_CreateTextureFromSurface(Engine.renderer, textSurface);
            SDL.SDL_FreeSurface(textSurface);
            if (textTexture == IntPtr.Zero)
            {
                Console.WriteLine("Error al crear textura de texto: " + SDL.SDL_GetError());
                return;
            }

            int bd;
            uint c;
            SDL.SDL_QueryTexture(textTexture, out c, out bd, out int textWidth, out int textHeight);


            SDL.SDL_Rect destRect = new SDL.SDL_Rect { x = x, y = y, w = textWidth, h = textHeight };

            // Renderiza la textura en la pantalla
            SDL.SDL_RenderCopy(Engine.renderer, textTexture, IntPtr.Zero, ref destRect);

            // Libera la textura después de renderizar
            SDL.SDL_DestroyTexture(textTexture);

        }

        
        public static bool KeyPress(SDL.SDL_Keycode c)
        {
            SDL.SDL_PumpEvents();

            SDL.SDL_Scancode scancode = SDL.SDL_GetScancodeFromKey(c);

            IntPtr keysPtr = SDL.SDL_GetKeyboardState(out _);

            bool press = Marshal.ReadByte(keysPtr, (int)scancode) == 1;

            return press;
        }



        public static void Show()
        {
            SDL.SDL_RenderPresent(Engine.renderer);
        }
        public static void Clear()
        {
            // Sets the color that the screen will be cleared with.
            if (SDL.SDL_SetRenderDrawColor(Engine.renderer, 135, 206, 235, 255) < 0)
            {
                Console.WriteLine($"There was an issue with setting the render draw color. {SDL.SDL_GetError()}");
            }

            // Clears the current render surface.
            if (SDL.SDL_RenderClear(Engine.renderer) < 0)
            {
                Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
            }
        }
        public static void ErrorFatal(string texto)
        {
            System.Console.WriteLine(texto);
            Environment.Exit(1);
        }

        public static Font LoadFont(string imagePath, short size)
        {
            return new Font(imagePath, size);
        }

        public static IntPtr LoadFont2(string filename, int size)
        {
            // Carga la fuente con un tamaño de 24 puntos
            IntPtr font = SDL_ttf.TTF_OpenFont(filename, size);
            if (font == IntPtr.Zero)
            {
                Console.WriteLine("Error al cargar la fuente: " + SDL.SDL_GetError());
                Environment.Exit(6);
            }
            return font;
        }

        public static Image LoadImage(string imagePath)
        {
            return new Image(imagePath);
        }


        private static List<SDL.SDL_Event> eventQueue = new List<SDL.SDL_Event>();

        public static bool MouseClick(int button, out int mouseX, out int mouseY)
        {
            bool click = false;
            mouseX = 0;
            mouseY = 0;
            SDL.SDL_PumpEvents();
            SDL.SDL_Event e;

            // Procesar eventos no procesados de la cola
            for (int i = 0; i < eventQueue.Count; i++)
            {
                e = eventQueue[i];
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN && e.button.button == button)
                {
                    click = true;
                    mouseX = e.button.x;
                    mouseY = e.button.y;
                    eventQueue.RemoveAt(i);
                    return click;
                }
            }

            // Procesar nuevos eventos
            while (SDL.SDL_PollEvent(out e) != 0)
            {
                // Verifica si el evento es de tipo Mouse Button Down
                if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    // Verifica qué botón fue presionado
                    if (e.button.button == (byte)button) // Asegúrate de que 'button' sea del tipo correcto
                    {
                        click = true;
                        mouseX = e.button.x;
                        mouseY = e.button.y;
                        return click;
                    }
                    else
                    {
                        // Almacenar eventos de otros botones del mouse en la cola
                        eventQueue.Add(e);
                    }
                }
            }

            return click;
        }

        public static void Initialize(int w = 1024, int h = 768)
        {
            // Initilizes SDL.
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
            }

            // Create a new window given a title, size, and passes it a flag indicating it should be shown.
            window = SDL.SDL_CreateWindow("Mi juego", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, w, h, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            if (window == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
            }

            // Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
            renderer = SDL.SDL_CreateRenderer(window,
                                                    -1,
                                                    SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                                    SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
            }

            // Initilizes SDL_image for use with png files.
            if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
            {
                Console.WriteLine($"Hubo un error initilizing SDL2_Image {SDL_image.IMG_GetError()}");
            }

            // Initializes SDL_ttf
            if (SDL_ttf.TTF_Init() == -1)
            {
                Console.WriteLine("Error al inicializar SDL_ttf: " + SDL.SDL_GetError());
                return;
            }
            // Initializes audio mixer
            if (SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 2048) == -1)
            {
                Console.WriteLine("Error al inicializar SDL_mixer: " + SDL.SDL_GetError());
            }
        }

        public static float Radiands(int angle)
        {
            return (float) ((angle - 90) * Math.PI / 180.0);
        }

        public static void Draw(Image screenImg, int ximg, int yimg)
        {
            IntPtr screen = screenImg.Pointer;
            int textureWidth, textureHeight;
            int b;
            uint c;
            SDL.SDL_QueryTexture(screen, out c, out b, out textureWidth, out textureHeight);

            SDL.SDL_Rect destRect = new SDL.SDL_Rect

            {
                x = ximg, // Posición X en la pantalla
                y = yimg, // Posición Y en la pantalla
                w = textureWidth,  // Ancho de la textura
                h = textureHeight   // Alto de la textura
            };
            SDL.SDL_RenderCopy(Engine.renderer, screen, IntPtr.Zero, ref destRect);
        }

        public static void Draw(string screenImg, int ximg, int yimg)
        {
            Image image = LoadImage(screenImg);
            IntPtr screen = image.Pointer;
            int textureWidth, textureHeight;
            int b;
            uint c;
            SDL.SDL_QueryTexture(screen, out c, out b, out textureWidth, out textureHeight);

            SDL.SDL_Rect destRect = new SDL.SDL_Rect

            {
                x = ximg, // Posición X en la pantalla
                y = yimg, // Posición Y en la pantalla
                w = textureWidth,  // Ancho de la textura
                h = textureHeight   // Alto de la textura
            };
            SDL.SDL_RenderCopy(Engine.renderer, screen, IntPtr.Zero, ref destRect);
        }

        public static void Draw(string img, int x, int y, double angle)
        {
            Image image = LoadImage(img);

            IntPtr texture = image.Pointer;

            int w, h;
            int b;
            uint c;

            SDL.SDL_QueryTexture(texture, out c, out b, out w, out h);

            SDL.SDL_Rect dest = new SDL.SDL_Rect
            {
                x = x,
                y = y,
                w = w,
                h = h
            };

            // Centro de rotación (centro de la imagen)
            SDL.SDL_Point center = new SDL.SDL_Point
            {
                x = w / 2,
                y = h / 2
            };

            SDL.SDL_RenderCopyEx(
                renderer,
                texture,
                IntPtr.Zero,
                ref dest,
                angle,
                ref center,
                SDL.SDL_RendererFlip.SDL_FLIP_NONE
            );
        }

        public static void Draw(Image img, int x, int y, double angle)
        {
            IntPtr texture = img.Pointer;

            int w, h;
            int b;
            uint c;

            SDL.SDL_QueryTexture(texture, out c, out b, out w, out h);

            SDL.SDL_Rect dest = new SDL.SDL_Rect
            {
                x = x,
                y = y,
                w = w,
                h = h
            };

            // Centro de rotación (centro de la imagen)
            SDL.SDL_Point center = new SDL.SDL_Point
            {
                x = w / 2,
                y = h / 2
            };

            SDL.SDL_RenderCopyEx(
                renderer,
                texture,
                IntPtr.Zero,
                ref dest,
                angle,
                ref center,
                SDL.SDL_RendererFlip.SDL_FLIP_NONE
            );
        }
        
        public static void MousePosition(out int mouseX, out int mouseY)
        {
            SDL.SDL_GetMouseState(out mouseX, out mouseY);
        }
        
        public static double AngleToMouse(int originX, int originY)
        {
            SDL.SDL_GetMouseState(out int mouseX, out int mouseY);
            double angle = Math.Atan2(mouseY - originY, mouseX - originX) * (180.0 / Math.PI) + 90;
            return angle;
        }

        public static void Debug(string text)
        {
            Console.WriteLine(text);
        }


        // Definiciones de teclas
        public static SDL.SDL_Keycode KEY_ESC = SDL.SDL_Keycode.SDLK_ESCAPE;
        public static SDL.SDL_Keycode KEY_A = SDL.SDL_Keycode.SDLK_a;
        public static SDL.SDL_Keycode KEY_B = SDL.SDL_Keycode.SDLK_b;
        public static SDL.SDL_Keycode KEY_C = SDL.SDL_Keycode.SDLK_c;
        public static SDL.SDL_Keycode KEY_D = SDL.SDL_Keycode.SDLK_d;
        public static SDL.SDL_Keycode KEY_E = SDL.SDL_Keycode.SDLK_e;
        public static SDL.SDL_Keycode KEY_F = SDL.SDL_Keycode.SDLK_f;
        public static SDL.SDL_Keycode KEY_G = SDL.SDL_Keycode.SDLK_g;
        public static SDL.SDL_Keycode KEY_H = SDL.SDL_Keycode.SDLK_h;
        public static SDL.SDL_Keycode KEY_I = SDL.SDL_Keycode.SDLK_i;
        public static SDL.SDL_Keycode KEY_J = SDL.SDL_Keycode.SDLK_j;
        public static SDL.SDL_Keycode KEY_K = SDL.SDL_Keycode.SDLK_k;
        public static SDL.SDL_Keycode KEY_L = SDL.SDL_Keycode.SDLK_l;
        public static SDL.SDL_Keycode KEY_M = SDL.SDL_Keycode.SDLK_m;
        public static SDL.SDL_Keycode KEY_N = SDL.SDL_Keycode.SDLK_n;
        public static SDL.SDL_Keycode KEY_O = SDL.SDL_Keycode.SDLK_o;
        public static SDL.SDL_Keycode KEY_P = SDL.SDL_Keycode.SDLK_p;
        public static SDL.SDL_Keycode KEY_Q = SDL.SDL_Keycode.SDLK_q;
        public static SDL.SDL_Keycode KEY_R = SDL.SDL_Keycode.SDLK_r;
        public static SDL.SDL_Keycode KEY_S = SDL.SDL_Keycode.SDLK_s;
        public static SDL.SDL_Keycode KEY_T = SDL.SDL_Keycode.SDLK_t;
        public static SDL.SDL_Keycode KEY_U = SDL.SDL_Keycode.SDLK_u;
        public static SDL.SDL_Keycode KEY_V = SDL.SDL_Keycode.SDLK_v;
        public static SDL.SDL_Keycode KEY_W = SDL.SDL_Keycode.SDLK_w;
        public static SDL.SDL_Keycode KEY_X = SDL.SDL_Keycode.SDLK_x;
        public static SDL.SDL_Keycode KEY_Y = SDL.SDL_Keycode.SDLK_y;
        public static SDL.SDL_Keycode KEY_Z = SDL.SDL_Keycode.SDLK_z;

        public static SDL.SDL_Keycode KEY_0 = SDL.SDL_Keycode.SDLK_0;
        public static SDL.SDL_Keycode KEY_1 = SDL.SDL_Keycode.SDLK_1;
        public static SDL.SDL_Keycode KEY_2 = SDL.SDL_Keycode.SDLK_2;
        public static SDL.SDL_Keycode KEY_3 = SDL.SDL_Keycode.SDLK_3;
        public static SDL.SDL_Keycode KEY_4 = SDL.SDL_Keycode.SDLK_4;
        public static SDL.SDL_Keycode KEY_5 = SDL.SDL_Keycode.SDLK_5;
        public static SDL.SDL_Keycode KEY_6 = SDL.SDL_Keycode.SDLK_6;
        public static SDL.SDL_Keycode KEY_7 = SDL.SDL_Keycode.SDLK_7;
        public static SDL.SDL_Keycode KEY_8 = SDL.SDL_Keycode.SDLK_8;
        public static SDL.SDL_Keycode KEY_9 = SDL.SDL_Keycode.SDLK_9;

        public static SDL.SDL_Keycode KEY_ESP = SDL.SDL_Keycode.SDLK_SPACE;

        public static SDL.SDL_Keycode KEY_UP = SDL.SDL_Keycode.SDLK_UP;
        public static SDL.SDL_Keycode KEY_DOWN = SDL.SDL_Keycode.SDLK_DOWN;
        public static SDL.SDL_Keycode KEY_LEFT = SDL.SDL_Keycode.SDLK_LEFT;
        public static SDL.SDL_Keycode KEY_RIGHT = SDL.SDL_Keycode.SDLK_RIGHT;

        public static int MOUSE_LEFT = (int)SDL.SDL_BUTTON_LEFT;
        public static int MOUSE_RIGHT = (int)SDL.SDL_BUTTON_RIGHT;

        public static int MOUSE_MIDDLE = (int)SDL.SDL_BUTTON_MIDDLE;
    }


}