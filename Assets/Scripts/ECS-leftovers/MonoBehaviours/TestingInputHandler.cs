using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TestingInputHandler : MonoBehaviour
{
    public void TestInput()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var em = world.EntityManager;

        var request = em.CreateEntity(typeof(TestingMessage));

        em.SetComponentData(request, new TestingMessage
        {
            Value = 42
        });
    }
}
