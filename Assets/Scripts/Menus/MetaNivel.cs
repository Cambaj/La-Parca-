using UnityEngine;

public class MetaNivel : MonoBehaviour
{
    [Header("Configuración de Progreso")]
    [Tooltip("Escribe el nombre exacto del reino (Ej: Hambruna, Guerra, Peste, Muerte)")]
    public string nombreReino;

    [Tooltip("¿Qué número de nivel se debe desbloquear en el menú al tocar esta meta? (Ej: Si pasaste el 1, aquí pones 2)")]
    public int numeroNivelALiberar;
}
