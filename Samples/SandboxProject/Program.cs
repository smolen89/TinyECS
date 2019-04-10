﻿using System;
using TinyECS.Impls;
using TinyECS.Interfaces;


namespace SandboxProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWorldContext worldContext = new WorldContext(new EntityManager(new ComponentManager()));
            
            ISystemManager systemManager = new SystemManager(worldContext);

            systemManager.Init();
            systemManager.Update(0.0f);

            Console.ReadKey();
        }
    }
}
