using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextFilterPrototype;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void OpenFile()
        {
            var expectedFileContent = "два";
            var expectedFileName = "2.txt";

            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(_ => _.ReadAllText(expectedFileName, It.IsAny<Encoding>()))
                .Returns(expectedFileContent)
                .Verifiable();

            var openFileDialog = new Mock<OpenFileDialog1>();
            openFileDialog.Setup(_ => _.ShowDialog()).Returns(true).Verifiable();
            openFileDialog.Setup(_ => _.FileName).Returns(expectedFileName).Verifiable();

            var sut = new Form1(openFileDialog.Object, fileSystem.Object);


            //Act
            var actual = sut.OpenTextFile();

            //Assert
            fileSystem.Verify();
            openFileDialog.Verify();
            Assert.AreEqual(expectedFileContent, actual.Item1);
            Assert.AreEqual(expectedFileName, actual.Item2);
        }
    }
}
