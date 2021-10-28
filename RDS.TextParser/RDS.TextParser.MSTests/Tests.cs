using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RDS.TextParser.MSTests
{
    [TestClass]
    public class RDSTextParserTests
    {
        private string _tokens = string.Empty;
        private string _source = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            _tokens = File.ReadAllText("Configs/Get1.tokens.txt");
            _source = File.ReadAllText("Sources/Get1.source.txt");
        }

        [TestMethod]
        public void Extract1AsArray()
        {
            // Arrange
            var expected = "34.63 %";

            // Act
            var result = new RDSTextParser().GetResult(_tokens, _source);

            // Assert
            Assert.IsTrue(result[0][1].ToString().Equals(expected, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void Extract1AsDictionary()
        {
            // Arrange
            var key = "InstitutionalOwnership";
            var expected = "34.63 %";

            // Act
            var result = new RDSTextParser().GetResultDictionary(_tokens, _source);

            // Assert
            Assert.IsTrue(result[key].ToString().Equals(expected, StringComparison.OrdinalIgnoreCase));
        }
    }
}
