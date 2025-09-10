using UnityEngine;

namespace XCAPE.Core
{
    public static class PhysicsDefaults
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            // Gravedad estándar de Unity (-9.81 en Y)
            Physics.gravity = new Vector3(0, -9.81f, 0);

            // Estabilidad: solver y thresholds razonables para mobile
            Physics.defaultContactOffset = 0.01f; // contacto más preciso
            Physics.sleepThreshold = 0.005f;      // dormir más estable
            Physics.bounceThreshold = 2f;
            Physics.defaultSolverIterations = 12;      // más iteraciones para stacks de pinos
            Physics.defaultSolverVelocityIterations = 4;
        }
    }
}
