using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CommandPattern;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tests
{
    public class SingletonCommandStrategyTests
    {
        #region Private classes
        /// <summary>
        /// Test class representing a game's state, as a singleton.
        /// </summary>
        [Serializable]
        private class TestGameState
        {
            public int numberOfEnemies = 0;
            public int numberOfPlayers = 0;
            private static TestGameState instance = null;

            private TestGameState() { }

            public static void ResetState()
            {
                Instance.numberOfEnemies = 0;
                Instance.numberOfPlayers = 0;
            }

            public static TestGameState Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TestGameState();
                    }
                    return instance;
                }
            }
        }

        /// <summary>
        /// Class that operates on the game state.
        /// </summary>
        [Serializable]
        private class GameStateChanger
        {
            public void IncreaseNumberOfEnemies()
            {
                Debug.Log("Increased number of enemies");
                TestGameState.Instance.numberOfEnemies++;
            }

            public void SetNumberOfPlayers(int number)
            {
                TestGameState.Instance.numberOfPlayers = number;
            }
        }

        /// <summary>
        /// Class that through a method call operates on the game state.
        /// </summary>
        [Serializable]
        private class MethodGameStateChanger
        {
            TestGameState state;
            int numberOfPlayers;
            int numberOfEnemies;

            public void SetPlayersAndEnemies(TestGameState state, int numberOfPlayers, int numberOfEnemies)
            {
                state.numberOfPlayers = numberOfPlayers;
                state.numberOfEnemies = numberOfEnemies;
            }
        }

        /// <summary>
        /// A simple helper class that helps us avoid DRY code.
        /// </summary>
        private static class TestHelper
        {
            public static ICommand GeneratePlayerAndEnemiesCommand()
            {
                var methodGameStateChanger = new MethodGameStateChanger();
                return new SerializableCommand<MethodGameStateChanger>
                (methodGameStateChanger, changer => changer.SetPlayersAndEnemies(TestGameState.Instance, 1, 2));
            }
        }

        #endregion

        [SetUp]
        public void BaseSetup()
        {
            TestGameState.ResetState();
        }

        [Test]
        public void TestOperateOnState()
        {
            var gameStateChanger = new GameStateChanger();
            var increaseNumberOfEnemiesCommand = new SerializableCommand<GameStateChanger>
                ( gameStateChanger, changer => changer.IncreaseNumberOfEnemies());

            increaseNumberOfEnemiesCommand.Execute();

            Assert.AreEqual(1, TestGameState.Instance.numberOfEnemies);
            Assert.AreEqual(0, TestGameState.Instance.numberOfPlayers);
        }

        [Test]
        public void TestOperateOnStateThroughMethod()
        {
            var methodGameStateChanger = new MethodGameStateChanger();

            var setPlayersAndEnemies = TestHelper.GeneratePlayerAndEnemiesCommand();

            setPlayersAndEnemies.Execute();

            Assert.AreEqual(1, TestGameState.Instance.numberOfPlayers);
            Assert.AreEqual(2, TestGameState.Instance.numberOfEnemies);
        }

        [Test]
        public void TestList()
        {
            var listOfCommands = new List<ICommand>();

            var methodGameStateChanger = new MethodGameStateChanger();
            var setPlayersAndEnemies = TestHelper.GeneratePlayerAndEnemiesCommand();
            listOfCommands.Add(setPlayersAndEnemies);

            var gameStateChanger = new GameStateChanger();
            var increaseNumberOfEnemies = new SerializableCommand<GameStateChanger>
                (gameStateChanger, changer => changer.IncreaseNumberOfEnemies());
            listOfCommands.Add(increaseNumberOfEnemies);

            foreach(var command in listOfCommands)
            {
                command.Execute();
            }

            Assert.AreEqual(1, TestGameState.Instance.numberOfPlayers);
            Assert.AreEqual(3, TestGameState.Instance.numberOfEnemies);
        }

        [Test]
        public void TestSerialization()
        {
            var listOfCommands = new List<ICommand>();

            var methodGameStateChanger = new MethodGameStateChanger();
            var setPlayersAndEnemies = TestHelper.GeneratePlayerAndEnemiesCommand();
            listOfCommands.Add(setPlayersAndEnemies);

            var gameStateChanger = new GameStateChanger();
            var increaseNumberOfEnemies = new SerializableCommand<GameStateChanger>
                (gameStateChanger, changer => changer.IncreaseNumberOfEnemies());
            listOfCommands.Add(increaseNumberOfEnemies);


            byte[] bytes = new byte[0];

            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, listOfCommands);
                bytes = memoryStream.ToArray();
            }

            var newListOfCommands = new List<ICommand>();

            using (var memoryStream = new MemoryStream(bytes, 0, bytes.Length))
            {
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Position = 0;
                var data = new BinaryFormatter().Deserialize(memoryStream);
                newListOfCommands = data as List<ICommand>;
            }

            foreach (var command in newListOfCommands)
            {
                command.Execute();
            }
            
            Assert.AreEqual(1, TestGameState.Instance.numberOfPlayers);
            Assert.AreEqual(3, TestGameState.Instance.numberOfEnemies);
        }
    }
}
