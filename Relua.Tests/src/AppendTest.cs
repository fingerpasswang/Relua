using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Relua.Tests
{
    [TestFixture]
    public class AppendTest
    {
        [Test]
        public void FunctionCallTableAccess()
        {
            var tokenizer = new Tokenizer("local NeedExp = GrowthConfig.GetConfig(M.bindData.LvFrom+1).LingExp");
            var parser = new Parser(tokenizer);

            var stat = parser.ReadStatement(); // only reads one statement!
            Assert.AreEqual("local NeedExp = GrowthConfig.GetConfig((M.bindData.LvFrom + 1)).LingExp", stat.ToString());
        }

        [Test]
        public void FunctionCallAssignment()
        {
            var tokenizer = new Tokenizer("ClientToGameDelegate:RequestSpiritLevelUpPreviewData(1).Callback = nil");
            var parser = new Parser(tokenizer);

            var stat = parser.ReadStatement(); // only reads one statement!
        }

        [Test]
        public void AssignmentFunction()
        {
            var tokenizer = new Tokenizer("cfg.InteractionRequirements = function (...) \n end");
            var parser = new Parser(tokenizer);

            var stat = parser.ReadStatement(); // only reads one statement!
        }

        [Test]
        public void Label()
        {
            var tokenizer = new Tokenizer("::IL_004c::");
            var parser = new Parser(tokenizer);

            var stat = parser.ReadStatement(); // only reads one statement!
        }

        [Test]
        public void ForStatement()
        {
            var tokenizer = new Tokenizer("for i, v in list.next, self, self do;end");
            var parser = new Parser(tokenizer);
            var stat = parser.ReadStatement() as AST.Node;
            Assert.AreEqual("for i, v in list.next, self, self do;end", stat.ToString(one_line: true));
        }

        [Test]
        public void ComplexIfStatement()
        {
            var tokenizer = new Tokenizer("if 1 < 2 then \n return \n elseif 2 < 3 then -- item \n return \n end");
            var parser = new Parser(tokenizer);
            var stat = parser.ReadStatement() as AST.Node;
            //Assert.AreEqual("goto next", stat.ToString(one_line: true));
        }

        [Test]
        public void ComplexIfStatement2()
        {
            var tokenizer = new Tokenizer("if 1 < 2 then \n return ; \n end");
            var parser = new Parser(tokenizer);
            var stat = parser.ReadStatement() as AST.Node;
            //Assert.AreEqual("goto next", stat.ToString(one_line: true));
        }

        [Test]
        public void GoToStatement()
        {
            var tokenizer = new Tokenizer("goto next");
            var parser = new Parser(tokenizer);
            var stat = parser.ReadStatement() as AST.Node;
            Assert.AreEqual("goto next", stat.ToString(one_line: true));
        }

        [Test]
        public void DecimalLiteral()
        {
            {
                var tokenizer = new Tokenizer("4.57e-3");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(4.57e-3.ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0.3e12");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(0.3e12.ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("5E+20");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(5E+20.ToString(), expr.ToString(one_line: true));
            }
        }

        [Test]
        public void HexLiteral()
        {
            {
                var tokenizer = new Tokenizer("0x123");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexInteger("0x123").ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0xff");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexInteger("0xff").ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0x1A3");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexInteger("0x1A3").ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0x0.2");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexFloat("0x0.2").ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0x1p-1");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexFloat("0x1p-1").ToString(), expr.ToString(one_line: true));
            }

            {
                var tokenizer = new Tokenizer("0xa.bp2");
                var parser = new Parser(tokenizer);
                var expr = parser.ReadExpression() as AST.Node;
                Assert.AreEqual(Extensions.ParseHexFloat("0xa.bp2").ToString(), expr.ToString(one_line: true));
            }
        }

        [Test]
        public void ContinuousAssignment()
        {
            var str = "M.bindData.DeltaExp = \"-\" \n M.bindData.Cost = 0";

            var tokenizer = new Tokenizer(str);
            var parser = new Parser(tokenizer);

            var stat1 = parser.ReadStatement(); // only reads one statement!
            var stat2 = parser.ReadStatement();

            Assert.AreEqual("M.bindData.DeltaExp = \"-\"", stat1.ToString());
            Assert.AreEqual("M.bindData.Cost = 0", stat2.ToString());
        }
    }
}
