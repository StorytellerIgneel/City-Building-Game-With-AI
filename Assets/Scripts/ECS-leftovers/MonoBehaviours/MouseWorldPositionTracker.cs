

using Unity.Entities;
using UnityEngine;

// This MonoBehaviour is responsible for tracking the mouse position in world space and writing it to an ECS component.
// Basically a bridge between the world and the ecs world for the mouse position specifically. 
public class MouseWorldPositionTracker : MonoBehaviour
{
    EntityManager em;
    Entity mouseEntity;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        mouseEntity = em.CreateEntity(typeof(MouseWorldPosition));
    }

    private void Update()
    {
        var mousePositionOnScreen = Input.mousePosition;
        var world = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);

        em.SetComponentData(mouseEntity, new MouseWorldPosition{
            Value = world
        });
    }
}