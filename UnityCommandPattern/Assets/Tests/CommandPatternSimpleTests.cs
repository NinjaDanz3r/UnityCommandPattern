using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CommandPattern;
using System;

namespace Tests
{
    public class CommandPatternSimpleTests
    {
        private class TestClass
        {
            public int number = 0;

            public void IncreaseNumber()
            {
                number++;
            }

            public void DecreaseNumber()
            {
                number--;
            }

            public void SetNumber(int number)
            {
                this.number = number;
            }
        }

        private static class TestHelper
        {
            public static List<ICommand> GenerateMixedCommands<T>(int n, T target, Action<T> execute, Action<T> undo)
            {
                var commands = new List<ICommand>();
                for (int i = 0; i < 10; i++)
                {
                    if (i % 2 == 0)
                        commands.Add(new SerializableCommand<T>(target, execute));
                    else
                        commands.Add(new SerializableReversibleCommand<T>(target, execute, undo));
                }
                return commands;
            }
        }

        [Test]
        public void TestExecute()
        {
            var testClass = new TestClass();
            var command = new SerializableCommand<TestClass>(testClass, t => t.IncreaseNumber());
            command.Execute();
            Assert.AreEqual(1, testClass.number);
        }

        [Test]
        public void TestExecuteAndUndo()
        {
            var testClass = new TestClass();
            var command = new SerializableReversibleCommand<TestClass>(testClass, t => t.IncreaseNumber(), t => t.DecreaseNumber());
            command.Execute();
            command.UndoExecute();
            Assert.AreEqual(0, testClass.number);
        }

        [Test]
        public void TestExecuteAndUndoAdvanced()
        {
            var testClass = new TestClass();
            testClass.number = 10;

            // Please note that we copy the old number, otherwise if we would set the undo action as following:
            // t => t.SetNumber(t.number) will actually call SetNumber with a reference to the t class!
            int oldNumber = testClass.number;
            var command = new SerializableReversibleCommand<TestClass>(testClass, t => t.SetNumber(77), t => t.SetNumber(oldNumber));
            Assert.AreEqual(10, testClass.number);

            command.Execute();
            Assert.AreEqual(77, testClass.number);

            command.UndoExecute();
            Assert.AreEqual(10, testClass.number);
        }

        [Test]
        public void TestMixMatchExecute()
        {
            var testClass = new TestClass();
            var commands = TestHelper.GenerateMixedCommands(10, testClass, t => t.IncreaseNumber(), t => t.DecreaseNumber());

            foreach (var command in commands)
            {
                command.Execute();
            }
            Assert.AreEqual(10, testClass.number);
        }

        [Test]
        public void TestMixMatchExecuteUndo()
        {
            var testClass = new TestClass();
            var commands = TestHelper.GenerateMixedCommands(10, testClass, t => t.IncreaseNumber(), t => t.DecreaseNumber());

            foreach (var command in commands)
            {
                command.Execute();
                if (command is IReversibleCommand)
                    (command as IReversibleCommand).UndoExecute();
            }

            Assert.AreEqual(5, testClass.number);
        }
    }
}
