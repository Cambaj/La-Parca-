using System.Diagnostics;

namespace ProyectoSDL2.Engine.Scripts
{
    class Program
    {
        static float deltaTime;

        public static  float DeltaTime => deltaTime;

        static void Main(string[] args)
        {
            Engine.Initialize();
                        
            DateTime startTime = DateTime.Now;
            float currentTime;
            
            float lastFrameTime = 0;
  
            while (true)
            {
                currentTime = (float)(DateTime.Now - startTime).TotalSeconds; 
                deltaTime = currentTime - lastFrameTime; 
                lastFrameTime = currentTime; 

                Update();
                Render();
            }

        }

        static void Update()
        {
            

        }

        static void Render()
        {
            Engine.Clear();
            


            Engine.Show();
        }

       

    }

}

