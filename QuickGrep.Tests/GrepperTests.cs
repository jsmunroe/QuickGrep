using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickGrep.Tests
{
    [TestClass]
    public class GrepperTests
    {
        private string rootPath = @"..\..\Tests\GrepperTests";

        [TestMethod]
        public void Construct()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Default.txt");

            // Execute
            var result = new Grepper(testPath);

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void ConstructWithNullPath()
        {
            // Execute
            var result = new Grepper(null);
        }

        [TestMethod]
        public void FindInFile()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Default.txt");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.Find("Media", matchCase: true);

            var resultLines = results.Select(p => p.Line).ToArray();
            var resultColumns = results.Select(p => p.Column).ToArray();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(10, results.Count());
            CollectionAssert.AreEqual(new[] {7, 7, 20, 20, 25, 25, 30, 30, 35, 35}, resultLines);
            CollectionAssert.AreEqual(new[] {4, 40, 4, 40, 4, 40, 4, 40, 4, 40}, resultColumns);
        }

        [TestMethod]
        public void FindInFileWhenNotExists()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Default.txt");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.Find("meow meow", matchCase: true);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public void FindInFileCaseInsensitively()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Default.txt");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.Find("media");

            var resultLines = results.Select(p => p.Line).ToArray();
            var resultColumns = results.Select(p => p.Column).ToArray();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(10, results.Count());
            CollectionAssert.AreEqual(new[] { 7, 7, 20, 20, 25, 25, 30, 30, 35, 35 }, resultLines);
            CollectionAssert.AreEqual(new[] { 4, 40, 4, 40, 4, 40, 4, 40, 4, 40 }, resultColumns);
        }

        [TestMethod]
        public void FindInFileMatchWholeWord()
        {
            // Setup
            var testPath = Path.Combine(rootPath, @"Directory\File1.txt");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.Find("in", matchWholeWord:true);

            var firstResult = results.FirstOrDefault();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(1, firstResult.Line);
            Assert.AreEqual(9, firstResult.Column);
        }



        [TestMethod]
        public void FindPatternInFile()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Default.txt");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.FindPattern(@"(\d+\.){3}(\d+)");

            var resultLines = results.Select(p => p.Line).ToArray();
            var resultColumns = results.Select(p => p.Column).ToArray();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count());
            CollectionAssert.AreEqual(new[] { 14, 15, 16 }, resultLines);
            CollectionAssert.AreEqual(new[] { 40, 40, 40 }, resultColumns);
        }

        [TestMethod]
        public void FindInDirectory()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Directory");
            var grepper = new Grepper(testPath);

            // Execute
            var results = grepper.Find("<DIR>");

            var resultFileNames = results.Select(p => p.FileName).Distinct().ToArray();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(20, results.Count());
            CollectionAssert.AreEquivalent(new[] { "File1.txt", "File2.txt", "File3.txt" }, resultFileNames);
        }

        [TestMethod]
        public void FindInDirectoryRecursively()
        {
            // Setup
            var testPath = Path.Combine(rootPath, "Directory");
            var grepper = new Grepper(testPath, isRecursive:true);

            // Execute
            var results = grepper.Find("<DIR>");

            var resultFileNames = results.Select(p => p.FileName).Distinct().ToArray();

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(40, results.Count());
            CollectionAssert.AreEquivalent(new[] { "File1.txt", "File2.txt", "File3.txt", "File4.txt", "File5.txt", "File6.txt" }, resultFileNames);
        }



    }
}
