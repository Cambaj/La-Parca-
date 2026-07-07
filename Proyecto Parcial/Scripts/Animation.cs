
namespace ProyectoSDL2.Engine.Scripts
{
    internal class Animation
    {
        List<Image> textures = new List<Image>();
        private int currentFrameIndex = 0;
        private float speed = 0.5f;
        private float currentAnimationTime = 0;

        public Image currentFrame => textures[currentFrameIndex];

        public Animation(List<Image> images, float newSpeed)
        {
            textures = images;
            speed = newSpeed;
        }

        public void Update()
        {
            currentAnimationTime += Program.DeltaTime;

            if (currentAnimationTime >= speed)
            {
                currentFrameIndex++;
                currentAnimationTime = 0;

                if (currentFrameIndex >= textures.Count)
                {
                    currentFrameIndex = 0;
                }

            }
        }

    }
}
