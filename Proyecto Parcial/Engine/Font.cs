using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSDL2.Engine
{
    class Font
    {
        // Atributos

        
        public IntPtr pointer { get; private set; }

        // Operaciones

        /// Constructor a partir de un nombre de fichero y un tamaño
        public Font(string fileName, short size)
        {
            Load(fileName, size);
        }

        public void Load(string fileName, short size)
        {
            pointer = Engine.LoadFont2(fileName, size);
            if (pointer == IntPtr.Zero)
                Engine.ErrorFatal("Fuente inexistente: " + fileName);
        }

        public IntPtr ReadPointer()
        {
            return pointer;
        }

    }
}
